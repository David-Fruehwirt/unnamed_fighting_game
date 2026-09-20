using System.Text.Json;
using System.Text.Json.Nodes;

namespace PixelAuthor;

public static partial class Program
{
    static readonly string[] FightLabels = ["Smear", "Hit", "Follow-through", "Recover", "Overshoot"];
    static readonly int[] FightTicks = [15,30,15,15,15]; // 300 Hz: 50/100/50/50/50 ms.
    static readonly double[] FightLean = [8,10,12,5,-3];

    // Fixed endpoint IK keeps the boots planted and preserves both leg lengths.
    static P FightKnee(P hip, P ankle, double thigh, double shin, P originalKnee)
    {
        double dx=ankle.X-hip.X,dy=ankle.Y-hip.Y,d=Math.Sqrt(dx*dx+dy*dy);
        if(d>thigh+shin||d<Math.Abs(thigh-shin))throw new Exception("Unreachable planted attack foot");
        double along=(thigh*thigh-shin*shin+d*d)/(2*d),height=Math.Sqrt(Math.Max(0,thigh*thigh-along*along));
        P Candidate(int sign)=>new((int)Math.Round(hip.X+dx/d*along-sign*dy/d*height),
            (int)Math.Round(hip.Y+dy/d*along+sign*dx/d*height));
        P a=Candidate(1),b=Candidate(-1);
        return Length(a,originalKnee)<Length(b,originalKnee)?a:b;
    }

    static List<PoseFrame> FightPoses()
    {
        var idle=JsonSerializer.Deserialize<Dictionary<string,Placement>>(
            JsonNode.Parse(File.ReadAllText("art/poses.json"))!["frames"]![0]!["parts"]!.ToJsonString())!;
        // Shoulder/elbow/wrist centers traced in the live top-left GIF panel.
        // Directions are mirrored, then fitted to the unmodified idle bone lengths.
        P[][] far = [
            [new(276,208),new(216,208),new(160,208)],
            [new(276,208),new(216,204),new(156,208)],
            [new(276,208),new(220,208),new(156,208)],
            [new(302,210),new(268,246),new(208,212)],
            [new(308,210),new(294,252),new(258,210)]];
        P[][] near = [
            [new(354,210),new(380,256),new(340,236)],
            [new(354,210),new(380,256),new(340,236)],
            [new(354,210),new(380,256),new(340,236)],
            [new(370,208),new(384,252),new(344,232)],
            [new(378,210),new(390,250),new(346,226)]];
        int[] shiftX=[2,3,3,1,0],shiftY=[1,2,2,1,0];
        var poses=new List<PoseFrame>();
        for(int i=0;i<5;i++)
        {
            P pelvis=idle["pelvis"].Start;
            P Root(P v){P r=Turn(new(v.X-pelvis.X,v.Y-pelvis.Y),FightLean[i]);
                return new(pelvis.X+r.X+shiftX[i],pelvis.Y+r.Y+shiftY[i]);}
            var p=idle.ToDictionary(x=>x.Key,x=>x.Value);
            p["pelvis"]=new(new(pelvis.X+shiftX[i],pelvis.Y+shiftY[i]));
            foreach(string id in new[]{"head","torso","backpack"})p[id]=new(Root(idle[id].Start));
            foreach(string side in new[]{"far","near"})
            {
                var trace=(side=="far"?far:near)[i];
                var arm=idle[side+"_upper_arm"];var forearm=idle[side+"_forearm"];
                P Mirror(P v)=>new(-v.X,v.Y);
                P shoulder=Root(arm.Start);
                P elbow=Along(shoulder,Mirror(trace[0]),Mirror(trace[1]),Length(arm.Start,arm.End!));
                P wrist=Along(elbow,Mirror(trace[1]),Mirror(trace[2]),Length(forearm.Start,forearm.End!));
                SetLimb(p,side,"upper_arm","forearm","hand",shoulder,elbow,wrist);
                var thigh=idle[side+"_thigh"];var shin=idle[side+"_shin"];
                P hip=new(thigh.Start.X+shiftX[i],thigh.Start.Y+shiftY[i]);
                P knee=FightKnee(hip,shin.End!,Length(thigh.Start,thigh.End!),Length(shin.Start,shin.End!),thigh.End!);
                SetLimb(p,side,"thigh","shin","foot",hip,knee,shin.End!);
            }
            poses.Add(new($"attack_{i}","attack",FightTicks[i],p));
        }
        Save("art/fight-reference.json",new{reference="reference_pics/soldier_class/soldier_fight.gif",
            decodedFrameCount=88,reviewedUniqueFrames=Enumerable.Range(0,13).Concat(Enumerable.Range(82,6)),
            mapping="Mirror reference directions. Keep idle geometry/scales and bone lengths; pin idle ankle locations with two-bone IK. White effect masks sampled from all four live effect stages.",
            frames=poses.Select((p,i)=>new{gifFrame=8+i,breakdownFrame=83+i,label=FightLabels[i],ticks=p.Ticks,
                torsoDegrees=FightLean[i],farTrace=far[i],nearTrace=near[i],parts=p.Parts})});
        return poses;
    }

