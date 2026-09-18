using System.Text.Json;
using System.Text.Json.Nodes;

namespace PixelAuthor;

public static partial class Program
{
    static double Length(P a,P b)=>Math.Sqrt(Math.Pow(b.X-a.X,2)+Math.Pow(b.Y-a.Y,2));
    static P Along(P root,P a,P b,double length)
    {
        double scale=length/Length(a,b);
        return new((int)Math.Round(root.X+(b.X-a.X)*scale),(int)Math.Round(root.Y+(b.Y-a.Y)*scale));
    }

    static List<PoseFrame> WalkPoses(Dictionary<string,PartDefinition> definitions)
    {
        var idle=JsonSerializer.Deserialize<Dictionary<string,Placement>>(
            JsonNode.Parse(File.ReadAllText("art/poses.json"))!["frames"]![0]!["parts"]!.ToJsonString())!;
        // Traced in original GIF pixels, top-right eight-frame demonstration.
        P[] hips=[new(790,320),new(790,324),new(792,308),new(800,316),
                  new(800,320),new(800,324),new(800,308),new(792,316)];
        P[] knees=[new(788,399),new(811,421),new(839,391),new(844,402),
                   new(839,408),new(821,416),new(808,411),new(768,420)];
        P[] ankles=[new(729,496),new(739,464),new(779,474),new(864,490),
                    new(864,494),new(828,498),new(799,498),new(768,496)];
        P[] toes=[new(752,508),new(749,492),new(778,494),new(894,490),
                  new(894,498),new(856,510),new(823,510),new(796,510)];
        P[] shoulders=[new(796,220),new(796,224),new(788,208),new(796,216),
                       new(796,220),new(796,224),new(788,208),new(796,216)];
        P[] elbows=[new(818,283),new(802,294),new(788,278),new(778,282),
                    new(778,286),new(792,292),new(810,280),new(820,276)];
        P[] wrists=[new(860,330),new(818,341),new(792,332),new(780,340),
                    new(780,344),new(802,340),new(832,328),new(864,320)];
        var poses=new List<PoseFrame>();var traces=new List<object>();
        for(int i=0;i<8;i++)
        {
            var p=idle.ToDictionary(x=>x.Key,x=>x.Value);
            foreach(string side in new[]{"near","far"})
            {
                int phase=side=="near"?i:(i+4)%8;
                void Fit(string upper,string lower,string tip,P a,P b,P c)
                {
                    var u=idle[side+"_"+upper];var l=idle[side+"_"+lower];
                    // In the reference's side view the hip joints overlap in projection.
                    // Retain their idle height and each segment's length, not the idle's
                    // wide guard projection (which would make alternate strides unequal).
                    P root=upper=="thigh"?new(side=="near"?64:65,u.Start.Y):u.Start;
                    P joint=Along(root,a,b,Length(u.Start,u.End!));
                    P end=Along(joint,b,c,Length(l.Start,l.End!));
                    SetLimb(p,side,upper,lower,tip,root,joint,end);
                }
                Fit("thigh","shin","foot",hips[phase],knees[phase],ankles[phase]);
                Fit("upper_arm","forearm","hand",shoulders[phase],elbows[phase],wrists[phase]);
                var foot=p[side+"_foot"].Start;
                p[side+"_foot"]=new(foot,new(foot.X+toes[phase].X-ankles[phase].X,
                    foot.Y+toes[phase].Y-ankles[phase].Y));
            }
            // Keep a support foot on the same floor as idle. Lift comes from the
            // reference's joint angles, rather than stretching legs between frames.
            int bottom=0;
            foreach(string id in new[]{"near_foot","far_foot"})
            {
                var c=DrawWalkPart(id,definitions[id],p[id]);
                for(int j=0;j<c.Cells.Length;j++)if(c.Cells[j]!=null)bottom=Math.Max(bottom,j/128);
            }
            int dy=121-bottom;
            foreach(string id in LayerOrder)
            {
                var v=p[id];p[id]=new(new(v.Start.X,v.Start.Y+dy),
                    v.End is P end?new(end.X,end.Y+dy):null);
            }
            poses.Add(new($"run_{i}","run",42,p));
            traces.Add(new{frame=i,gifFrame=i*7,durationMs=140,
                near=new{hip=hips[i],knee=knees[i],ankle=ankles[i],toe=toes[i],
                    shoulder=shoulders[i],elbow=elbows[i],wrist=wrists[i]},
                farPhase=(i+4)%8,mapped=p});
        }
        Save("art/walk-reference.json",new{reference="reference_pics/soldier_class/soldier_walk.gif",
            decodedFrames=224,uniqueFullImages=12,referencePoseFrames=new[]{0,7,14,21,28,35,42,49},
            durationMs=140,loopMs=1120,
            method="Trace joint directions from the eight-frame figure; retain idle bone lengths and part drawing dimensions. Round endpoints to integer pixels. Armor contours remain the existing design.",frames=traces});
        return poses;
    }

