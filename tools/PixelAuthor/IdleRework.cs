using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PixelAuthor;

public static partial class Program
{
    // Original GIF coordinates, not game coordinates. Frame 130 supplies the rig;
    // frames 0–7 supply the live motion. The pink knee marks identify joint centers.
    // Mirror once because the dummy faces left and the Soldier source faces right.
    static P ProjectJoint(P p) => new((int)Math.Round(64 - (p.X - 680) * .30),
        (int)Math.Round(121 + (p.Y - 695) * .30));

    static List<PoseFrame> ReferenceIdle()
    {
        // Cleanup: fixed articulation, planted legs, and one shared pixel of breathing.
        int[] breath = [0,0,-1,-1,-1,0,0,0];
        var result = new List<PoseFrame>();
        var traces = new List<object>();
        for (int i=0;i<8;i++)
        {
            int b=0;
            var source = new Dictionary<string,Placement> {
                ["head"] = new(new(674,398)),
                ["torso"] = new(new(678,496+b)),
                ["pelvis"] = new(new(678,508+b)),
                ["backpack"] = new(new(711,434+b))
            };
            SetLimb(source,"far","upper_arm","forearm","hand",
                new(638,406),new(626,478),new(598,450));
            SetLimb(source,"near","upper_arm","forearm","hand",
                new(718,398),new(730,474),new(686,466));
            SetLimb(source,"far","thigh","shin","foot",
                new(658,514),new(624,596),new(648,680));
            SetLimb(source,"near","thigh","shin","foot",
                new(698,514),new(724,596),new(748,684));
            var mapped=source.ToDictionary(p=>p.Key,p=>new Placement(ProjectJoint(p.Value.Start),
                p.Value.End is P end ? ProjectJoint(end) : null));
            foreach(string id in LayerOrder.Where(id=>id is "head" or "torso" or "backpack" ||
                id.Contains("arm") || id.EndsWith("hand")))
            {
                var p=mapped[id];
                mapped[id]=new(new(p.Start.X,p.Start.Y+breath[i]),p.End is P e?new(e.X,e.Y+breath[i]):null);
            }
            int ticks=i<4?6:3;
            result.Add(new($"idle_{i}","idle",ticks,mapped));
            traces.Add(new{gifFrame=i,durationMs=ticks*1000/60,source,mapped});
        }
        Save("art/idle-reference.json",new{
            reference="reference_pics/soldier_class/soldier_stance.gif",skeletonFrame=130,
            skeleton=new{head=new[]{664,375},neck=new[]{676,391},pelvis=new[]{684,500},
                shoulders=new[]{new[]{638,406},new[]{718,398}},
                elbows=new[]{new[]{626,458},new[]{730,454}},
                wrists=new[]{new[]{598,410},new[]{686,426}},
                hips=new[]{new[]{668,513},new[]{698,513}},
                knees=new[]{new[]{622,586},new[]{722,590}},
                ankles=new[]{new[]{648,681},new[]{746,685}}},
            mapping=new{scale=.30,mirrorX=true,sourceOrigin=new[]{680,695},targetOrigin=new[]{64,121}},
            notes="Cleanup supersedes exact GIF motion: lowered guard, planted legs and shared one-pixel breathing. Source coordinates establish the base stance; mapped coordinates include breathing. Original armor definitions remain unchanged.",
            frames=traces});
        return result;
    }

    static Canvas DrawIdlePart(string id, PartDefinition d, Placement p)
    {
        var canvas=new Canvas(128,128);
        int ax=d.Anchor[0], ay=d.Anchor[1];
        if(p.End is P end)
        {
            double dx=end.X-p.Start.X,dy=end.Y-p.Start.Y;
            double length=Math.Sqrt(dx*dx+dy*dy);
            double width=id.EndsWith("upper_arm")?.65:id.EndsWith("forearm")?.70:
                id.EndsWith("shin")?.85:1.0;
            Draw(canvas,d,v=>{
                double along=(double)(v.Y-ay)/(d.End[1]-ay),across=(v.X-ax)*width;
                return new((int)Math.Round(p.Start.X+along*dx+across*dy/length),
                    (int)Math.Round(p.Start.Y+along*dy-across*dx/length));
            });
        }
        else
        {
            // Proportion changes apply to this clip only. Preserve the authored shapes.
            (double sx,double sy)=id switch {
                "head"=>(.72,.65), "torso"=>(.85,1.12), "pelvis"=>(.80,.55),
                "backpack"=>(.70,.90),
                _ when id.EndsWith("hand")=>(.60,.50),
                _ when id.EndsWith("foot")=>(.64,.40), _=>(1,1)};
            Draw(canvas,d,v=>new((int)Math.Round(p.Start.X+(v.X-ax)*sx),
                (int)Math.Round(p.Start.Y+(v.Y-ay)*sy)));
        }
        CleanIdlePixels(canvas);
        return canvas;
    }