    static Canvas DrawFightPart(string id,PartDefinition d,Placement p,int phase)
    {
        if(id is not ("head" or "torso" or "backpack"))return DrawIdlePart(id,d,p);
        var c=new Canvas(128,128);
        (double sx,double sy)=id switch{"head"=>(.72,.65),"torso"=>(.85,1.12),_=>(.70,.90)};
        double angle=FightLean[phase]*(id=="head"?.3:1)*Math.PI/180;
        Draw(c,d,v=>{
            double x=(v.X-d.Anchor[0])*sx,y=(v.Y-d.Anchor[1])*sy;
            return new((int)Math.Round(p.Start.X+x*Math.Cos(angle)-y*Math.Sin(angle)),
                (int)Math.Round(p.Start.Y+x*Math.Sin(angle)+y*Math.Cos(angle)));
        });
        return c;
    }

    static Canvas FightEffect(List<PoseFrame> poses,int phase)
    {
        var c=new Canvas(160,128);
        if(phase==0)
        {
            // Separate trailing arc, matching the reference's arm smear. Armor is never stretched.
            P elbow=poses[0].Parts["far_forearm"].Start,wrist=poses[0].Parts["far_hand"].Start;
            P[] arc=[new(elbow.X-6,elbow.Y+5),new(elbow.X-2,elbow.Y+8),
                new(elbow.X+7,elbow.Y+7),new(wrist.X-2,wrist.Y+3)];
            for(int i=1;i<arc.Length;i++)c.Line(arc[i-1],arc[i],"steel");
        }
        else
        {
            var mask=JsonNode.Parse(File.ReadAllText("art/fight-effect-traces.json"))![phase-1]!;
            var rows=mask["rows"]!.AsArray();
            // Effect stays at the impact point while the fist retracts.
            P hit=poses[1].Parts["far_hand"].Start;
            for(int y=0;y<rows.Count;y++)
            {
                string row=rows[y]!.GetValue<string>();
                for(int x=0;x<row.Length;x++)if(row[x]=='#')
                {
                    int left=(int)Math.Round(hit.X-(108+x*4+4-156)*.30);
                    int right=(int)Math.Round(hit.X-(108+x*4-156)*.30);
                    int top=(int)Math.Round(hit.Y+(164+y*4-208)*.30);
                    int bottom=(int)Math.Round(hit.Y+(164+y*4+4-208)*.30);
                    for(int py=top;py<bottom;py++)for(int px=left;px<right;px++)c.Set(px,py,"light");
                }
            }
        }
        return c;
    }

