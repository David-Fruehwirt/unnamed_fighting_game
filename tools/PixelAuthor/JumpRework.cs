using System.Text.Json;
using System.Text.Json.Nodes;

namespace PixelAuthor;

public static partial class Program
{
    static readonly string[] JumpLabels=["Prepare","Push-off","Takeoff","Ascent 1","Ascent 2","Apex",
        "Descent 1","Descent 2","Landing","Recovery 1","Recovery 2","Idle"];
    static readonly double[] JumpLean=[0,30,5,0,0,0,8,12,45,25,3,0];
    static P Turn(P v,double degrees)
    {
        double a=degrees*Math.PI/180;
        return new((int)Math.Round(v.X*Math.Cos(a)-v.Y*Math.Sin(a)),
            (int)Math.Round(v.X*Math.Sin(a)+v.Y*Math.Cos(a)));
    }
    static P Bone(P root,double degrees,double length)
    {
        double a=degrees*Math.PI/180;
        return new((int)Math.Round(root.X+Math.Sin(a)*length),(int)Math.Round(root.Y+Math.Cos(a)*length));
    }
    static List<PoseFrame> JumpPoses(Dictionary<string,PartDefinition> defs)
    {
        var idle=JsonSerializer.Deserialize<Dictionary<string,Placement>>(
            JsonNode.Parse(File.ReadAllText("art/poses.json"))!["frames"]![0]!["parts"]!.ToJsonString())!;
        // Directions traced from each numbered pose. Zero points downward; positive
        // angles point forward. Lengths always come from the existing idle rig.
        double[] nearThigh=[0,20,-5,30,-15,45,-3,40,75,45,-10,0];
        double[] nearShin=[0,-75,-35,-65,-20,-65,-40,-50,-70,-70,-25,0];
        double[] farThigh=[0,45,40,50,35,65,30,55,65,60,35,0];
        double[] farShin=[0,-35,-40,-65,-25,-75,-25,-50,-65,-5,-20,0];
        double[] nearArm=[0,-76,-54,-43,-15,-36,-49,-44,-68,-44,-40,0];
        double[] nearForearm=[0,-59,-30,-32,90,-10,-37,-18,-20,-15,72,0];
        double[] farArm=[0,-7,6,25,19,17,0,5,-5,17,23,0];
        double[] farForearm=[0,75,80,153,137,135,110,124,27,22,152,0];
        var poses=new List<PoseFrame>();var reference=new List<object>();
        for(int i=0;i<12;i++)
        {
            var p=idle.ToDictionary(x=>x.Key,x=>x.Value);
            if(i!=0&&i!=11)
            {
                var pivot=idle["pelvis"].Start;
                P UpperRoot(P v){var r=Turn(new(v.X-pivot.X,v.Y-pivot.Y),JumpLean[i]);return new(pivot.X+r.X,pivot.Y+r.Y);}
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
                    bool flying=i>=2&&i<=6;
                    p[side+"_foot"]=new(foot,new(foot.X+20,foot.Y+(flying?25:0)));
                }
                // Grounded phases keep contact on the baseline. In flight, only the
                // body articulation is baked here; physics supplies the world arc.
                if(i==1||i>=7)
                {
                    int bottom=0;
                    foreach(string id in new[]{"near_foot","far_foot"})
                    {
                        var c=DrawJumpPart(id,defs[id],p[id],i);
                        for(int j=0;j<c.Cells.Length;j++)if(c.Cells[j]!=null)bottom=Math.Max(bottom,j/128);
                    }
                    int dy=121-bottom;
                    foreach(string id in LayerOrder){var v=p[id];p[id]=new(new(v.Start.X,v.Start.Y+dy),
                        v.End is P e?new(e.X,e.Y+dy):null);}
                }
            }
            string clip=i<2?"prepare":i<6?"jump":i<8?"fall":"land";
            int ticks=i<2?10:i<6?20:i<8?30:20;
            poses.Add(new($"jump_pose_{i:00}",clip,ticks,p));
            reference.Add(new{number=i+1,label=JumpLabels[i],torsoDegrees=JumpLean[i],
                near=new{thigh=nearThigh[i],shin=nearShin[i],upperArm=nearArm[i],forearm=nearForearm[i]},
                far=new{thigh=farThigh[i],shin=farShin[i],upperArm=farArm[i],forearm=farForearm[i]},parts=p});
        }
        Save("art/jump-reference.json",new{reference="reference_pics/soldier_class/soldier_jump.png",
            method="Twelve traced articulations with unchanged idle part dimensions and fixed bone lengths. Integer endpoints; physics supplies the jump arc. Timing authored because reference is a still image.",frames=reference});
        return poses;
    }
    static Canvas DrawJumpPart(string id,PartDefinition d,Placement p,int phase)
    {
        if(id.EndsWith("foot"))return p.End is null?DrawIdlePart(id,d,p):DrawWalkPart(id,d,p);
        if(id is not ("head" or "torso" or "backpack")||JumpLean[phase]==0)return DrawIdlePart(id,d,p);
        var c=new Canvas(128,128);
        (double sx,double sy)=id switch{"head"=>(.72,.65),"torso"=>(.85,1.12),_=>(.70,.90)};
        double angle=JumpLean[phase]*(id=="head"?.3:1)*Math.PI/180;
        Draw(c,d,v=>{
            double x=(v.X-d.Anchor[0])*sx,y=(v.Y-d.Anchor[1])*sy;
            return new((int)Math.Round(p.Start.X+x*Math.Cos(angle)-y*Math.Sin(angle)),
                (int)Math.Round(p.Start.Y+x*Math.Sin(angle)+y*Math.Cos(angle)));
        });
        return c;
    }
    static void ReworkJump()
    {
        const string file="art/soldier.pixel.json";
        Console.WriteLine(RunPix("inspect",file,"--json"));
        var source=JsonNode.Parse(File.ReadAllText(file))!;
        if(source["ticksPerSecond"]!.GetValue<int>()!=300)throw new Exception("Jump authoring requires the existing 300 Hz source clock");
        var character=source["characters"]![0]!;
        var defs=LayerOrder.ToDictionary(id=>id,id=>JsonSerializer.Deserialize<PartDefinition>(File.ReadAllText($"art/draw/{id}.json"))!);
        var poses=JumpPoses(defs);
        var ops=new List<object>();
        foreach(var a in character["animations"]!.AsArray().Where(a=>a!["id"]!.GetValue<string>() is not ("idle" or "run")))
            ops.Add(new{type="removeAnimation",characterId="soldier",animationId=a!["id"]!.GetValue<string>()});
        foreach(var f in character["views"]![0]!["frames"]!.AsArray().Where(f=>{
            string id=f!["id"]!.GetValue<string>();return id.StartsWith("jump_")||id.StartsWith("fall_");}))
            ops.Add(new{type="removeFrame",characterId="soldier",viewId="right",frameId=f!["id"]!.GetValue<string>()});
        var idleCels=character["views"]![0]!["frames"]!.AsArray().Single(f=>f!["id"]!.GetValue<string>()=="idle_0")!["cels"]!;
        for(int i=0;i<poses.Count;i++)
        {
            var p=poses[i];
            var cels=i is 0 or 11?idleCels.DeepClone():JsonSerializer.SerializeToNode(LayerOrder.ToDictionary(id=>id,id=>new{
                grid=new{width=128,height=128,cells=DrawJumpPart(id,defs[id],p.Parts[id],i).Cells},offset=new{x=0,y=0}}))!;
            ops.Add(new{type="addFrame",characterId="soldier",viewId="right",frame=new{id=p.Id,name=JumpLabels[i],durationTicks=p.Ticks,cels}});
        }
        foreach(string clip in new[]{"prepare","jump","fall","land","jump_sequence"})
            ops.Add(new{type="addAnimation",characterId="soldier",animation=new{id=clip,name=clip,viewId="right",loop=false,
                tags=new[]{"frame-by-frame","jump-reference"},frames=poses.Where(p=>clip=="jump_sequence"||p.Clip==clip)
                    .Select(p=>new{frameId=p.Id,durationTicks=p.Ticks}).ToArray()}});
        Save("art/work/jump.operations.json",ops);
        Console.WriteLine(RunPix("apply",file,"--operations","art/work/jump.operations.json","--expected-hash",ProjectHash(file)));
        Console.WriteLine(RunPix("validate",file,"--json"));
        RunPix("render",file,"--frame","jump_pose_08","--scale","4","--out","art/previews/jump-landing.png");
        foreach(string clip in new[]{"prepare","jump","fall","land","jump_sequence"})
        {
            RunPix("sheet",file,"--animation",clip,"--layout","horizontal","--out",$"art/exports/animations/{clip}.png","--manifest",$"art/exports/animations/{clip}.json");
            RunPix("gif",file,"--animation",clip,"--scale","4","--out",$"art/previews/{clip}.gif");
        }
        var metadata=JsonNode.Parse(File.ReadAllText("art/poses.json"))!;
        var frames=new JsonArray();
        foreach(var f in metadata["frames"]!.AsArray().Where(f=>f!["clip"]!.GetValue<string>() is "idle" or "run"))frames.Add(f!.DeepClone());
        foreach(var p in poses)frames.Add(JsonSerializer.SerializeToNode(new{id=p.Id,clip=p.Clip,ticks=p.Ticks,parts=p.Parts}));
        metadata["frames"]=frames;Save("art/poses.json",metadata);
        var authored=JsonNode.Parse(File.ReadAllText(file))!;var c=authored["characters"]![0]!;
        c["animations"]!.AsArray().Add(JsonSerializer.SerializeToNode(new{id="all",name="All",viewId="right",loop=false,tags=Array.Empty<string>(),
            frames=frames.Select(p=>new{frameId=p!["id"]!.GetValue<string>(),durationTicks=p["ticks"]!.GetValue<int>()}).ToArray()}));
        foreach(string id in LayerOrder)
        {
            foreach(var layer in c["layers"]!.AsArray())layer!["visible"]=layer["id"]!.GetValue<string>()==id;
            string temp=$"art/work/layer-{id}.pixel.json";File.WriteAllText(temp,authored.ToJsonString());
            RunPix("sheet",temp,"--animation","all","--layout","horizontal","--out",$"art/exports/layers/{id}.png");
        }
        File.WriteAllText(file,JsonNode.Parse(File.ReadAllText(file))!.ToJsonString()+"\n");
    }
}
