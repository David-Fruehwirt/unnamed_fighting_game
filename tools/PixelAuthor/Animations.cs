using System.Text.Json;
using System.Text.Json.Nodes;

namespace PixelAuthor;

public static partial class Program
{
    static readonly string[] LayerOrder = [
        "backpack", "far_upper_arm", "far_forearm", "far_hand",
        "far_thigh", "far_shin", "far_foot", "near_thigh", "near_shin", "near_foot",
        "pelvis", "torso", "head", "near_upper_arm", "near_forearm", "near_hand"
    ];
    record Placement(P Start, P? End = null);
    record PoseFrame(string Id, string Clip, int Ticks, Dictionary<string, Placement> Parts);

    static Dictionary<string, Placement> IdlePose(int bob = 0)
    {
        return new() {
            ["pelvis"] = new(new(64, 67 + bob)), ["torso"] = new(new(64, 63 + bob)),
            ["head"] = new(new(66, 35 + bob)), ["backpack"] = new(new(53, 47 + bob)),
            ["near_upper_arm"] = new(new(55, 44 + bob), new(49, 61 + bob)),
            ["near_forearm"] = new(new(49, 61 + bob), new(52, 76 + bob)),
            ["near_hand"] = new(new(52, 76 + bob)),
            ["far_upper_arm"] = new(new(74, 44 + bob), new(78, 60 + bob)),
            ["far_forearm"] = new(new(78, 60 + bob), new(80, 75 + bob)),
            ["far_hand"] = new(new(80, 75 + bob)),
            ["near_thigh"] = new(new(59, 70 + bob), new(56, 91)),
            ["near_shin"] = new(new(56, 91), new(53, 112)),
            ["near_foot"] = new(new(53, 112)),
            ["far_thigh"] = new(new(71, 70 + bob), new(74, 91)),
            ["far_shin"] = new(new(74, 91), new(77, 112)),
            ["far_foot"] = new(new(77, 112))
        };
    }

    static void SetLimb(Dictionary<string, Placement> pose, string side, string upper, string lower,
        string tip, P root, P joint, P end)
    {
        pose[side + "_" + upper] = new(root, joint);
        pose[side + "_" + lower] = new(joint, end);
        pose[side + "_" + tip] = new(end);
    }

    static List<PoseFrame> Poses()
    {
        var result = new List<PoseFrame>();
        int[] idleBob = [0, -1, 0, 1];
        for (int i = 0; i < 4; i++)
            result.Add(new($"idle_{i}", "idle", 15, IdlePose(idleBob[i])));
        // Explicit eight-phase run cycle: contact, down, passing, lift, opposite contact.
        P[] knees = [new(76,90),new(68,92),new(57,90),new(48,85),
                     new(51,82),new(57,80),new(68,82),new(76,85)];
        P[] ankles = [new(85,112),new(70,112),new(54,112),new(42,104),
                      new(39,96),new(45,91),new(61,97),new(78,105)];
        int[] bob = [0, 2, 0, -3, 0, 2, 0, -3];
        for (int i = 0; i < 8; i++)
        {
            var pose = IdlePose(bob[i]);
            pose["torso"] = new(new(68, 62 + bob[i]));
            pose["head"] = new(new(72, 35 + bob[i]));
            pose["backpack"] = new(new(55, 47 + bob[i]));
            SetLimb(pose, "near", "thigh", "shin", "foot", new(59,70+bob[i]), knees[i], ankles[i]);
            int opposite = (i + 4) % 8;
            SetLimb(pose, "far", "thigh", "shin", "foot", new(70,70+bob[i]), knees[opposite], ankles[opposite]);
            int swing = new[] { -10,-5,2,9,12,5,-2,-9 }[i];
            SetLimb(pose,"near","upper_arm","forearm","hand",new(58,44+bob[i]),
                new(57+swing,60+bob[i]),new(67+swing,67+bob[i]));
            SetLimb(pose,"far","upper_arm","forearm","hand",new(76,44+bob[i]),
                new(73-swing,57+bob[i]),new(82-swing,65+bob[i]));
            result.Add(new($"run_{i}", "run", 5, pose));
        }
        foreach (string clip in new[] { "jump", "fall" })
        for (int i = 0; i < 3; i++)
        {
            bool jump = clip == "jump";
            var pose = IdlePose(-2);
            pose["head"] = new(new(68,33));
            SetLimb(pose,"near","thigh","shin","foot",new(60,68),
                jump ? new(76,80-i) : new(65,88+i),
                jump ? new(67,98-i*2) : new(57,110+i));
            SetLimb(pose,"far","thigh","shin","foot",new(70,68),
                jump ? new(54,83+i) : new(79,86+i),
                jump ? new(41,99+i) : new(86,106+i));
            SetLimb(pose,"near","upper_arm","forearm","hand",new(55,43),
                new(43,55+i), new(52,65+i));
            SetLimb(pose,"far","upper_arm","forearm","hand",new(75,43),
                new(84,54+i), new(93,63+i));
            result.Add(new($"{clip}_{i}",clip,6,pose));
        }
        return result;
    }

