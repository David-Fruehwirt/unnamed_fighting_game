using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PixelAuthor;

public static partial class Program
{
    static void VerifyJump()
    {
        const string baseline="440d39c";
        string Git(params string[] args)
        {
            var info=new ProcessStartInfo("git"){RedirectStandardOutput=true,RedirectStandardError=true};
            foreach(string arg in args)info.ArgumentList.Add(arg);
            using var process=Process.Start(info)!;string result=process.StandardOutput.ReadToEnd(),error=process.StandardError.ReadToEnd();
            process.WaitForExit();if(process.ExitCode!=0)throw new Exception("Jump preservation failed: "+result+error);return result;
        }
        var paths=new List<string>{"art/parts","art/draw","art/palette.json"};
        foreach(string clip in new[]{"idle","run"})paths.AddRange(new[]{
            $"art/exports/animations/{clip}.png",$"art/exports/animations/{clip}.json",$"art/previews/{clip}.gif"});
        Git(new[]{"diff","--exit-code",baseline,"--"}.Concat(paths).ToArray());
        var old=JsonNode.Parse(Git("show",baseline+":art/soldier.pixel.json"))!;
        var now=JsonNode.Parse(File.ReadAllText("art/soldier.pixel.json"))!;
        var oldC=old["characters"]![0]!;var c=now["characters"]![0]!;
        var frames=c["views"]![0]!["frames"]!.AsArray().ToDictionary(f=>f!["id"]!.GetValue<string>());
        foreach(var f in oldC["views"]![0]!["frames"]!.AsArray().Where(f=>{
            string id=f!["id"]!.GetValue<string>();return id.StartsWith("idle_")||id.StartsWith("run_")||id=="neutral";}))
            if(!JsonNode.DeepEquals(f,frames[f!["id"]!.GetValue<string>()]))throw new Exception("Protected frame changed");
        foreach(string clip in new[]{"idle","run"})
            if(!JsonNode.DeepEquals(oldC["animations"]!.AsArray().Single(a=>a!["id"]!.GetValue<string>()==clip),
                c["animations"]!.AsArray().Single(a=>a!["id"]!.GetValue<string>()==clip)))throw new Exception("Protected clip changed");
        foreach(string id in new[]{"jump_pose_00","jump_pose_11"})
            if(!JsonNode.DeepEquals(frames[id]!["cels"],frames["idle_0"]!["cels"]))throw new Exception("Jump endpoint differs from idle");
        var poses=JsonNode.Parse(File.ReadAllText("art/poses.json"))!["frames"]!.AsArray();
        var oldPoses=JsonNode.Parse(Git("show",baseline+":art/poses.json"))!["frames"]!.AsArray();
        for(int i=0;i<16;i++)if(!JsonNode.DeepEquals(poses[i],oldPoses[i]))throw new Exception("Protected attachment pose changed");
        var idle=JsonSerializer.Deserialize<Dictionary<string,Placement>>(poses[0]!["parts"]!.ToJsonString())!;
        double maxError=0;
        foreach(var pose in poses.Skip(16))
        {
            var parts=JsonSerializer.Deserialize<Dictionary<string,Placement>>(pose!["parts"]!.ToJsonString())!;
            foreach(string id in LayerOrder.Where(id=>idle[id].End!=null))
            {
                double error=Math.Abs(Length(parts[id].Start,parts[id].End!)-Length(idle[id].Start,idle[id].End!));
                maxError=Math.Max(maxError,error);if(error>1.42)throw new Exception("Part length changed: "+id);
            }
            foreach(string id in LayerOrder)
            {
                var cells=frames[pose["id"]!.GetValue<string>()]!["cels"]![id]!["grid"]!["cells"]!;
                for(int k=0;k<128;k++)if(cells[k]!=null||cells[127*128+k]!=null||cells[k*128]!=null||cells[k*128+127]!=null)
                    throw new Exception("Jump art touches canvas edge: "+id);
            }
        }
        const string bridge="""
        const fs=require('fs'),path=require('path'),cp=require('child_process'),crypto=require('crypto');
        const {PNG}=require(path.join(path.dirname(process.argv[1]),'../../../node_modules/pngjs'));
        const baseline=process.argv[2],sha=b=>crypto.createHash('sha256').update(b).digest('hex'),parts=[];
        for(const name of fs.readdirSync('art/exports/layers').filter(n=>n.endsWith('.png'))){
            const file='art/exports/layers/'+name;
            const old=PNG.sync.read(cp.execFileSync('git',['show',baseline+':'+file]));
            const now=PNG.sync.read(fs.readFileSync(file));
            if(now.width!==28*128||now.height!==128)throw Error('Invalid atlas');
            const a=[],b=[];
            for(let y=0;y<128;y++){
                a.push(old.data.subarray(y*old.width*4,(y*old.width+16*128)*4));
                b.push(now.data.subarray(y*now.width*4,(y*now.width+16*128)*4));
            }
            if(sha(Buffer.concat(a))!==sha(Buffer.concat(b)))throw Error('Protected pixels changed: '+name);
            const pix=PNG.sync.read(fs.readFileSync('art/pixelloid/layers/'+name));
            const game=PNG.sync.read(fs.readFileSync('game/assets/soldier_frames/'+name));
            if(sha(now.data)!==sha(pix.data)||sha(now.data)!==sha(game.data))throw Error('Import mismatch: '+name);
            parts.push({part:name,protectedRgbaSha256:sha(Buffer.concat(a))});
        }
        fs.writeFileSync('art/JUMP_VERIFICATION.json',JSON.stringify({baseline,protectedClips:['idle','run'],
            protectedPartFrames:256,jumpFrames:12,parts},null,2)+'\n');
        console.log('PASS: 256 idle/walk part frames unchanged; Pixelloid and game imports match.');
        """;
        var info=new ProcessStartInfo("node");foreach(string arg in new[]{"-e",bridge,Pix,baseline})info.ArgumentList.Add(arg);
        using var check=Process.Start(info)!;check.WaitForExit();if(check.ExitCode!=0)throw new Exception("Jump atlas verification failed");
        Console.WriteLine($"PASS: all 12 poses in bounds; prepare/idle endpoints exact; maximum bone-length rounding {maxError:F3} px.");
    }
}
