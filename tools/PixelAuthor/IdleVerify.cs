using System.Diagnostics;
using System.Text.Json.Nodes;

namespace PixelAuthor;

public static partial class Program
{
    static void VerifyIdle()
    {
        const string baseline="d9709b9993d400836a2ea9b0818618a01d229047";
        string Git(params string[] args)
        {
            var info=new ProcessStartInfo("git"){RedirectStandardOutput=true,RedirectStandardError=true};
            foreach(string arg in args)info.ArgumentList.Add(arg);
            using var process=Process.Start(info)!;
            string output=process.StandardOutput.ReadToEnd(),errors=process.StandardError.ReadToEnd();
            process.WaitForExit();
            if(process.ExitCode!=0)throw new Exception("Idle preservation check failed: "+output+errors);
            return output;
        }
        var protectedPaths=new List<string>{"art/parts","art/draw","art/palette.json","game/scripts/Soldier.cs"};
        foreach(string clip in new[]{"run","jump","fall"})
            protectedPaths.AddRange(new[]{ $"art/exports/animations/{clip}.png",$"art/exports/animations/{clip}.json",
                $"art/previews/{clip}.gif",$"art/pixelloid/animations/{clip}.png" });
        Git(new[]{"diff","--exit-code",baseline,"--"}.Concat(protectedPaths).ToArray());
        var old=JsonNode.Parse(Git("show",baseline+":art/soldier.pixel.json"))!["characters"]![0]!;
        var current=JsonNode.Parse(File.ReadAllText("art/soldier.pixel.json"))!["characters"]![0]!;
        var frames=current["views"]![0]!["frames"]!.AsArray().ToDictionary(f=>f!["id"]!.GetValue<string>());
        int preserved=0;
        foreach(var frame in old["views"]![0]!["frames"]!.AsArray())
        {
            string id=frame!["id"]!.GetValue<string>();
            if(id.StartsWith("idle_")||id=="neutral")continue;
            if(!JsonNode.DeepEquals(frame,frames[id]))throw new Exception("Changed source frame: "+id);
            preserved++;
        }
        foreach(var clip in old["animations"]!.AsArray().Where(c=>c!["id"]!.GetValue<string>()!="idle"))
            if(!JsonNode.DeepEquals(clip,current["animations"]!.AsArray().Single(c=>c!["id"]!.GetValue<string>()==clip!["id"]!.GetValue<string>())))
                throw new Exception("Changed non-idle timing");
        var oldPoses=JsonNode.Parse(Git("show",baseline+":art/poses.json"))!["frames"]!.AsArray();
        var poses=JsonNode.Parse(File.ReadAllText("art/poses.json"))!["frames"]!.AsArray();
        foreach(var p in oldPoses.Where(p=>p!["clip"]!.GetValue<string>()!="idle"))
            if(!JsonNode.DeepEquals(p,poses.Single(n=>n!["id"]!.GetValue<string>()==p!["id"]!.GetValue<string>())))
                throw new Exception("Changed non-idle attachment pose");
        // Use the pixel tool's PNG decoder to compare every RGBA byte in the
        // fourteen preserved frames of all sixteen atlases, including transparency.
        const string compare="""
        const fs=require('fs'),path=require('path'),cp=require('child_process'),crypto=require('crypto');
        const {PNG}=require(path.join(path.dirname(process.argv[1]),'../../../node_modules/pngjs'));
        const baseline=process.argv[2], reports=[];
        const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
        for(const name of fs.readdirSync('art/exports/layers').filter(n=>n.endsWith('.png'))){
            const file='art/exports/layers/'+name;
            const before=PNG.sync.read(cp.execFileSync('git',['show',baseline+':'+file]));
            const after=PNG.sync.read(fs.readFileSync(file));
            if(before.width!==18*128||after.width!==22*128||after.height!==128)throw Error('Bad atlas dimensions');
            const a=[],b=[];
            for(let y=0;y<128;y++){
                a.push(before.data.subarray((y*before.width+4*128)*4,(y*before.width+18*128)*4));
                b.push(after.data.subarray((y*after.width+8*128)*4,(y*after.width+22*128)*4));
            }
            const oldHash=sha(Buffer.concat(a)),newHash=sha(Buffer.concat(b));
            if(oldHash!==newHash)throw Error('Non-idle pixels changed: '+name);
            const processed=PNG.sync.read(fs.readFileSync('art/pixelloid/layers/'+name));
            const game=PNG.sync.read(fs.readFileSync('game/assets/soldier_frames/'+name));
            if(sha(after.data)!==sha(processed.data)||sha(after.data)!==sha(game.data))throw Error('Import mismatch: '+name);
            reports.push({part:name,nonIdleFrames:14,beforeRgbaSha256:oldHash,afterRgbaSha256:newHash});
        }
        fs.writeFileSync('art/IDLE_VERIFICATION.json',JSON.stringify({baseline,
            protectedSourceFrames:14,protectedPartFrames:224,unchangedClips:['run','jump','fall'],
            compared:'Source cels, pose metadata, clip timings, standalone exports, and atlas RGBA pixels',parts:reports},null,2)+'\n');
        console.log('PASS: all 224 non-idle part frames match baseline; Pixelloid and game atlases match exports.');
        """;
        var node=new ProcessStartInfo("node");
        foreach(string arg in new[]{"-e",compare,Pix,baseline})node.ArgumentList.Add(arg);
        using var check=Process.Start(node)!;
        check.WaitForExit();
        if(check.ExitCode!=0)throw new Exception("Atlas preservation failed");
        Console.WriteLine($"PASS: {preserved} non-idle source frames, clip timings, poses, original designs and controller match {baseline[..7]}.");
    }
}
