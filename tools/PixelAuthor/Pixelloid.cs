using System.Diagnostics;

namespace PixelAuthor;

public static partial class Program
{
    static void ProcessPixelloid()
    {
        string checkout = Environment.GetEnvironmentVariable("PIXELLOID_SOURCE") ??
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".codex", "tools", "pixelloid");
        if (!Directory.Exists(checkout)) throw new Exception("Set PIXELLOID_SOURCE to the Pixelloid source checkout.");
        // The game and authoring orchestration are C#. This bridge executes Pixelloid's own
        // TypeScript conversion engine, rather than a reimplementation of its algorithm.
        const string bridge = """
        const fs = require('fs'), path = require('path'), crypto = require('crypto'), cp = require('child_process');
        const root = process.argv[1], pixelloid = process.argv[2], pixCli = process.argv[3];
        const {PNG} = require(path.join(path.dirname(pixCli),'..','..','..','node_modules','pngjs'));
        const esbuild = require(path.join(pixelloid,'node_modules','esbuild'));
        const bundled = path.join(root,'art','work','pixelloid-core.cjs');
        esbuild.buildSync({entryPoints:[path.join(pixelloid,'src','lib','pixelizeCore.ts')],
            bundle:true,platform:'node',format:'cjs',outfile:bundled});
        const {pixelizeBuffer} = require(bundled);
        const settings = {pixelSize:1,offsetX:0,offsetY:0,samplingMode:'medoid'};
        const sha = bytes => crypto.createHash('sha256').update(bytes).digest('hex');
        const files = [];
        const walk = dir => fs.readdirSync(dir,{withFileTypes:true}).forEach(e=>{
            const p=path.join(dir,e.name);if(e.isDirectory())walk(p);else if(p.endsWith('.png'))files.push(p);
        });
        const sourceDir=path.join(root,'art','exports');
        walk(sourceDir);
        const results=[];
        for(const file of files.sort()){
            const relative=path.relative(sourceDir,file);
            // Supplied stage artwork retains its full decoded palette at native resolution.
            const nativeStage=relative.replaceAll('\\','/').startsWith('stages/');
            const source=PNG.sync.read(fs.readFileSync(file));
            const result=pixelizeBuffer({width:source.width,height:source.height,data:new Uint8ClampedArray(source.data)},settings);
            const sourceHash=sha(source.data),resultHash=sha(result.data);
            const colors=new Set();let opaque=0,partial=0;
            for(let i=0;i<result.data.length;i+=4){
                const alpha=result.data[i+3];
                if(alpha!==0&&alpha!==255)partial++;
                if(alpha){opaque++;colors.add(Array.from(result.data.slice(i,i+4)).join(','));}
            }
            if(source.width!==result.width||source.height!==result.height||sourceHash!==resultHash||partial||(!nativeStage&&colors.size>12))
                throw new Error('Pixelloid validation failed: '+file);
            const output=path.join(root,'art','pixelloid',relative);
            fs.mkdirSync(path.dirname(output),{recursive:true});
            const png=new PNG({width:result.width,height:result.height});
            png.data=Buffer.from(result.data);
            fs.writeFileSync(output,PNG.sync.write(png));
            results.push({file:relative.replaceAll('\\','/'),width:result.width,height:result.height,
                colors:colors.size,opaquePixels:opaque,partialAlphaPixels:partial,
                sourceRgbaSha256:sourceHash,processedRgbaSha256:resultHash,identity:result.passthrough});
        }
        const revision=cp.execFileSync('git',['-C',pixelloid,'rev-parse','HEAD'],{encoding:'utf8'}).trim();
        const report={tool:'Pixelloid',version:JSON.parse(fs.readFileSync(path.join(pixelloid,'package.json'))).version,
            source:'https://github.com/retrogarage/pixelloid',revision,settings,
            explanation:'Already-authored 1:1 pixels. Pixelloid identity processing preserves every RGBA pixel; no AI generation, smoothing, resizing or palette substitution.',
            files:results};
        fs.writeFileSync(path.join(root,'art','PIXELLOID_REPORT.json'),JSON.stringify(report,null,2)+'\n');
        console.log('Pixelloid verified and exported '+results.length+' PNGs; all RGBA hashes match.');
        """;
        var info = new ProcessStartInfo("node");
        info.ArgumentList.Add("-e");
        info.ArgumentList.Add(bridge);
        info.ArgumentList.Add(Root);
        info.ArgumentList.Add(checkout);
        info.ArgumentList.Add(Pix);
        using var process = Process.Start(info)!;
        process.WaitForExit();
        if (process.ExitCode != 0) throw new Exception("Pixelloid processing failed; do not import assets into Godot.");
    }
}
