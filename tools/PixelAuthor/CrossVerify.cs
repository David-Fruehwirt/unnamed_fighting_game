using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PixelAuthor;

public static partial class Program
{
    static void VerifyCross()
    {
        const string baseline="a6d8b2e";
        string Git(params string[] args)
        {
            var info=new ProcessStartInfo("git"){RedirectStandardOutput=true,RedirectStandardError=true};
            foreach(string arg in args)info.ArgumentList.Add(arg);
            using var process=Process.Start(info)!;string result=process.StandardOutput.ReadToEnd(),error=process.StandardError.ReadToEnd();
            process.WaitForExit();if(process.ExitCode!=0)throw new Exception("Fight preservation failed: "+result+error);return result;
        }
        var paths=new List<string>{"art/parts","art/draw","art/palette.json","art/stages","game/assets/stages","art/fight-effects.pixel.json","art/exports/effects/attack.png","game/assets/soldier_effects/attack.png"};
        foreach(string clip in new[]{"idle","run","prepare","jump","fall","land","jump_sequence","attack"})paths.AddRange(new[]{
            $"art/exports/animations/{clip}.png",$"art/exports/animations/{clip}.json",$"art/previews/{clip}.gif"});
        Git(new[]{"diff","--exit-code",baseline,"--"}.Concat(paths).ToArray());
        var old=JsonNode.Parse(Git("show",baseline+":art/soldier.pixel.json"))!;
        var now=JsonNode.Parse(File.ReadAllText("art/soldier.pixel.json"))!;
        var oldC=old["characters"]![0]!;var c=now["characters"]![0]!;
        foreach(string key in new[]{"palette","ticksPerSecond"})
            if(!JsonNode.DeepEquals(old[key],now[key]))throw new Exception("Protected source setting changed: "+key);
        foreach(string key in new[]{"layers","parts","width","height","origin","pivot"})
            if(!JsonNode.DeepEquals(oldC[key],c[key]))throw new Exception("Protected rig setting changed: "+key);
        var frames=c["views"]![0]!["frames"]!.AsArray().ToDictionary(f=>f!["id"]!.GetValue<string>());
        foreach(var f in oldC["views"]![0]!["frames"]!.AsArray())
            if(!JsonNode.DeepEquals(f,frames[f!["id"]!.GetValue<string>()]))throw new Exception("Protected frame changed");
        foreach(string clip in new[]{"idle","run","prepare","jump","fall","land","jump_sequence","attack"})
            if(!JsonNode.DeepEquals(oldC["animations"]!.AsArray().Single(a=>a!["id"]!.GetValue<string>()==clip),
                c["animations"]!.AsArray().Single(a=>a!["id"]!.GetValue<string>()==clip)))throw new Exception("Protected clip changed");
        if(frames.Keys.Count(id=>id.StartsWith("cross_"))!=8)
            throw new Exception("Cross must contain exactly eight poses");
        var poses=JsonNode.Parse(File.ReadAllText("art/poses.json"))!["frames"]!.AsArray();
        var oldPoses=JsonNode.Parse(Git("show",baseline+":art/poses.json"))!["frames"]!.AsArray();
        for(int i=0;i<29;i++)if(!JsonNode.DeepEquals(poses[i],oldPoses[i]))throw new Exception("Protected attachment pose changed");
        var idle=JsonSerializer.Deserialize<Dictionary<string,Placement>>(poses[0]!["parts"]!.ToJsonString())!;
        double maxError=0;
        foreach(var pose in poses.Skip(29))
        {
            var parts=JsonSerializer.Deserialize<Dictionary<string,Placement>>(pose!["parts"]!.ToJsonString())!;
            foreach(string foot in new[]{"near_foot","far_foot"})
                if(parts[foot]!=idle[foot])throw new Exception("Cross foot contact moved");
            foreach(string id in LayerOrder.Where(id=>idle[id].End!=null))
            {
                double error=Math.Abs(Length(parts[id].Start,parts[id].End!)-Length(idle[id].Start,idle[id].End!));
                maxError=Math.Max(maxError,error);if(error>1.42)throw new Exception("Part length changed: "+id);
            }
            foreach(string id in LayerOrder)
            {
                var cells=frames[pose["id"]!.GetValue<string>()]!["cels"]![id]!["grid"]!["cells"]!;
                for(int k=0;k<128;k++)if(cells[k]!=null||cells[127*128+k]!=null||cells[k*128]!=null||cells[k*128+127]!=null)
                    throw new Exception("Attack art touches canvas edge: "+id);
            }
        }
        var fx=JsonNode.Parse(File.ReadAllText("art/cross-effects.pixel.json"))!["characters"]![0]!;
        var attack=c["animations"]!.AsArray().Single(a=>a!["id"]!.GetValue<string>()=="cross")!;
        var effectAttack=fx["animations"]![0]!;
        if(!JsonNode.DeepEquals(attack["frames"],effectAttack["frames"]))throw new Exception("Effect/body timing mismatch");
        if(attack["frames"]!.AsArray().Select((f,i)=>f!["durationTicks"]!.GetValue<int>()==CrossTicks[i]).Any(ok=>!ok))
            throw new Exception("Reference timing changed");
        const string bridge="""
        const fs=require('fs'),path=require('path'),cp=require('child_process'),crypto=require('crypto');
        const {PNG}=require(path.join(path.dirname(process.argv[1]),'../../../node_modules/pngjs'));
        const baseline=process.argv[2],sha=b=>crypto.createHash('sha256').update(b).digest('hex'),parts=[];
        for(const name of fs.readdirSync('art/exports/layers').filter(n=>n.endsWith('.png'))){
            const file='art/exports/layers/'+name;
            const old=PNG.sync.read(cp.execFileSync('git',['show',baseline+':'+file]));
            const now=PNG.sync.read(fs.readFileSync(file));
            if(now.width!==37*128||now.height!==128)throw Error('Invalid atlas');
            const a=[],b=[];
            for(let y=0;y<128;y++){
                a.push(old.data.subarray(y*old.width*4,(y*old.width+29*128)*4));
                b.push(now.data.subarray(y*now.width*4,(y*now.width+29*128)*4));
            }
            if(sha(Buffer.concat(a))!==sha(Buffer.concat(b)))throw Error('Protected pixels changed: '+name);
            const pix=PNG.sync.read(fs.readFileSync('art/pixelloid/layers/'+name));
            const game=PNG.sync.read(fs.readFileSync('game/assets/soldier_frames/'+name));
            if(sha(now.data)!==sha(pix.data)||sha(now.data)!==sha(game.data))throw Error('Import mismatch: '+name);
            parts.push({part:name,protectedRgbaSha256:sha(Buffer.concat(a))});
        }
        const effects=['art/exports/effects/cross.png','art/pixelloid/effects/cross.png','game/assets/soldier_effects/cross.png']
            .map(p=>PNG.sync.read(fs.readFileSync(p)));
        if(effects.some(p=>p.width!==1280||p.height!==128||sha(p.data)!==sha(effects[0].data)))throw Error('Effect import mismatch');
        const counts=[];
        for(let f=0;f<8;f++){
            let count=0;for(let y=0;y<128;y++)for(let x=0;x<160;x++){
                const alpha=effects[0].data[(y*1280+f*160+x)*4+3];
                if(alpha){count++;if(x===0||x===159||y===0||y===127)throw Error('Clipped effect');}
            }
            if((f<2&&count)||(f>=2&&!count))throw Error('Unexpected effect phase');counts.push(count);
        }
        fs.writeFileSync('art/CROSS_VERIFICATION.json',JSON.stringify({baseline,protectedClips:['idle','run','prepare','jump','fall','land','jump_sequence','attack'],
            protectedPartFrames:464,crossFrames:8,durationMs:450,maximumBoneRoundingPixels:Number(process.argv[3]),
            plantedFeet:true,effectRgbaSha256:sha(effects[0].data),effectPixelCounts:counts,parts},null,2)+'\n');
        console.log('PASS: 464 idle/walk/jump part frames unchanged; Pixelloid and game imports match.');
        """;
        var info=new ProcessStartInfo("node");foreach(string arg in new[]{"-e",bridge,Pix,baseline,maxError.ToString(System.Globalization.CultureInfo.InvariantCulture)})info.ArgumentList.Add(arg);
        using var check=Process.Start(info)!;check.WaitForExit();if(check.ExitCode!=0)throw new Exception("Fight atlas verification failed");
        Console.WriteLine($"PASS: exactly eight cross poses in bounds; maximum bone-length rounding {maxError:F3} px.");
    }
}