    static Canvas DrawPlaced(PartDefinition definition, Placement placement)
    {
        var canvas = new Canvas(128,128);
        var a = definition.Anchor;
        if (placement.End is not P end)
        {
            Draw(canvas, definition, p => new(p.X-a[0]+placement.Start.X,p.Y-a[1]+placement.Start.Y));
            return canvas;
        }
        // Re-rasterize vector outlines directly at integer vertices for each authored pose.
        // No bitmap is rotated/resampled. Segment width stays fixed as the limb bends.
        double length = definition.End[1] - a[1];
        double dx = end.X-placement.Start.X, dy = end.Y-placement.Start.Y;
        double targetLength = Math.Sqrt(dx*dx+dy*dy);
        Draw(canvas, definition, p => {
            double along = (p.Y-a[1])/length, across = p.X-a[0];
            return new P((int)Math.Round(placement.Start.X+along*dx+across*dy/targetLength),
                (int)Math.Round(placement.Start.Y+along*dy-across*dx/targetLength));
        });
        return canvas;
    }

    static string ProjectHash(string file)
    {
        var compact = JsonNode.Parse(File.ReadAllText(file))!.ToJsonString(new JsonSerializerOptions {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(compact))).ToLowerInvariant();
    }

    static void BuildAnimations()
    {
        string file = "art/soldier.pixel.json";
        if (File.Exists(file) && JsonNode.Parse(File.ReadAllText(file))!["characters"]![0]!["animations"]!.AsArray().Count > 0)
            throw new Exception("Animation source already exists; edit it through pix operations.");
        var poses = Poses();
        var definitions = LayerOrder.ToDictionary(id=>id,id=>JsonSerializer.Deserialize<PartDefinition>(
            File.ReadAllText($"art/draw/{id}.json"))!);
        foreach (string id in LayerOrder) RunPix("inspect",$"art/parts/{id}.pixel.json","--json");
        var canvases = poses.ToDictionary(p=>p.Id,p=>LayerOrder.ToDictionary(id=>id,
            id=>DrawPlaced(definitions[id],p.Parts[id])));
        // Retain full-size grids: the pixel framework requires every cel to match the canvas.
        var regions = new Dictionary<string,(int X,int Y,int W,int H)>();
        foreach (string id in LayerOrder)
        {
            int minX=128,minY=128,maxX=0,maxY=0;
            foreach (var pose in poses)
            {
                var c=canvases[pose.Id][id];
                for(int y=0;y<128;y++) for(int x=0;x<128;x++)
                    if(c.Cells[y*128+x]!=null) { minX=Math.Min(minX,x);minY=Math.Min(minY,y);maxX=Math.Max(maxX,x);maxY=Math.Max(maxY,y); }
            }
            regions[id]=(minX,minY,maxX-minX+1,maxY-minY+1);
        }
        var project=Project("soldier",128,128,LayerOrder);
        var character=project["characters"]![0]!;
        character["origin"]=JsonSerializer.SerializeToNode(new{x=64,y=121});
        character["pivot"]=JsonSerializer.SerializeToNode(new{x=64,y=121});
        var cels=character["views"]![0]!["frames"]![0]!["cels"]!;
        foreach(string id in LayerOrder)
        {
            var r=regions[id];
            cels[id]=JsonSerializer.SerializeToNode(new{
                grid=new{width=128,height=128,cells=canvases["idle_0"][id].Cells},
                offset=new{x=0,y=0}
            });
        }
        Save(file,project);
        RunPix("inspect",file,"--json");
        var idlePlan = new {
            characterId="soldier",viewId="right",sourceFrameId="neutral",
            animation=new{id="idle",name="idle",loop=true,tags=new[]{"frame-by-frame"}},
            frames=poses.Where(p=>p.Clip=="idle").Select((p,i)=>new{
                id=p.Id,name=p.Id,durationTicks=p.Ticks,
                moves=new[]{"head","torso","backpack","near_upper_arm","near_forearm","near_hand","far_upper_arm","far_forearm","far_hand"}
                    .Select(id=>new{partId=id,dx=0,dy=new[]{0,-1,0,1}[i]}).ToArray()
            }).ToArray()
        };
        Save("art/work/idle.plan.json",idlePlan);
        Console.WriteLine(RunPix("animate",file,"--plan","art/work/idle.plan.json","--expected-hash",ProjectHash(file)));
        foreach(string clip in new[]{"run","jump","fall"})
        {
            var operations=new List<object>();
            foreach(var pose in poses.Where(p=>p.Clip==clip))
            {
                operations.Add(new{type="addFrame",characterId="soldier",viewId="right",
                    frame=new{id=pose.Id,name=pose.Id,durationTicks=pose.Ticks,
                        cels=LayerOrder.ToDictionary(id=>id,id=>new{
                            grid=new{width=128,height=128,cells=canvases[pose.Id][id].Cells},
                            offset=new{x=0,y=0}
                        })
                    }
                });
            }
            operations.Add(new{type="addAnimation",characterId="soldier",
                animation=new{id=clip,name=clip,viewId="right",loop=clip=="run",tags=new[]{"frame-by-frame"},
                    frames=poses.Where(p=>p.Clip==clip).Select(p=>new{frameId=p.Id,durationTicks=p.Ticks}).ToArray()}});
            string opFile=$"art/work/{clip}.operations.json";
            Save(opFile,operations);
            Console.WriteLine(RunPix("apply",file,"--operations",opFile,"--expected-hash",ProjectHash(file)));
        }
        Console.WriteLine(RunPix("validate",file,"--json"));
        Directory.CreateDirectory("art/exports/animations");
        Directory.CreateDirectory("art/previews");
        foreach(string clip in new[]{"idle","run","jump","fall"})
        {
            RunPix("sheet",file,"--animation",clip,"--layout","horizontal",
                "--out",$"art/exports/animations/{clip}.png","--manifest",$"art/exports/animations/{clip}.json");
            RunPix("gif",file,"--animation",clip,"--scale","4","--out",$"art/previews/{clip}.gif");
        }
        RunPix("render",file,"--frame","idle_0","--scale","4","--out","art/previews/idle.png");
        RunPix("render",file,"--frame","run_0","--scale","4","--out","art/previews/run.png");
        // Export all layers independently with the same frame index and canvas origin.
        Directory.CreateDirectory("art/exports/layers");
        var layers=character["layers"]!.AsArray();
        var authored=JsonNode.Parse(File.ReadAllText(file))!;
        var authoredCharacter=authored["characters"]![0]!;
        authoredCharacter["animations"]!.AsArray().Add(JsonSerializer.SerializeToNode(new{
            id="all",name="All frames",viewId="right",loop=false,tags=Array.Empty<string>(),
            frames=poses.Select(p=>new{frameId=p.Id,durationTicks=p.Ticks}).ToArray()
        }));
        foreach(string id in LayerOrder)
        {
            foreach(var layer in authoredCharacter["layers"]!.AsArray())
                layer!["visible"]=layer["id"]!.GetValue<string>()==id;
            string temp=$"art/work/layer-{id}.pixel.json";
            Save(temp,authored);
            RunPix("sheet",temp,"--animation","all","--layout","horizontal",
                "--out",$"art/exports/layers/{id}.png");
        }
        Save("art/poses.json",new{canvas=new{width=128,height=128},origin=new{x=64,y=121},
            frames=poses.Select(p=>new{id=p.Id,clip=p.Clip,ticks=p.Ticks,parts=p.Parts}).ToArray()});
        // Compact this large derived assembly; the sixteen editable part sources remain formatted.
        File.WriteAllText(file,JsonNode.Parse(File.ReadAllText(file))!.ToJsonString()+"\n");
    }
}