    static void BuildFight()
    {
        const string file="art/soldier.pixel.json",fxFile="art/fight-effects.pixel.json";
        Directory.CreateDirectory("art/exports/effects");
        Console.WriteLine(RunPix("inspect",file,"--json"));
        var before=JsonNode.Parse(File.ReadAllText(file))!;var character=before["characters"]![0]!;
        var poses=FightPoses();
        var defs=LayerOrder.ToDictionary(id=>id,id=>JsonSerializer.Deserialize<PartDefinition>(File.ReadAllText($"art/draw/{id}.json"))!);
        var ops=new List<object>();
        if(character["animations"]!.AsArray().Any(a=>a!["id"]!.GetValue<string>()=="attack"))
            ops.Add(new{type="removeAnimation",characterId="soldier",animationId="attack"});
        foreach(var f in character["views"]![0]!["frames"]!.AsArray().Where(f=>f!["id"]!.GetValue<string>().StartsWith("attack_")))
            ops.Add(new{type="removeFrame",characterId="soldier",viewId="right",frameId=f!["id"]!.GetValue<string>()});
        foreach(var (p,i) in poses.Select((p,i)=>(p,i)))
            ops.Add(new{type="addFrame",characterId="soldier",viewId="right",frame=new{id=p.Id,name=FightLabels[i],durationTicks=p.Ticks,
                cels=LayerOrder.ToDictionary(id=>id,id=>new{grid=new{width=128,height=128,cells=DrawFightPart(id,defs[id],p.Parts[id],i).Cells},offset=new{x=0,y=0}})}});
        object Animation()=>new{id="attack",name="Jab",viewId="right",loop=false,tags=new[]{"reference-jab","frame-by-frame"},
            frames=poses.Select(p=>new{frameId=p.Id,durationTicks=p.Ticks}).ToArray()};
        ops.Add(new{type="addAnimation",characterId="soldier",animation=Animation()});
        Save("art/work/fight.operations.json",ops);
        Console.WriteLine(RunPix("apply",file,"--operations","art/work/fight.operations.json","--expected-hash",ProjectHash(file)));
        Console.WriteLine(RunPix("validate",file,"--json"));
        RunPix("render",file,"--frame","attack_1","--scale","4","--out","art/previews/attack-hit.png");
        RunPix("sheet",file,"--animation","attack","--layout","horizontal","--out","art/exports/animations/attack.png","--manifest","art/exports/animations/attack.json");
        RunPix("gif",file,"--animation","attack","--scale","4","--out","art/previews/attack-body.gif");

        // Independent effect project prevents changes to existing body-layer IDs or cels.
        if(!File.Exists(fxFile))
        {
            var fx=Project("fight_effects",160,128,["impact"]);fx["ticksPerSecond"]=300;
            fx["characters"]![0]!["origin"]=JsonSerializer.SerializeToNode(new{x=64,y=121});
            fx["characters"]![0]!["pivot"]=JsonSerializer.SerializeToNode(new{x=64,y=121});
            Save(fxFile,fx);
        }
        Console.WriteLine(RunPix("inspect",fxFile,"--json"));
        var existing=JsonNode.Parse(File.ReadAllText(fxFile))!["characters"]![0]!;
        var fxOps=new List<object>();
        foreach(var a in existing["animations"]!.AsArray())fxOps.Add(new{type="removeAnimation",characterId="fight_effects",animationId=a!["id"]!.GetValue<string>()});
        foreach(var f in existing["views"]![0]!["frames"]!.AsArray().Where(f=>f!["id"]!.GetValue<string>()!="neutral"))
            fxOps.Add(new{type="removeFrame",characterId="fight_effects",viewId="right",frameId=f!["id"]!.GetValue<string>()});
        for(int i=0;i<5;i++)fxOps.Add(new{type="addFrame",characterId="fight_effects",viewId="right",frame=new{id=poses[i].Id,name=FightLabels[i],durationTicks=FightTicks[i],
            cels=new{impact=new{grid=new{width=160,height=128,cells=FightEffect(poses,i).Cells},offset=new{x=0,y=0}}}}});
        fxOps.Add(new{type="addAnimation",characterId="fight_effects",animation=Animation()});
        Save("art/work/fight-effects.operations.json",fxOps);
        Console.WriteLine(RunPix("apply",fxFile,"--operations","art/work/fight-effects.operations.json","--expected-hash",ProjectHash(fxFile)));
        Console.WriteLine(RunPix("validate",fxFile,"--json"));
        RunPix("render",fxFile,"--frame","attack_1","--scale","4","--out","art/previews/attack-effect.png");
        RunPix("sheet",fxFile,"--animation","attack","--layout","horizontal","--out","art/exports/effects/attack.png","--manifest","art/exports/effects/attack.json");

        var metadata=JsonNode.Parse(File.ReadAllText("art/poses.json"))!;
        var frames=new JsonArray(metadata["frames"]!.AsArray().Where(p=>p!["clip"]!.GetValue<string>()!="attack").Select(p=>p!.DeepClone()).ToArray());
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
        // Composite preview: effect above the unchanged body, including the impact ring's open fist gap.
        var preview=Project("fight_preview",160,128,["body","effect"]);preview["ticksPerSecond"]=300;
        var pc=preview["characters"]![0]!;var pf=new JsonArray();
        for(int i=0;i<5;i++)
        {
            var canvas=new Canvas(160,128);
            foreach(string id in LayerOrder)
            {
                var part=DrawFightPart(id,defs[id],poses[i].Parts[id],i);
                for(int y=0;y<128;y++)for(int x=0;x<128;x++)if(part.Cells[y*128+x] is string token)canvas.Set(x,y,token);
            }
            pf.Add(JsonSerializer.SerializeToNode(new{id=poses[i].Id,name=FightLabels[i],durationTicks=FightTicks[i],cels=new{
                body=new{grid=new{width=160,height=128,cells=canvas.Cells},offset=new{x=0,y=0}},
                effect=new{grid=new{width=160,height=128,cells=FightEffect(poses,i).Cells},offset=new{x=0,y=0}}}}));
        }
        pc["views"]![0]!["frames"]=pf;pc["animations"]=JsonSerializer.SerializeToNode(new[]{Animation()});
        Save("art/work/fight-preview.pixel.json",preview);
        RunPix("validate","art/work/fight-preview.pixel.json","--json");
        RunPix("gif","art/work/fight-preview.pixel.json","--animation","attack","--scale","4","--out","art/previews/attack.gif");
        RunPix("sheet","art/work/fight-preview.pixel.json","--animation","attack","--layout","horizontal","--out","art/previews/attack-poses.png");
        foreach(string path in new[]{file,fxFile})File.WriteAllText(path,JsonNode.Parse(File.ReadAllText(path))!.ToJsonString()+"\n");
    }
}
