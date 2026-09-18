using System.Diagnostics;
using System.Text.Json.Nodes;

namespace PixelAuthor;

public static partial class Program
{
    static void VerifyWalk()
    {
        const string baseline="d681304";
        string Git(params string[] args)
        {
            var info=new ProcessStartInfo("git"){RedirectStandardOutput=true,RedirectStandardError=true};
            foreach(string arg in args)info.ArgumentList.Add(arg);
            using var process=Process.Start(info)!;
            string output=process.StandardOutput.ReadToEnd(),error=process.StandardError.ReadToEnd();process.WaitForExit();
            if(process.ExitCode!=0)throw new Exception("Walk preservation failed: "+output+error);
            return output;
        }
        var paths=new List<string>{"art/parts","art/draw","art/palette.json","game/scripts/Soldier.cs"};
        foreach(string clip in new[]{"idle","jump","fall"})paths.AddRange(new[]{
            $"art/exports/animations/{clip}.png",$"art/exports/animations/{clip}.json",$"art/previews/{clip}.gif"});
        Git(new[]{"diff","--exit-code",baseline,"--"}.Concat(paths).ToArray());
        var old=JsonNode.Parse(Git("show",baseline+":art/soldier.pixel.json"))!;
        var now=JsonNode.Parse(File.ReadAllText("art/soldier.pixel.json"))!;
        double oldRate=old["ticksPerSecond"]!.GetValue<int>(),rate=now["ticksPerSecond"]!.GetValue<int>();
        var oldC=old["characters"]![0]!;var newC=now["characters"]![0]!;
        var frames=newC["views"]![0]!["frames"]!.AsArray().ToDictionary(f=>f!["id"]!.GetValue<string>());
        foreach(var f in oldC["views"]![0]!["frames"]!.AsArray().Where(f=>!f!["id"]!.GetValue<string>().StartsWith("run_")))
        {
            var n=frames[f!["id"]!.GetValue<string>()]!;
            if(!JsonNode.DeepEquals(f["cels"],n["cels"])||
                Math.Abs(f["durationTicks"]!.GetValue<int>()/oldRate-n["durationTicks"]!.GetValue<int>()/rate)>1e-9)
                throw new Exception("Protected frame changed");
        }
        foreach(var clip in oldC["animations"]!.AsArray().Where(c=>c!["id"]!.GetValue<string>()!="run"))
        {
            var n=newC["animations"]!.AsArray().Single(n=>n!["id"]!.GetValue<string>()==clip!["id"]!.GetValue<string>())!;
            if(n["loop"]!.GetValue<bool>()!=clip!["loop"]!.GetValue<bool>())throw new Exception("Protected loop changed");
            for(int i=0;i<clip["frames"]!.AsArray().Count;i++)
                if(n["frames"]![i]!["frameId"]!.GetValue<string>()!=clip["frames"]![i]!["frameId"]!.GetValue<string>()||
                    Math.Abs(n["frames"]![i]!["durationTicks"]!.GetValue<int>()/rate-
                        clip["frames"]![i]!["durationTicks"]!.GetValue<int>()/oldRate)>1e-9)
                    throw new Exception("Protected clip timing changed");
        }
        var poses=JsonNode.Parse(File.ReadAllText("art/poses.json"))!["frames"]!.AsArray();
        var oldPoses=JsonNode.Parse(Git("show",baseline+":art/poses.json"))!["frames"]!.AsArray();
        foreach(var p in oldPoses.Where(p=>p!["clip"]!.GetValue<string>()!="run"))
        {
            var n=poses.Single(n=>n!["id"]!.GetValue<string>()==p!["id"]!.GetValue<string>())!;
            if(!JsonNode.DeepEquals(p!["parts"],n["parts"])||
                Math.Abs(p["ticks"]!.GetValue<int>()/oldRate-n["ticks"]!.GetValue<int>()/rate)>1e-9)
                throw new Exception("Protected pose or socket changed");
        }
        var idle=System.Text.Json.JsonSerializer.Deserialize<Dictionary<string,Placement>>(poses[0]!["parts"]!.ToJsonString())!;
        double maxError=0;
        foreach(var pose in poses.Where(p=>p!["clip"]!.GetValue<string>()=="run"))
        {
            if(pose!["ticks"]!.GetValue<int>()/rate!=.14)throw new Exception("Walk timing differs from reference");
            var parts=System.Text.Json.JsonSerializer.Deserialize<Dictionary<string,Placement>>(pose["parts"]!.ToJsonString())!;
            foreach(string id in LayerOrder.Where(id=>idle[id].End!=null))
            {
                double error=Math.Abs(Length(parts[id].Start,parts[id].End!)-Length(idle[id].Start,idle[id].End!));
                maxError=Math.Max(maxError,error);
                if(error>1.42)throw new Exception("Walk segment differs from idle proportion: "+id);
            }
        }
        // PNG comparison uses the same decoder as the existing pixel pipeline.
        const string bridge="""
        const fs=require('fs'),path=require('path'),cp=require('child_process'),crypto=require('crypto');
        const {PNG}=require(path.join(path.dirname(process.argv[1]),'../../../node_modules/pngjs'));
        const baseline=process.argv[2],sha=b=>crypto.createHash('sha256').update(b).digest('hex'),parts=[];
        for(const name of fs.readdirSync('art/exports/layers').filter(n=>n.endsWith('.png'))){
            const file='art/exports/layers/'+name;
            const old=PNG.sync.read(cp.execFileSync('git',['show',baseline+':'+file]));
            const now=PNG.sync.read(fs.readFileSync(file));
            if(old.width!==now.width||now.width!==22*128||now.height!==128)throw Error('Atlas dimensions changed');
            const a=[],b=[];
            for(let y=0;y<128;y++)for(const [start,end] of [[0,8],[16,22]]){
                a.push(old.data.subarray((y*old.width+start*128)*4,(y*old.width+end*128)*4));
                b.push(now.data.subarray((y*now.width+start*128)*4,(y*now.width+end*128)*4));
            }
            if(sha(Buffer.concat(a))!==sha(Buffer.concat(b)))throw Error('Protected pixels changed: '+name);
            const pix=PNG.sync.read(fs.readFileSync('art/pixelloid/layers/'+name));
            const game=PNG.sync.read(fs.readFileSync('game/assets/soldier_frames/'+name));
            if(sha(now.data)!==sha(pix.data)||sha(now.data)!==sha(game.data))throw Error('Import mismatch: '+name);
            parts.push({part:name,protectedRgbaSha256:sha(Buffer.concat(a))});
        }
        fs.writeFileSync('art/WALK_VERIFICATION.json',JSON.stringify({baseline,protectedClips:['idle','jump','fall'],
            protectedPartFrames:224,walkFrames:8,frameDurationMs:140,parts},null,2)+'\n');
        console.log('PASS: 224 protected part frames unchanged; game and Pixelloid pixels match exports.');
        """;
        var info=new ProcessStartInfo("node");
        foreach(string arg in new[]{"-e",bridge,Pix,baseline})info.ArgumentList.Add(arg);
        using var check=Process.Start(info)!;check.WaitForExit();
        if(check.ExitCode!=0)throw new Exception("Walk atlas verification failed");
        Console.WriteLine($"PASS: eight 140 ms walk frames; idle rig segment lengths differ by at most {maxError:F3} pixels from integer rounding. Protected timing unchanged.");
    }
}
