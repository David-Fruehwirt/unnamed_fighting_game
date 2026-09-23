using System.Text.Json;
using System.Text.Json.Nodes;

namespace PixelAuthor;

public static partial class Program
{
    static readonly string[] CrossLabels = ["Load", "Pull", "Smear", "Hit A", "Hit B", "Follow-through", "Recover", "Overshoot"];
    static readonly int[] CrossTicks = [15,15,15,15,15,30,15,15]; // 300 Hz: 450 ms total.
    static readonly double[] CrossLean = [-5,-6,12,14,14,18,8,-3];

    static List<PoseFrame> CrossPoses()
    {
        var idle=JsonSerializer.Deserialize<Dictionary<string,Placement>>(
            JsonNode.Parse(File.ReadAllText("art/poses.json"))!["frames"]![0]!["parts"]!.ToJsonString())!;
        // Shoulder/elbow/wrist centers traced in the live top-left GIF panel.
        // Directions are mirrored, then fitted to the unmodified idle bone lengths.
        P[][] far = [
            [new(192,210),new(168,248),new(184,222)],
            [new(188,212),new(164,248),new(180,222)],
            [new(252,208),new(268,224),new(252,236)],
            [new(252,208),new(268,224),new(252,236)],
            [new(252,208),new(268,224),new(252,236)],
            [new(256,208),new(276,228),new(252,240)],
            [new(224,204),new(212,244),new(180,228)],
            [new(204,210),new(192,252),new(168,212)]];
        P[][] near = [
            [new(284,202),new(328,232),new(300,224)],
            [new(284,202),new(328,232),new(308,226)],
            [new(228,212),new(144,216),new(60,212)],
            [new(228,212),new(144,216),new(60,212)],
            [new(228,212),new(144,216),new(60,212)],
            [new(228,208),new(144,212),new(56,212)],
            [new(252,204),new(208,256),new(140,224)],
            [new(280,208),new(296,252),new(248,228)]];
        int[] shiftX=[-1,-2,3,4,4,5,2,0],shiftY=[0,0,1,2,2,2,1,0];
        int[] twist=[0,0,16,16,16,17,7,0];
        var poses=new List<PoseFrame>();
        for(int i=0;i<8;i++)
        {
            P pelvis=idle["pelvis"].Start;
            P Root(P v){P r=Turn(new(v.X-pelvis.X,v.Y-pelvis.Y),CrossLean[i]);
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
                shoulder=new(shoulder.X+(side=="near"?twist[i]:-twist[i]),shoulder.Y);
                P elbow=Along(shoulder,Mirror(trace[0]),Mirror(trace[1]),Length(arm.Start,arm.End!));
                P wrist=Along(elbow,Mirror(trace[1]),Mirror(trace[2]),Length(forearm.Start,forearm.End!));
                SetLimb(p,side,"upper_arm","forearm","hand",shoulder,elbow,wrist);
                var thigh=idle[side+"_thigh"];var shin=idle[side+"_shin"];
                P hip=new(thigh.Start.X+shiftX[i],thigh.Start.Y+shiftY[i]);
                P knee=FightKnee(hip,shin.End!,Length(thigh.Start,thigh.End!),Length(shin.Start,shin.End!),thigh.End!);
                SetLimb(p,side,"thigh","shin","foot",hip,knee,shin.End!);
            }
            poses.Add(new($"cross_{i}","cross",CrossTicks[i],p));
        }
        Save("art/cross-reference.json",new{reference="reference_pics/soldier_class/soldier_fight_2.gif",
            decodedFrameCount=117,reviewedUniqueFrames=Enumerable.Range(0,16).Concat(Enumerable.Range(109,8)),
            mapping="Mirror reference directions. Keep idle geometry/scales and bone lengths; pin idle ankle locations with two-bone IK. White masks sampled from all five live impact states; rear shoulder rotates forward for the cross.",
            frames=poses.Select((p,i)=>new{gifFrame=8+i,breakdownFrame=110+i-(i>=4?1:0),label=CrossLabels[i],ticks=p.Ticks,
                torsoDegrees=CrossLean[i],farTrace=far[i],nearTrace=near[i],parts=p.Parts})});
        return poses;
    }