    static void CleanIdlePixels(Canvas canvas)
    {
        var cells=canvas.Cells;
        // Broad, stable color planes instead of several one-pixel highlight ramps.
        for(int i=0;i<cells.Length;i++)cells[i]=cells[i] switch {
            "light"=>"white", "blue_light"=>"blue", "cyan_light"=>"cyan",
            "steel"=>"joint", _=>cells[i]};
        var visited=new bool[cells.Length];
        for(int seed=0;seed<cells.Length;seed++)
        {
            if(visited[seed]||cells[seed] is null)continue;
            var island=new List<int>();var queue=new Queue<int>();queue.Enqueue(seed);visited[seed]=true;
            while(queue.TryDequeue(out int p))
            {
                island.Add(p);
                for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)
                {
                    int x=p%128+dx,y=p/128+dy,n=y*128+x;
                    if(x<0||x>=128||y<0||y>=128||visited[n]||cells[n] is null)continue;
                    visited[n]=true;queue.Enqueue(n);
                }
            }
            if(island.Count<=3)foreach(int p in island)cells[p]=null;
        }
        var original=(string?[])cells.Clone();
        for(int y=1;y<127;y++)for(int x=1;x<127;x++)
        {
            int p=y*128+x;
            if(original[p] is null or "outline" or "cyan")continue;
            var neighbors=new List<string>();
            for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)
                if((dx!=0||dy!=0)&&original[p+dy*128+dx] is string token)neighbors.Add(token);
            if(neighbors.Count!=8||neighbors.Count(t=>t==original[p])>1)continue;
            var dominant=neighbors.GroupBy(t=>t).OrderByDescending(g=>g.Count()).First();
            if(dominant.Count()>=5)cells[p]=dominant.Key;
        }
    }

    static void ReworkIdle()
    {
        const string file="art/soldier.pixel.json";
        Console.WriteLine(RunPix("inspect",file,"--json"));
        var before=JsonNode.Parse(File.ReadAllText(file))!;
        var oldCharacter=before["characters"]![0]!;
        var protectedFrames=oldCharacter["views"]![0]!["frames"]!.AsArray()
            .Where(f=>!f!["id"]!.GetValue<string>().StartsWith("idle_"))
            .ToDictionary(f=>f!["id"]!.GetValue<string>(),f=>f!.ToJsonString());
        var protectedAnimations=oldCharacter["animations"]!.AsArray()
            .Where(a=>a!["id"]!.GetValue<string>()!="idle")
            .ToDictionary(a=>a!["id"]!.GetValue<string>(),a=>a!.ToJsonString());
        var protectedPaths=Directory.GetFiles("art/parts","*.json")
            .Concat(Directory.GetFiles("art/draw","*.json"))
            .Concat(new[]{"art/palette.json","game/scripts/Soldier.cs"})
            .Concat(new[]{"run","jump","fall"}.SelectMany(c=>new[]{
                $"art/exports/animations/{c}.png",$"art/exports/animations/{c}.json",$"art/previews/{c}.gif"}));
        string Hash(string path)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
        var hashes=protectedPaths.ToDictionary(p=>p,Hash);
        var oldPoses=JsonNode.Parse(File.ReadAllText("art/poses.json"))!;
        var idle=ReferenceIdle();
        var definitions=LayerOrder.ToDictionary(id=>id,id=>JsonSerializer.Deserialize<PartDefinition>(
            File.ReadAllText($"art/draw/{id}.json"))!);
        var basePixels=LayerOrder.ToDictionary(id=>id,id=>DrawIdlePart(id,definitions[id],idle[0].Parts[id]).Cells);
        string?[] StablePixels(string id, PoseFrame pose)
        {
            var pixels=new string?[128*128];
            int dx=pose.Parts[id].Start.X-idle[0].Parts[id].Start.X;
            int dy=pose.Parts[id].Start.Y-idle[0].Parts[id].Start.Y;
            for(int y=0;y<128;y++)for(int x=0;x<128;x++)
                if(x+dx>=0&&x+dx<128&&y+dy>=0&&y+dy<128)
                    pixels[(y+dy)*128+x+dx]=basePixels[id][y*128+x];
            return pixels;
        }
        var ops=new List<object>{new{type="removeAnimation",characterId="soldier",animationId="idle"}};
        foreach(var f in oldCharacter["views"]![0]!["frames"]!.AsArray()
            .Where(f=>f!["id"]!.GetValue<string>().StartsWith("idle_")))
            ops.Add(new{type="removeFrame",characterId="soldier",viewId="right",frameId=f!["id"]!.GetValue<string>()});
        foreach(var pose in idle)
            ops.Add(new{type="addFrame",characterId="soldier",viewId="right",frame=new{
                id=pose.Id,name=pose.Id,durationTicks=pose.Ticks,
                cels=LayerOrder.ToDictionary(id=>id,id=>new{grid=new{width=128,height=128,
                    cells=StablePixels(id,pose)},offset=new{x=0,y=0}})}});
        ops.Add(new{type="addAnimation",characterId="soldier",index=0,animation=new{
            id="idle",name="idle",viewId="right",loop=true,tags=new[]{"frame-by-frame","reference-skeleton"},
            frames=idle.Select(p=>new{frameId=p.Id,durationTicks=p.Ticks}).ToArray()}});
        Save("art/work/idle-rework.operations.json",ops);
        Console.WriteLine(RunPix("apply",file,"--operations","art/work/idle-rework.operations.json",
            "--expected-hash",ProjectHash(file)));
        Console.WriteLine(RunPix("validate",file,"--json"));
        var authored=JsonNode.Parse(File.ReadAllText(file))!;
        var character=authored["characters"]![0]!;
        foreach(var f in character["views"]![0]!["frames"]!.AsArray())
            if(protectedFrames.TryGetValue(f!["id"]!.GetValue<string>(),out var original)&&f.ToJsonString()!=original)
                throw new Exception("Non-idle source frame changed");
        foreach(var a in character["animations"]!.AsArray())
            if(protectedAnimations.TryGetValue(a!["id"]!.GetValue<string>(),out var original)&&a.ToJsonString()!=original)
                throw new Exception("Non-idle animation changed");
        RunPix("sheet",file,"--animation","idle","--layout","horizontal",
            "--out","art/exports/animations/idle.png","--manifest","art/exports/animations/idle.json");
        RunPix("gif",file,"--animation","idle","--scale","4","--out","art/previews/idle.gif");
        RunPix("render",file,"--frame","idle_0","--scale","4","--out","art/previews/idle.png");
        var ordered=new JsonArray();
        foreach(var p in idle)ordered.Add(JsonSerializer.SerializeToNode(new{id=p.Id,clip=p.Clip,ticks=p.Ticks,parts=p.Parts}));
        foreach(var f in oldPoses["frames"]!.AsArray().Where(f=>f!["clip"]!.GetValue<string>()!="idle"))
            ordered.Add(f!.DeepClone());
        oldPoses["frames"]=ordered;
        Save("art/poses.json",oldPoses);
        character["animations"]!.AsArray().Add(JsonSerializer.SerializeToNode(new{
            id="all",name="All frames",viewId="right",loop=false,tags=Array.Empty<string>(),
            frames=ordered.Select(p=>new{frameId=p!["id"]!.GetValue<string>(),durationTicks=p["ticks"]!.GetValue<int>()}).ToArray()}));
        foreach(string id in LayerOrder)
        {
            foreach(var layer in character["layers"]!.AsArray())layer!["visible"]=layer["id"]!.GetValue<string>()==id;
            string temp=$"art/work/layer-{id}.pixel.json";
            File.WriteAllText(temp,authored.ToJsonString());
            RunPix("sheet",temp,"--animation","all","--layout","horizontal","--out",$"art/exports/layers/{id}.png");
        }
        foreach(var (path,hash) in hashes)if(Hash(path)!=hash)throw new Exception("Protected file changed: "+path);
        Save("art/IDLE_PRESERVATION.json",new{protectedFileHashes=hashes,
            protectedFrameHashes=protectedFrames.ToDictionary(p=>p.Key,p=>Convert.ToHexString(
                SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(p.Value))).ToLowerInvariant()),
            result="Non-idle cels, animation timing, pose records, part definitions and controller preserved."});
        File.WriteAllText(file,JsonNode.Parse(File.ReadAllText(file))!.ToJsonString()+"\n");
        Console.WriteLine("Eight idle frames exported. Non-idle source and protected file checks passed.");
    }
}