    static Canvas DrawWalkPart(string id,PartDefinition d,Placement p)
    {
        if(!id.EndsWith("foot"))return DrawIdlePart(id,d,p);
        var c=new Canvas(128,128);var end=p.End!;
        double length=Length(p.Start,end),cos=(end.X-p.Start.X)/length,sin=(end.Y-p.Start.Y)/length;
        Draw(c,d,v=>{
            double x=(v.X-d.Anchor[0])*.64,y=(v.Y-d.Anchor[1])*.40;
            return new((int)Math.Round(p.Start.X+x*cos-y*sin),(int)Math.Round(p.Start.Y+x*sin+y*cos));
        });
        return c;
    }

    static void ReworkWalk()
    {
        const string file="art/soldier.pixel.json";
        Console.WriteLine(RunPix("inspect",file,"--json"));
        var source=JsonNode.Parse(File.ReadAllText(file))!;
        int oldRate=source["ticksPerSecond"]!.GetValue<int>();
        if(300%oldRate!=0)throw new Exception("Unsupported source clock");
        int factor=300/oldRate;
        var character=source["characters"]![0]!;
        var definitions=LayerOrder.ToDictionary(id=>id,id=>JsonSerializer.Deserialize<PartDefinition>(
            File.ReadAllText($"art/draw/{id}.json"))!);
        var poses=WalkPoses(definitions);
        var ops=new List<object>{new{type="updateProject",patch=new{ticksPerSecond=300}}};
        foreach(var clip in character["animations"]!.AsArray())
            ops.Add(new{type="removeAnimation",characterId="soldier",animationId=clip!["id"]!.GetValue<string>()});
        foreach(var frame in character["views"]![0]!["frames"]!.AsArray())
        {
            string id=frame!["id"]!.GetValue<string>();
            if(id.StartsWith("run_"))ops.Add(new{type="removeFrame",characterId="soldier",viewId="right",frameId=id});
            else ops.Add(new{type="updateFrame",characterId="soldier",viewId="right",frameId=id,
                patch=new{durationTicks=frame["durationTicks"]!.GetValue<int>()*factor}});
        }
        foreach(var pose in poses)
            ops.Add(new{type="addFrame",characterId="soldier",viewId="right",frame=new{
                id=pose.Id,name=pose.Id,durationTicks=42,
                cels=LayerOrder.ToDictionary(id=>id,id=>new{grid=new{width=128,height=128,
                    cells=DrawWalkPart(id,definitions[id],pose.Parts[id]).Cells},offset=new{x=0,y=0}})}});
        foreach(var oldClip in character["animations"]!.AsArray())
        {
            var clip=oldClip!.DeepClone();
            foreach(var frame in clip["frames"]!.AsArray())
                frame!["durationTicks"]=clip["id"]!.GetValue<string>()=="run"?42:frame["durationTicks"]!.GetValue<int>()*factor;
            ops.Add(new{type="addAnimation",characterId="soldier",animation=clip});
        }
        Save("art/work/walk.operations.json",ops);
        Console.WriteLine(RunPix("apply",file,"--operations","art/work/walk.operations.json","--expected-hash",ProjectHash(file)));
        Console.WriteLine(RunPix("validate",file,"--json"));
        RunPix("render",file,"--frame","run_0","--scale","4","--out","art/previews/run.png");
        RunPix("sheet",file,"--animation","run","--layout","horizontal","--out","art/exports/animations/run.png",
            "--manifest","art/exports/animations/run.json");
        RunPix("gif",file,"--animation","run","--scale","4","--out","art/previews/run.gif");
        var metadata=JsonNode.Parse(File.ReadAllText("art/poses.json"))!;
        metadata["ticksPerSecond"]=300;
        var ordered=metadata["frames"]!.AsArray();
        for(int i=0;i<ordered.Count;i++)
        {
            if(ordered[i]!["clip"]!.GetValue<string>()=="run")
            {
                var p=poses.Single(p=>p.Id==ordered[i]!["id"]!.GetValue<string>());
                ordered[i]=JsonSerializer.SerializeToNode(new{id=p.Id,clip=p.Clip,ticks=p.Ticks,parts=p.Parts});
            }
            else ordered[i]!["ticks"]=ordered[i]!["ticks"]!.GetValue<int>()*factor;
        }
        Save("art/poses.json",metadata);
        var authored=JsonNode.Parse(File.ReadAllText(file))!;
        var c=authored["characters"]![0]!;
        c["animations"]!.AsArray().Add(JsonSerializer.SerializeToNode(new{
            id="all",name="All frames",viewId="right",loop=false,tags=Array.Empty<string>(),
            frames=ordered.Select(p=>new{frameId=p!["id"]!.GetValue<string>(),durationTicks=p["ticks"]!.GetValue<int>()}).ToArray()}));
        foreach(string id in LayerOrder)
        {
            foreach(var layer in c["layers"]!.AsArray())layer!["visible"]=layer["id"]!.GetValue<string>()==id;
            string temp=$"art/work/layer-{id}.pixel.json";
            File.WriteAllText(temp,authored.ToJsonString());
            RunPix("sheet",temp,"--animation","all","--layout","horizontal","--out",$"art/exports/layers/{id}.png");
        }
        File.WriteAllText(file,JsonNode.Parse(File.ReadAllText(file))!.ToJsonString()+"\n");
    }
}
