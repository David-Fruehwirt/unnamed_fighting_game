using System.Text.Json;
using System.Text.Json.Nodes;
namespace PixelAuthor;
public static partial class Program
{
    static readonly string[] KickLabels=["Guard","Knee lift","Extension","Crescent","Impact","Retract","Tuck","Lower","Last trail","Set down","Recover","Guard settle"];
    static readonly int[] KickTicks=[5,10,5,10,15,10,5,5,5,5,5,10]; // 300 Hz = 300 ms.
    static readonly double[] KickLean=[0,-3,-8,-8,-8,-5,-3,-2,-1,0,0,0];
    static List<PoseFrame> KickPoses()
    {
        var idle=JsonSerializer.Deserialize<Dictionary<string,Placement>>(JsonNode.Parse(File.ReadAllText("art/poses.json"))!["frames"]![0]!["parts"]!.ToJsonString())!;
        // Directions traced from all twelve numbered poses; bones retain idle lengths.
        double[] thigh=[0,112,112,112,112,112,105,65,52,40,25,0];
        double[] shin=[0,-6,103,103,103,-8,-22,-18,-12,-8,-14,0];
        int[] dx=[0,-7,-8,-8,-8,-7,-6,-4,-3,-2,-1,0];
        var result=new List<PoseFrame>();
        for(int i=0;i<12;i++)
        {
            var p=idle.ToDictionary(x=>x.Key,x=>x.Value);
            P pelvis=idle["pelvis"].Start;
            P Root(P v){var r=Turn(new(v.X-pelvis.X,v.Y-pelvis.Y),KickLean[i]);return new(pelvis.X+r.X+dx[i],pelvis.Y+r.Y);}
            p["pelvis"]=new(new(pelvis.X+dx[i],pelvis.Y));
            foreach(string id in new[]{"head","torso","backpack"})p[id]=new(Root(idle[id].Start));
            foreach(string side in new[]{"near","far"})
            {
                foreach(string part in new[]{"upper_arm","forearm","hand"})
                {
                    var v=idle[side+"_"+part];p[side+"_"+part]=new(Root(v.Start),v.End is P e?Root(e):null);
                }
            }
            if(i>0&&i<11)
            {
                var u=idle["near_thigh"];var l=idle["near_shin"];
                P hip=new(u.Start.X+dx[i],u.Start.Y),ankle=l.End!;
                P knee=FightKnee(hip,ankle,Length(u.Start,u.End!),Length(l.Start,l.End!),u.End!);
                SetLimb(p,"near","thigh","shin","foot",hip,knee,ankle);
                u=idle["far_thigh"];l=idle["far_shin"];hip=new(u.Start.X+dx[i],u.Start.Y);
                knee=Bone(hip,thigh[i],Length(u.Start,u.End!));ankle=Bone(knee,shin[i],Length(l.Start,l.End!));
                SetLimb(p,"far","thigh","shin","foot",hip,knee,ankle);
                // Boot points upward on extension; down slightly during retraction.
                p["far_foot"]=new(ankle,new(ankle.X+(i>=2&&i<=4?0:20),ankle.Y+(i>=2&&i<=4?-20:10)));
            }
            result.Add(new($"kick_{i}","kick",KickTicks[i],p));
        }
        Save("art/kick-reference.json",new{reference="reference_pics/soldier_class/soldier_fight_3.png",frameCount=12,
            method="Every numbered pose reviewed. Trace joint directions; retain original part definitions/scales and idle bone lengths. Near ankle pinned; far leg kicks. Timing authored from the still reference.",
            frames=result.Select((p,i)=>new{number=i+1,label=KickLabels[i],ticks=p.Ticks,lean=KickLean[i],thighDegrees=thigh[i],shinDegrees=shin[i],parts=p.Parts})});
        return result;
    }
    static Canvas DrawKickPart(string id,PartDefinition d,Placement p,int phase)
    {
        if(id.EndsWith("foot"))return p.End is null?DrawIdlePart(id,d,p):DrawWalkPart(id,d,p);
        if(id is not ("head" or "torso" or "backpack"))return DrawIdlePart(id,d,p);
        var c=new Canvas(128,128);
        (double sx,double sy)=id switch{"head"=>(.72,.65),"torso"=>(.85,1.12),_=>(.70,.90)};
        double a=KickLean[phase]*(id=="head"?.3:1)*Math.PI/180;
        Draw(c,d,v=>{double x=(v.X-d.Anchor[0])*sx,y=(v.Y-d.Anchor[1])*sy;
            return new((int)Math.Round(p.Start.X+x*Math.Cos(a)-y*Math.Sin(a)),(int)Math.Round(p.Start.Y+x*Math.Sin(a)+y*Math.Cos(a)));});
        return c;
    }
    static Canvas KickEffect(List<PoseFrame> poses,int phase)
    {
        var c=new Canvas(160,128);P foot=poses[phase].Parts["far_foot"].Start;
        P At(int x,int y)=>new(foot.X+x,foot.Y+y);
        void Arc(string token,params P[] points){for(int i=1;i<points.Length;i++)c.Line(points[i-1],points[i],token);}
        if(phase==2)
            Arc("blue_light",At(-28,-12),At(-16,-14),At(-3,-11),At(4,-5),At(5,5),At(0,16),At(-10,25));
        if(phase==3)
        {
            Arc("blue",At(-24,-27),At(-10,-19),At(4,-9),At(7,0),At(3,12),At(-6,24),At(-24,34));
            c.Polygon(new[]{At(-10,-23),At(5,-10),At(9,-1),At(4,12),At(-9,25),At(-1,10),At(3,0),At(0,-10)},"blue_light");
            c.Polygon(new[]{At(-5,-15),At(5,-6),At(6,0),At(1,12),At(-4,17),At(1,3),At(1,-5)},"cyan_light");
            Arc("light",At(0,-9),At(4,-2),At(2,7),At(-2,14));
            Arc("blue",At(-15,27),At(-25,34),At(-30,37));
        }
        if(phase==4)
        {
            // Deliberate asymmetric star rays reproduce the reference's boot-centered flash.
            P[] rays=[At(-24,-38),At(-3,-12),At(4,-31),At(8,-9),At(24,-35),At(13,-4),At(31,-1),At(13,4),At(26,20),At(8,11),At(11,37),At(1,13),At(-9,35),At(-5,10),At(-26,20),At(-9,3),At(-27,-8),At(-8,-7)];
            c.Polygon(rays,"blue");
            c.Polygon(rays.Select(p=>new P(foot.X+(int)Math.Round((p.X-foot.X)*.76),foot.Y+(int)Math.Round((p.Y-foot.Y)*.76))).ToArray(),"blue_light");
            c.Polygon(rays.Select(p=>new P(foot.X+(int)Math.Round((p.X-foot.X)*.52),foot.Y+(int)Math.Round((p.Y-foot.Y)*.52))).ToArray(),"cyan_light");
            c.Polygon(new[]{At(-5,-10),At(1,-5),At(6,-13),At(5,-2),At(12,0),At(5,4),At(5,12),At(0,7),At(-7,12),At(-4,3),At(-11,0),At(-5,-3)},"light");
        }
        if(phase is 5 or 6 or 8)
        {
            int fade=phase==5?0:phase==6?4:8;
            Arc("blue_light",At(9,-24+fade),At(15,-16+fade),At(17,-4),At(13,7),At(5,18-fade));
            if(phase==5)Arc("blue",At(21,-6),At(19,8),At(9,23),At(-2,29));
        }
        return c;
    }
    static void BuildKick()
    {
        const string file="art/soldier.pixel.json",fxFile="art/kick-effects.pixel.json";
        Directory.CreateDirectory("art/exports/effects");
        Console.WriteLine(RunPix("inspect",file,"--json"));
        var before=JsonNode.Parse(File.ReadAllText(file))!;var character=before["characters"]![0]!;
        var poses=KickPoses();
        var defs=LayerOrder.ToDictionary(id=>id,id=>JsonSerializer.Deserialize<PartDefinition>(File.ReadAllText($"art/draw/{id}.json"))!);
        var ops=new List<object>();
        if(character["animations"]!.AsArray().Any(a=>a!["id"]!.GetValue<string>()=="kick"))
            ops.Add(new{type="removeAnimation",characterId="soldier",animationId="kick"});
        foreach(var f in character["views"]![0]!["frames"]!.AsArray().Where(f=>f!["id"]!.GetValue<string>().StartsWith("kick_")))
            ops.Add(new{type="removeFrame",characterId="soldier",viewId="right",frameId=f!["id"]!.GetValue<string>()});
        foreach(var (p,i) in poses.Select((p,i)=>(p,i)))
            ops.Add(new{type="addFrame",characterId="soldier",viewId="right",frame=new{id=p.Id,name=KickLabels[i],durationTicks=p.Ticks,
                cels=LayerOrder.ToDictionary(id=>id,id=>new{grid=new{width=128,height=128,cells=DrawKickPart(id,defs[id],p.Parts[id],i).Cells},offset=new{x=0,y=0}})}});
        object Animation()=>new{id="kick",name="Kick",viewId="right",loop=false,tags=new[]{"reference-kick","frame-by-frame"},
            frames=poses.Select(p=>new{frameId=p.Id,durationTicks=p.Ticks}).ToArray()};
        ops.Add(new{type="addAnimation",characterId="soldier",animation=Animation()});
        Save("art/work/kick.operations.json",ops);
        Console.WriteLine(RunPix("apply",file,"--operations","art/work/kick.operations.json","--expected-hash",ProjectHash(file)));
        Console.WriteLine(RunPix("validate",file,"--json"));
        RunPix("render",file,"--frame","kick_3","--scale","4","--out","art/previews/kick-hit.png");
        RunPix("sheet",file,"--animation","kick","--layout","horizontal","--out","art/exports/animations/kick.png","--manifest","art/exports/animations/kick.json");
        RunPix("gif",file,"--animation","kick","--scale","4","--out","art/previews/kick-body.gif");

        // Independent effect project prevents changes to existing body-layer IDs or cels.
        if(!File.Exists(fxFile))
        {
            var fx=Project("kick_effects",160,128,["impact"]);fx["ticksPerSecond"]=300;
            fx["characters"]![0]!["origin"]=JsonSerializer.SerializeToNode(new{x=64,y=121});
            fx["characters"]![0]!["pivot"]=JsonSerializer.SerializeToNode(new{x=64,y=121});
            Save(fxFile,fx);
        }
        Console.WriteLine(RunPix("inspect",fxFile,"--json"));
        var existing=JsonNode.Parse(File.ReadAllText(fxFile))!["characters"]![0]!;
        var fxOps=new List<object>();
        foreach(var a in existing["animations"]!.AsArray())fxOps.Add(new{type="removeAnimation",characterId="kick_effects",animationId=a!["id"]!.GetValue<string>()});
        foreach(var f in existing["views"]![0]!["frames"]!.AsArray().Where(f=>f!["id"]!.GetValue<string>()!="neutral"))
            fxOps.Add(new{type="removeFrame",characterId="kick_effects",viewId="right",frameId=f!["id"]!.GetValue<string>()});
        for(int i=0;i<12;i++)fxOps.Add(new{type="addFrame",characterId="kick_effects",viewId="right",frame=new{id=poses[i].Id,name=KickLabels[i],durationTicks=KickTicks[i],
            cels=new{impact=new{grid=new{width=160,height=128,cells=KickEffect(poses,i).Cells},offset=new{x=0,y=0}}}}});
        fxOps.Add(new{type="addAnimation",characterId="kick_effects",animation=Animation()});
        Save("art/work/kick-effects.operations.json",fxOps);
        Console.WriteLine(RunPix("apply",fxFile,"--operations","art/work/kick-effects.operations.json","--expected-hash",ProjectHash(fxFile)));
        Console.WriteLine(RunPix("validate",fxFile,"--json"));
        RunPix("render",fxFile,"--frame","kick_3","--scale","4","--out","art/previews/kick-effect.png");
        RunPix("sheet",fxFile,"--animation","kick","--layout","horizontal","--out","art/exports/effects/kick.png","--manifest","art/exports/effects/kick.json");

        var metadata=JsonNode.Parse(File.ReadAllText("art/poses.json"))!;
        var frames=new JsonArray(metadata["frames"]!.AsArray().Where(p=>p!["clip"]!.GetValue<string>()!="kick").Select(p=>p!.DeepClone()).ToArray());
        foreach(var p in poses)frames.Add(JsonSerializer.SerializeToNode(new{id=p.Id,clip=p.Clip,ticks=p.Ticks,parts=p.Parts}));
        metadata["frames"]=frames;Save("art/poses.json",metadata);
        var authored=JsonNode.Parse(File.ReadAllText(file))!;var body=authored["characters"]![0]!;
        body["animations"]!.AsArray().Add(JsonSerializer.SerializeToNode(new{id="all",name="All",viewId="right",loop=false,tags=Array.Empty<string>(),
            frames=frames.Select(p=>new{frameId=p!["id"]!.GetValue<string>(),durationTicks=p["ticks"]!.GetValue<int>()}).ToArray()}));
        foreach(string id in LayerOrder)
        {
            foreach(var layer in body["layers"]!.AsArray())layer!["visible"]=layer["id"]!.GetValue<string>()==id;
            string temp=$"art/work/layer-{id}.pixel.json";File.WriteAllText(temp,authored.ToJsonString());
            RunPix("sheet",temp,"--animation","all","--layout","horizontal","--out",$"art/exports/layers/{id}.png");
        }
        BuildThrusterSockets();
        // Composite preview: independently authored effects above the unchanged body.
        var preview=Project("kick_preview",160,128,["body","effect"]);preview["ticksPerSecond"]=300;
        var pc=preview["characters"]![0]!;var pf=new JsonArray();
        for(int i=0;i<12;i++)
        {
            var canvas=new Canvas(160,128);
            foreach(string id in LayerOrder)
            {
                var part=DrawKickPart(id,defs[id],poses[i].Parts[id],i);
                for(int y=0;y<128;y++)for(int x=0;x<128;x++)if(part.Cells[y*128+x] is string token)canvas.Set(x,y,token);
            }
            pf.Add(JsonSerializer.SerializeToNode(new{id=poses[i].Id,name=KickLabels[i],durationTicks=KickTicks[i],cels=new{
                body=new{grid=new{width=160,height=128,cells=canvas.Cells},offset=new{x=0,y=0}},
                effect=new{grid=new{width=160,height=128,cells=KickEffect(poses,i).Cells},offset=new{x=0,y=0}}}}));
        }
        pc["views"]![0]!["frames"]=pf;pc["animations"]=JsonSerializer.SerializeToNode(new[]{Animation()});
        Save("art/work/kick-preview.pixel.json",preview);
        RunPix("validate","art/work/kick-preview.pixel.json","--json");
        RunPix("gif","art/work/kick-preview.pixel.json","--animation","kick","--scale","4","--out","art/previews/kick.gif");
        RunPix("sheet","art/work/kick-preview.pixel.json","--animation","kick","--layout","horizontal","--out","art/previews/kick-poses.png");
        foreach(string path in new[]{file,fxFile})File.WriteAllText(path,JsonNode.Parse(File.ReadAllText(path))!.ToJsonString()+"\n");
    }
}
