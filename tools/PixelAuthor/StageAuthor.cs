using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PixelAuthor;

public static partial class Program
{
    static void StageBridge(string script)
    {
        var info=new ProcessStartInfo("node");
        foreach(string arg in new[]{"-e",script,Pix})info.ArgumentList.Add(arg);
        using var p=Process.Start(info)!;p.WaitForExit();if(p.ExitCode!=0)throw new Exception("Stage image bridge failed");
    }

    static void BuildStage()
    {
        Directory.CreateDirectory("art/stages");Directory.CreateDirectory("art/exports/stages");
        const string source="reference_pics/stage_1.jpg",file="art/stages/stage_1.pixel.json";
        // Sharp only decodes JPEG. All mask decisions and editable pixels are authored in C#.
        StageBridge("""
        const fs=require('fs'),path=require('path');
        const sharp=require(path.join(path.dirname(process.argv[1]),'../../../node_modules/sharp'));
        sharp('reference_pics/stage_1.jpg').ensureAlpha().raw().toBuffer({resolveWithObject:true}).then(({data,info})=>{
            if(info.width!==1024||info.height!==559)throw Error('Unexpected reference dimensions');
            fs.writeFileSync('art/work/stage-input.rgba',data);
        }).catch(e=>{console.error(e);process.exitCode=1;});
        """);
        byte[] rgba=File.ReadAllBytes("art/work/stage-input.rgba");
        const int sw=1024,sh=559,cropX=68,cropY=72,width=888,height=448;
        var background=new bool[sw*sh];var queue=new Queue<int>();
        bool Checker(int i)
        {
            int r=rgba[i*4],g=rgba[i*4+1],b=rgba[i*4+2];
            return Math.Max(r,Math.Max(g,b))-Math.Min(r,Math.Min(g,b))<=14 && r+g+b>=114 && r+g+b<=510;
        }
        void Seed(int i){if(!background[i]&&Checker(i)){background[i]=true;queue.Enqueue(i);}}
        for(int x=0;x<sw;x++){Seed(x);Seed((sh-1)*sw+x);}
        for(int y=0;y<sh;y++){Seed(y*sw);Seed(y*sw+sw-1);}
        // Reference-visible holes enclosed by the crane cables and wrench/rocket silhouettes.
        foreach(var p in new[]{new P(140,202),new P(189,190),new P(202,215),new P(323,143)})Seed(p.Y*sw+p.X);
        while(queue.TryDequeue(out int i))
        {
            int x=i%sw,y=i/sw;
            if(x>0)Seed(i-1);if(x<sw-1)Seed(i+1);if(y>0)Seed(i-sw);if(y<sh-1)Seed(i+sw);
        }
        // Keep the connected platform; isolated JPEG edge noise in the background is not artwork.
        var visited=new bool[sw*sh];var largest=new List<int>();
        for(int start=0;start<background.Length;start++)if(!background[start]&&!visited[start])
        {
            var component=new List<int>();queue.Enqueue(start);visited[start]=true;
            void Add(int p){if(!background[p]&&!visited[p]){visited[p]=true;queue.Enqueue(p);}}
            while(queue.TryDequeue(out int i))
            {
                component.Add(i);int x=i%sw,y=i/sw;
                if(x>0)Add(i-1);if(x<sw-1)Add(i+1);if(y>0)Add(i-sw);if(y<sh-1)Add(i+sw);
            }
            if(component.Count>largest.Count)largest=component;
        }
        var keep=new bool[sw*sh];foreach(int i in largest)keep[i]=true;
        var colors=new Dictionary<string,string>();var cells=new string?[width*height];var output=new byte[width*height*4];
        int retained=0;
        for(int y=0;y<height;y++)for(int x=0;x<width;x++)
        {
            int src=(y+cropY)*sw+x+cropX,dst=y*width+x;
            // Palette includes all crop colors so source IDs stay stable when refining the mask.
            string hex=$"{rgba[src*4]:x2}{rgba[src*4+1]:x2}{rgba[src*4+2]:x2}",token="rgb_"+hex;
            colors.TryAdd(token,"#"+hex);
            if(!keep[src])continue;
            cells[dst]=token;Array.Copy(rgba,src*4,output,dst*4,4);retained++;
        }
        if(retained<150000||retained>350000)throw new Exception("Stage mask needs inspection: unexpected area "+retained);
        if(!File.Exists(file))
        {
            var project=Project("stage_1",width,height,["stage"]);
            project["palette"]=JsonSerializer.SerializeToNode(colors.Select(c=>new{id=c.Key,name=c.Key,color=c.Value}));
            project["metadata"]=JsonSerializer.SerializeToNode(new{reference=source,method="Original JPEG RGB; C# binary checkerboard mask; 1:1 pixels; no quantization"});
            File.WriteAllText(file,project.ToJsonString());
        }
        File.WriteAllText("art/work/stage-inspection.json",RunPix("inspect",file,"--json"));
        var existing=JsonNode.Parse(File.ReadAllText(file))!["characters"]![0]!["views"]![0]!["frames"]!.AsArray();
        var ops=new List<object>();
        foreach(var f in existing)ops.Add(new{type="removeFrame",characterId="stage_1",viewId="right",frameId=f!["id"]!.GetValue<string>()});
        ops.Add(new{type="addFrame",characterId="stage_1",viewId="right",frame=new{id="stage",name="Scrapyard",durationTicks=1,
            cels=new{stage=new{grid=new{width,height,cells},offset=new{x=0,y=0}}}}});
        Save("art/work/stage.operations.json",ops);
        Console.WriteLine(RunPix("apply",file,"--operations","art/work/stage.operations.json","--expected-hash",ProjectHash(file)));
        Console.WriteLine(RunPix("validate",file,"--json"));
        RunPix("render",file,"--frame","stage","--out","art/exports/stages/stage_1.png");
        File.WriteAllBytes("art/work/stage-expected.rgba",output);
        StageBridge("""
        const fs=require('fs'),path=require('path');
        const {PNG}=require(path.join(path.dirname(process.argv[1]),'../../../node_modules/pngjs'));
        const p=PNG.sync.read(fs.readFileSync('art/exports/stages/stage_1.png'));
        if(!p.data.equals(fs.readFileSync('art/work/stage-expected.rgba')))throw Error('Native stage pixels changed in export');
        """);
        // Source-coordinate front deck lip. The sprite and collider share the same translation.
        int[][] surface=[[114,294],[168,326],[249,326],[268,334],[418,334],[428,342],
            [642,342],[665,333],[789,331],[847,328],[934,296]];
        Save("art/stages/stage_1.layout.json",new{reference=source,crop=new{x=cropX,y=cropY,width,height},
            position=new{x=36,y=64},surface,collisionDepth=14,spawn=new{x=480,y=334},
            notes="Front deck lip traced in original JPEG coordinates. Props and hanging scrap are decoration. Runtime subtracts crop and adds sprite position for both artwork and collision."});
        Save("art/stages/STAGE_1_SOURCE_REPORT.json",new{reference=source,sourceSha256=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(source))).ToLowerInvariant(),
            width,height,retainedPixels=retained,sourceRgbPreserved=true,resampling=false,paletteReduction=false,
            mask="Border and four traced enclosed-hole seeds; channel spread <=14, RGB sum 114..510; retain largest connected foreground component.",
            exportedRgbaSha256=Convert.ToHexString(SHA256.HashData(output)).ToLowerInvariant()});
        File.WriteAllText(file,JsonNode.Parse(File.ReadAllText(file))!.ToJsonString()+"\n");
        Console.WriteLine($"Stage: {width}x{height}, {retained} retained original pixels; export RGB verified.");
    }

    static void VerifyStage()
    {
        Console.WriteLine(RunPix("validate","art/stages/stage_1.pixel.json","--json"));
        StageBridge("""
        const fs=require('fs'),path=require('path'),cp=require('child_process'),crypto=require('crypto');
        const modules=path.join(path.dirname(process.argv[1]),'../../../node_modules');
        const {PNG}=require(path.join(modules,'pngjs')),sharp=require(path.join(modules,'sharp'));
        const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
        cp.execFileSync('git',['diff','--exit-code','5f438cc','--','art/parts','art/draw','art/palette.json',
            'art/soldier.pixel.json','art/poses.json','art/fight-effects.pixel.json','art/exports/layers',
            'art/exports/animations','art/exports/effects','game/assets/soldier_frames','game/assets/soldier_effects']);
        const layout=JSON.parse(fs.readFileSync('art/stages/stage_1.layout.json'));
        if(!fs.readFileSync('art/stages/stage_1.layout.json').equals(fs.readFileSync('game/assets/stage/stage_1.layout.json')))
            throw Error('Game layout differs from authored trace');
        const images=['art/exports/stages/stage_1.png','art/pixelloid/stages/stage_1.png','game/assets/stage/stage_1.png']
            .map(p=>PNG.sync.read(fs.readFileSync(p)));
        const img=images[0],{crop,position,surface}=layout;
        if(images.some(p=>p.width!==crop.width||p.height!==crop.height||sha(p.data)!==sha(img.data)))throw Error('Stage export/import mismatch');
        sharp('reference_pics/stage_1.jpg').ensureAlpha().raw().toBuffer({resolveWithObject:true}).then(({data,info})=>{
            let retained=0;
            for(let y=0;y<img.height;y++)for(let x=0;x<img.width;x++){
                const d=(y*img.width+x)*4,s=((y+crop.y)*info.width+x+crop.x)*4,a=img.data[d+3];
                if(a!==0&&a!==255)throw Error('Unexpected partial alpha');
                if(!a)continue;
                retained++;
                if(x===0||y===0||x===img.width-1||y===img.height-1)throw Error('Foreground clipped by crop');
                for(let c=0;c<3;c++)if(img.data[d+c]!==data[s+c])throw Error('Original foreground color changed');
            }
            for(const [x,y] of surface)if(img.data[((y-crop.y)*img.width+x-crop.x)*4+3]!==255)
                throw Error('Collision trace point is outside visible artwork');
            const result={baseline:'5f438cc',soldierAssetsUnchanged:true,width:img.width,height:img.height,
                retainedOriginalPixels:retained,originalRgbPreserved:true,noResampling:true,binaryTransparency:true,
                croppedWithoutClipping:true,exportPixelloidGameRgbaSha256:sha(img.data),
                sourceSha256:sha(fs.readFileSync('reference_pics/stage_1.jpg')),
                deckLeft:surface[0][0]-crop.x+position.x,deckRight:surface.at(-1)[0]-crop.x+position.x,
                deckSections:surface.length-1,sharedLayout:true};
            fs.writeFileSync('art/stages/STAGE_1_VERIFICATION.json',JSON.stringify(result,null,2)+'\n');
            console.log('PASS: '+retained+' original pixels retained; no clipping/resampling; Pixelloid and Godot files match; all Soldier assets unchanged.');
        }).catch(e=>{console.error(e);process.exitCode=1;});
        """);
    }
}
