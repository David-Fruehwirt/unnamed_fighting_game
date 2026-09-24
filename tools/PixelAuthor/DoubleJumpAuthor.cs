using System.Text.Json;
using System.Text.Json.Nodes;
namespace PixelAuthor;
public static partial class Program
{
    static readonly double[] DoubleLean=[18,12,8,6,8,18,16,20];
    static List<PoseFrame> DoublePoses(Dictionary<string,PartDefinition> defs)
    {
        var idle=JsonSerializer.Deserialize<Dictionary<string,Placement>>(
            JsonNode.Parse(File.ReadAllText("art/poses.json"))!["frames"]![0]!["parts"]!.ToJsonString())!;
        // Directions traced from each numbered pose. Zero points downward; positive
        // angles point forward. Lengths always come from the existing idle rig.
        double[] nearThigh=[-20,-12,4,25,28,25,25,-34];
        double[] nearShin=[-56,-37,-30,-38,-50,-41,-35,-56];
        double[] farThigh=[60,52,60,65,58,54,41,51];
        double[] farShin=[-36,-42,-36,-44,-42,-50,-58,-36];
        double[] nearArm=[-65,-60,-54,-52,-50,-55,-50,-50];
        double[] nearForearm=[-30,-30,-20,-30,-35,-20,-25,-25];
        double[] farArm=[15,12,10,14,12,18,16,15];
        double[] farForearm=[130,135,138,135,140,128,132,130];
        var poses=new List<PoseFrame>();var reference=new List<object>();
        for(int i=0;i<8;i++)
        {
            var p=idle.ToDictionary(x=>x.Key,x=>x.Value);
            {
                var pivot=idle["pelvis"].Start;
                P UpperRoot(P v){var r=Turn(new(v.X-pivot.X,v.Y-pivot.Y),DoubleLean[i]);return new(pivot.X+r.X,pivot.Y+r.Y);}
                foreach(string id in new[]{"torso","head","backpack"})p[id]=new(UpperRoot(idle[id].Start));
                foreach(string side in new[]{"near","far"})
                {
                    bool near=side=="near";
                    void Fit(string upper,string lower,string tip,double a,double b,bool arm)
                    {
                        var u=idle[side+"_"+upper];var l=idle[side+"_"+lower];
                        P root=arm?UpperRoot(u.Start):u.Start;
                        P joint=Bone(root,a,Length(u.Start,u.End!));
                        P end=Bone(joint,b,Length(l.Start,l.End!));
                        SetLimb(p,side,upper,lower,tip,root,joint,end);
                    }
                    Fit("thigh","shin","foot",near?nearThigh[i]:farThigh[i],near?nearShin[i]:farShin[i],false);
                    Fit("upper_arm","forearm","hand",near?nearArm[i]:farArm[i],near?nearForearm[i]:farForearm[i],true);
                    var foot=p[side+"_foot"].Start;
                    bool flying=i<=5;
                    p[side+"_foot"]=new(foot,new(foot.X+20,foot.Y+(flying?25:0)));
                }
                // Grounded phases keep contact on the baseline. In flight, only the
                // body articulation is baked here; physics supplies the world arc.
                if(i>=6)
                {
                    int bottom=0;
                    foreach(string id in new[]{"near_foot","far_foot"})
                    {
                        var c=DrawDoublePart(id,defs[id],p[id],i);
                        for(int j=0;j<c.Cells.Length;j++)if(c.Cells[j]!=null)bottom=Math.Max(bottom,j/128);
                    }
                    int dy=121-bottom;
                    foreach(string id in LayerOrder){var v=p[id];p[id]=new(new(v.Start.X,v.Start.Y+dy),
                        v.End is P e?new(e.X,e.Y+dy):null);}
                }
            }
            string clip=i<=4?"double_rise":i<=6?"double_fall":"double_land";
            int ticks=i<=4?20:i<=6?30:40;
            poses.Add(new($"double_pose_{i:00}",clip,ticks,p));
            reference.Add(new{number=i+1,label=JumpLabels[i],torsoDegrees=DoubleLean[i],
                near=new{thigh=nearThigh[i],shin=nearShin[i],upperArm=nearArm[i],forearm=nearForearm[i]},
                far=new{thigh=farThigh[i],shin=farShin[i],upperArm=farArm[i],forearm=farForearm[i]},parts=p});
        }
        Save("art/double-jump-reference.json",new{reference="reference_pics/soldier_class/soldier_jump_2.png",
            method="Eight powered variations of the existing jump, with unchanged idle dimensions and fixed bone lengths. Raised guard, tighter tuck and integer endpoints; physics supplies the arc.",frames=reference});
        return poses;
    }
    static Canvas DrawDoublePart(string id,PartDefinition d,Placement p,int phase)
    {
        if(id.EndsWith("foot"))return p.End is null?DrawIdlePart(id,d,p):DrawWalkPart(id,d,p);
        if(id is not ("head" or "torso" or "backpack")||DoubleLean[phase]==0)return DrawIdlePart(id,d,p);
        var c=new Canvas(128,128);
        (double sx,double sy)=id switch{"head"=>(.72,.65),"torso"=>(.85,1.12),_=>(.70,.90)};
        double angle=DoubleLean[phase]*(id=="head"?.3:1)*Math.PI/180;
        Draw(c,d,v=>{
            double x=(v.X-d.Anchor[0])*sx,y=(v.Y-d.Anchor[1])*sy;
            return new((int)Math.Round(p.Start.X+x*Math.Cos(angle)-y*Math.Sin(angle)),
                (int)Math.Round(p.Start.Y+x*Math.Sin(angle)+y*Math.Cos(angle)));
        });
        return c;
    }