    static Canvas DrawCrossPart(string id,PartDefinition d,Placement p,int phase)
    {
        if(id is not ("head" or "torso" or "backpack"))return DrawIdlePart(id,d,p);
        var c=new Canvas(128,128);
        (double sx,double sy)=id switch{"head"=>(.72,.65),"torso"=>(.85,1.12),_=>(.70,.90)};
        double angle=CrossLean[phase]*(id=="head"?.3:1)*Math.PI/180;
        Draw(c,d,v=>{
            double x=(v.X-d.Anchor[0])*sx,y=(v.Y-d.Anchor[1])*sy;
            return new((int)Math.Round(p.Start.X+x*Math.Cos(angle)-y*Math.Sin(angle)),
                (int)Math.Round(p.Start.Y+x*Math.Sin(angle)+y*Math.Cos(angle)));
        });
        return c;
    }

    static Canvas CrossEffect(List<PoseFrame> poses,int phase)
    {
        var c=new Canvas(160,128);
        if(phase==2)
        {
            // Separate arc follows the reference smear; do not stretch armor geometry.
            P elbow=poses[2].Parts["near_forearm"].Start,wrist=poses[2].Parts["near_hand"].Start;
            P[] arc=[new(elbow.X-9,elbow.Y+6),new(elbow.X-3,elbow.Y+9),
                new(elbow.X+8,elbow.Y+7),new(wrist.X-1,wrist.Y+3)];
            for(int i=1;i<arc.Length;i++)c.Line(arc[i-1],arc[i],"steel");
        }
        else if(phase>=3)
        {
            var mask=JsonNode.Parse(File.ReadAllText("art/cross-effect-traces.json"))![phase-3]!;
            var rows=mask["rows"]!.AsArray();
            P hit=poses[3].Parts["near_hand"].Start;
            int ox=mask["origin"]![0]!.GetValue<int>(),oy=mask["origin"]![1]!.GetValue<int>();
            for(int y=0;y<rows.Count;y++)
            {
                string row=rows[y]!.GetValue<string>();
                for(int x=0;x<row.Length;x++)if(row[x]=='#')
                {
                    int left=(int)Math.Round(hit.X-(ox+x*4+4-60)*.30);
                    int right=(int)Math.Round(hit.X-(ox+x*4-60)*.30);
                    int top=(int)Math.Round(hit.Y+(oy+y*4-212)*.30);
                    int bottom=(int)Math.Round(hit.Y+(oy+y*4+4-212)*.30);
                    for(int py=top;py<bottom;py++)for(int px=left;px<right;px++)c.Set(px,py,"light");
                }
            }
        }
        return c;
    }