    static void BuildDoubleJump()
    {
        const string file="art/soldier.pixel.json";
        var defs=LayerOrder.ToDictionary(id=>id,id=>JsonSerializer.Deserialize<PartDefinition>(File.ReadAllText($"art/draw/{id}.json"))!);
        var poses=DoublePoses(defs);
        Console.WriteLine(RunPix("inspect",file,"--json"));
        var source=JsonNode.Parse(File.ReadAllText(file))!["characters"]![0]!;
        var ops=new List<object>();
        foreach(var a in source["animations"]!.AsArray().Where(a=>a!["id"]!.GetValue<string>().StartsWith("double_")))
            ops.Add(new{type="removeAnimation",characterId="soldier",animationId=a!["id"]!.GetValue<string>()});
        foreach(var f in source["views"]![0]!["frames"]!.AsArray().Where(f=>f!["id"]!.GetValue<string>().StartsWith("double_pose_")))
            ops.Add(new{type="removeFrame",characterId="soldier",viewId="right",frameId=f!["id"]!.GetValue<string>()});
        foreach(var (p,i) in poses.Select((p,i)=>(p,i)))
            ops.Add(new{type="addFrame",characterId="soldier",viewId="right",frame=new{id=p.Id,name=i==0?"Air recoil":JumpLabels[i],durationTicks=p.Ticks,
                cels=LayerOrder.ToDictionary(id=>id,id=>new{grid=new{width=128,height=128,cells=DrawDoublePart(id,defs[id],p.Parts[id],i).Cells},offset=new{x=0,y=0}})}});
        foreach(string clip in new[]{"double_rise","double_fall","double_land","double_jump_sequence"})
            ops.Add(new{type="addAnimation",characterId="soldier",animation=new{id=clip,name=clip,viewId="right",loop=false,tags=new[]{"frame-by-frame","powered-jump"},
                frames=poses.Where(p=>clip=="double_jump_sequence"||p.Clip==clip).Select(p=>new{frameId=p.Id,durationTicks=p.Ticks}).ToArray()}});
        Save("art/work/double.operations.json",ops);
        Console.WriteLine(RunPix("apply",file,"--operations","art/work/double.operations.json","--expected-hash",ProjectHash(file)));
        Console.WriteLine(RunPix("validate",file,"--json"));
        RunPix("sheet",file,"--animation","double_jump_sequence","--layout","horizontal","--out","art/exports/animations/double_jump_sequence.png");
        RunPix("gif",file,"--animation","double_jump_sequence","--scale","4","--out","art/previews/double-jump.gif");
        var metadata=JsonNode.Parse(File.ReadAllText("art/poses.json"))!;
        var frames=new JsonArray(metadata["frames"]!.AsArray().Where(f=>!f!["id"]!.GetValue<string>().StartsWith("double_pose_")).Select(f=>f!.DeepClone()).ToArray());
        metadata["frames"]=frames;
        foreach(var p in poses)frames.Add(JsonSerializer.SerializeToNode(new{id=p.Id,clip=p.Clip,ticks=p.Ticks,parts=p.Parts}));
        Save("art/poses.json",metadata);
        var authored=JsonNode.Parse(File.ReadAllText(file))!;var body=authored["characters"]![0]!;
        body["animations"]!.AsArray().Add(JsonSerializer.SerializeToNode(new{id="all",name="All",viewId="right",loop=false,tags=Array.Empty<string>(),
            frames=frames.Select(p=>new{frameId=p!["id"]!.GetValue<string>(),durationTicks=p["ticks"]!.GetValue<int>()}).ToArray()}));
        foreach(string id in LayerOrder)
        {
            foreach(var layer in body["layers"]!.AsArray())layer!["visible"]=layer["id"]!.GetValue<string>()==id;
            string temp=$"art/work/layer-{id}.pixel.json";File.WriteAllText(temp,authored.ToJsonString());
            RunPix("sheet",temp,"--animation","all","--layout","horizontal","--out",$"art/exports/layers/{id}.png");
        }
        File.WriteAllText(file,JsonNode.Parse(File.ReadAllText(file))!.ToJsonString()+"\n");
    }
}