    static void BuildCross()
    {
        const string file="art/soldier.pixel.json",fxFile="art/cross-effects.pixel.json";
        Directory.CreateDirectory("art/exports/effects");
        Console.WriteLine(RunPix("inspect",file,"--json"));
        var before=JsonNode.Parse(File.ReadAllText(file))!;var character=before["characters"]![0]!;
        var poses=CrossPoses();
        var defs=LayerOrder.ToDictionary(id=>id,id=>JsonSerializer.Deserialize<PartDefinition>(File.ReadAllText($"art/draw/{id}.json"))!);
        var ops=new List<object>();
        if(character["animations"]!.AsArray().Any(a=>a!["id"]!.GetValue<string>()=="cross"))
            ops.Add(new{type="removeAnimation",characterId="soldier",animationId="cross"});
        foreach(var f in character["views"]![0]!["frames"]!.AsArray().Where(f=>f!["id"]!.GetValue<string>().StartsWith("cross_")))
            ops.Add(new{type="removeFrame",characterId="soldier",viewId="right",frameId=f!["id"]!.GetValue<string>()});
        foreach(var (p,i) in poses.Select((p,i)=>(p,i)))
            ops.Add(new{type="addFrame",characterId="soldier",viewId="right",frame=new{id=p.Id,name=CrossLabels[i],durationTicks=p.Ticks,
                cels=LayerOrder.ToDictionary(id=>id,id=>new{grid=new{width=128,height=128,cells=DrawCrossPart(id,defs[id],p.Parts[id],i).Cells},offset=new{x=0,y=0}})}});
        object Animation()=>new{id="cross",name="Cross",viewId="right",loop=false,tags=new[]{"reference-cross","frame-by-frame"},
            frames=poses.Select(p=>new{frameId=p.Id,durationTicks=p.Ticks}).ToArray()};
        ops.Add(new{type="addAnimation",characterId="soldier",animation=Animation()});
        Save("art/work/cross.operations.json",ops);
        Console.WriteLine(RunPix("apply",file,"--operations","art/work/cross.operations.json","--expected-hash",ProjectHash(file)));
        Console.WriteLine(RunPix("validate",file,"--json"));
        RunPix("render",file,"--frame","cross_3","--scale","4","--out","art/previews/cross-hit.png");
        RunPix("sheet",file,"--animation","cross","--layout","horizontal","--out","art/exports/animations/cross.png","--manifest","art/exports/animations/cross.json");
        RunPix("gif",file,"--animation","cross","--scale","4","--out","art/previews/cross-body.gif");

        // Independent effect project prevents changes to existing body-layer IDs or cels.
        if(!File.Exists(fxFile))
        {
            var fx=Project("cross_effects",160,128,["impact"]);fx["ticksPerSecond"]=300;
            fx["characters"]![0]!["origin"]=JsonSerializer.SerializeToNode(new{x=64,y=121});
            fx["characters"]![0]!["pivot"]=JsonSerializer.SerializeToNode(new{x=64,y=121});
            Save(fxFile,fx);
        }
        Console.WriteLine(RunPix("inspect",fxFile,"--json"));
        var existing=JsonNode.Parse(File.ReadAllText(fxFile))!["characters"]![0]!;
        var fxOps=new List<object>();
        foreach(var a in existing["animations"]!.AsArray())fxOps.Add(new{type="removeAnimation",characterId="cross_effects",animationId=a!["id"]!.GetValue<string>()});
        foreach(var f in existing["views"]![0]!["frames"]!.AsArray().Where(f=>f!["id"]!.GetValue<string>()!="neutral"))
            fxOps.Add(new{type="removeFrame",characterId="cross_effects",viewId="right",frameId=f!["id"]!.GetValue<string>()});
        for(int i=0;i<8;i++)fxOps.Add(new{type="addFrame",characterId="cross_effects",viewId="right",frame=new{id=poses[i].Id,name=CrossLabels[i],durationTicks=CrossTicks[i],
            cels=new{impact=new{grid=new{width=160,height=128,cells=CrossEffect(poses,i).Cells},offset=new{x=0,y=0}}}}});
        fxOps.Add(new{type="addAnimation",characterId="cross_effects",animation=Animation()});
        Save("art/work/cross-effects.operations.json",fxOps);
        Console.WriteLine(RunPix("apply",fxFile,"--operations","art/work/cross-effects.operations.json","--expected-hash",ProjectHash(fxFile)));
        Console.WriteLine(RunPix("validate",fxFile,"--json"));
        RunPix("render",fxFile,"--frame","cross_3","--scale","4","--out","art/previews/cross-effect.png");
        RunPix("sheet",fxFile,"--animation","cross","--layout","horizontal","--out","art/exports/effects/cross.png","--manifest","art/exports/effects/cross.json");

        var metadata=JsonNode.Parse(File.ReadAllText("art/poses.json"))!;
        var frames=new JsonArray(metadata["frames"]!.AsArray().Where(p=>p!["clip"]!.GetValue<string>()!="cross").Select(p=>p!.DeepClone()).ToArray());
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
        var preview=Project("cross_preview",160,128,["body","effect"]);preview["ticksPerSecond"]=300;
        var pc=preview["characters"]![0]!;var pf=new JsonArray();
        for(int i=0;i<8;i++)
        {
            var canvas=new Canvas(160,128);
            foreach(string id in LayerOrder)
            {
                var part=DrawCrossPart(id,defs[id],poses[i].Parts[id],i);
                for(int y=0;y<128;y++)for(int x=0;x<128;x++)if(part.Cells[y*128+x] is string token)canvas.Set(x,y,token);
            }
            pf.Add(JsonSerializer.SerializeToNode(new{id=poses[i].Id,name=CrossLabels[i],durationTicks=CrossTicks[i],cels=new{
                body=new{grid=new{width=160,height=128,cells=canvas.Cells},offset=new{x=0,y=0}},
                effect=new{grid=new{width=160,height=128,cells=CrossEffect(poses,i).Cells},offset=new{x=0,y=0}}}}));
        }
        pc["views"]![0]!["frames"]=pf;pc["animations"]=JsonSerializer.SerializeToNode(new[]{Animation()});
        Save("art/work/cross-preview.pixel.json",preview);
        RunPix("validate","art/work/cross-preview.pixel.json","--json");
        RunPix("gif","art/work/cross-preview.pixel.json","--animation","cross","--scale","4","--out","art/previews/cross.gif");
        RunPix("sheet","art/work/cross-preview.pixel.json","--animation","cross","--layout","horizontal","--out","art/previews/cross-poses.png");
        foreach(string path in new[]{file,fxFile})File.WriteAllText(path,JsonNode.Parse(File.ReadAllText(path))!.ToJsonString()+"\n");
    }
}
