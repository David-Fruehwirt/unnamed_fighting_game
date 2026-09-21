using System.Text.Json;
using System.Text.Json.Nodes;

namespace PixelAuthor;

public static partial class Program
{
    static void BuildPixelStage()
    {
        const string master="art/stages/stage_1.pixel.json",file="art/stages/stage_1.pixelized.pixel.json";
        Console.WriteLine(RunPix("validate",master,"--json"));
        RunPix("render",master,"--frame","stage","--out","art/work/stage-master.png");
        StageBridge("""
        const fs=require('fs'),path=require('path'),crypto=require('crypto'),cp=require('child_process');
        const {PNG}=require(path.join(path.dirname(process.argv[1]),'../../../node_modules/pngjs'));
        const checkout=process.env.PIXELLOID_SOURCE||path.join(process.env.USERPROFILE,'.codex/tools/pixelloid');
        const esbuild=require(path.join(checkout,'node_modules/esbuild'));
        const bundle=path.resolve('art/work/stage-pixelizer.cjs');
        esbuild.buildSync({entryPoints:[path.join(checkout,'src/lib/pixelizeCore.ts')],bundle:true,platform:'node',format:'cjs',outfile:bundle});
        const {pixelizeBuffer}=require(bundle),master=PNG.sync.read(fs.readFileSync('art/work/stage-master.png'));
        const input=new PNG({width:900,height:450});input.data.fill(0);
        for(let y=0;y<448;y++)master.data.copy(input.data,((y+1)*900+6)*4,y*888*4,(y+1)*888*4);
        const settings={pixelSize:3,offsetX:0,offsetY:0,samplingMode:'medoid'};
        const result=pixelizeBuffer({width:900,height:450,data:new Uint8ClampedArray(input.data)},settings);
        if(result.width!==300||result.height!==150||result.passthrough)throw Error('Pixelizer did not reduce the stage');
        const pixels=Buffer.from(result.data);
        for(let i=0;i<pixels.length;i+=4){if(pixels[i+3]!==0&&pixels[i+3]!==255)throw Error('Partial alpha');if(!pixels[i+3])pixels.fill(0,i,i+4);}
        fs.writeFileSync('art/work/stage-low.rgba',pixels);
        const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
        fs.writeFileSync('art/stages/STAGE_1_PIXELIZATION.json',JSON.stringify({tool:'Pixelloid',
            revision:cp.execFileSync('git',['-C',checkout,'rev-parse','HEAD'],{encoding:'utf8'}).trim(),settings,
            reference:'reference_pics/stage_1.jpg',input:[900,450],padding:{left:6,top:1,right:6,bottom:1},
            output:[300,150],displayScale:2,passthrough:false,inputRgbaSha256:sha(input.data),outputRgbaSha256:sha(pixels)},null,2)+'\n');
        """);
        byte[] rgba=File.ReadAllBytes("art/work/stage-low.rgba");
        var palette=new Dictionary<string,string>();var cells=new string?[300*150];
        for(int i=0;i<cells.Length;i++)if(rgba[i*4+3]!=0)
        {
            string hex=$"{rgba[i*4]:x2}{rgba[i*4+1]:x2}{rgba[i*4+2]:x2}";
            cells[i]="rgb_"+hex;palette.TryAdd(cells[i]!,"#"+hex);
        }
        if(!File.Exists(file))
        {
            var project=Project("stage_pixel",300,150,["stage"]);
            project["palette"]=JsonSerializer.SerializeToNode(palette.Select(p=>new{id=p.Key,name=p.Key,color=p.Value}));
            File.WriteAllText(file,project.ToJsonString());
        }
        File.WriteAllText("art/work/stage-pixel-inspection.json",RunPix("inspect",file,"--json"));
        var source=JsonNode.Parse(File.ReadAllText(file))!;
        var ops=new List<object>();
        foreach(var f in source["characters"]![0]!["views"]![0]!["frames"]!.AsArray())
            ops.Add(new{type="removeFrame",characterId="stage_pixel",viewId="right",frameId=f!["id"]!.GetValue<string>()});
        ops.Add(new{type="addFrame",characterId="stage_pixel",viewId="right",frame=new{id="stage",name="Pixel scrapyard",durationTicks=1,
            cels=new{stage=new{grid=new{width=300,height=150,cells},offset=new{x=0,y=0}}}}});
        Save("art/work/stage-pixel.operations.json",ops);
        Console.WriteLine(RunPix("apply",file,"--operations","art/work/stage-pixel.operations.json","--expected-hash",ProjectHash(file)));
        Console.WriteLine(RunPix("validate",file,"--json"));
        RunPix("render",file,"--frame","stage","--out","art/previews/stage-logical.png");
        RunPix("render",file,"--frame","stage","--scale","2","--out","art/exports/stages/stage_1.png");
        RunPix("render",file,"--frame","stage","--scale","2","--out","art/previews/stage_1.png");
        // Original deck endpoints mapped through padding, /3 sampling and exact 2x export.
        Save("art/stages/stage_1.layout.json",new{reference="reference_pics/stage_1.jpg",
            crop=new{x=0,y=0,width=600,height=300},position=new{x=180,y=186},
            surface=new[]{new[]{34,148},new[]{582,148}},collisionDepth=14,spawn=new{x=480,y=334},
            notes="Display-pixel coordinates: flat invisible surface at reference row 294 across the widest deck section. Stage artwork is 300x150 logical pixels exported at 2x."});
        File.WriteAllText(file,JsonNode.Parse(File.ReadAllText(file))!.ToJsonString()+"\n");
    }

    static void VerifyPixelStage()
    {
        Console.WriteLine(RunPix("validate","art/stages/stage_1.pixelized.pixel.json","--json"));
        StageBridge("""
        const fs=require('fs'),path=require('path'),cp=require('child_process'),crypto=require('crypto');
        const {PNG}=require(path.join(path.dirname(process.argv[1]),'../../../node_modules/pngjs'));
        cp.execFileSync('git',['diff','--exit-code','440eef7','--','art/parts','art/draw','art/palette.json',
            'art/soldier.pixel.json','art/poses.json','art/fight-effects.pixel.json','art/exports/layers','art/exports/animations',
            'art/exports/effects','game/assets/soldier_frames','game/assets/soldier_effects','game/scripts/Soldier.cs','game/scenes/soldier.tscn',
            'art/stages/stage_1.pixel.json']);
        const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
        const images=['art/exports/stages/stage_1.png','art/pixelloid/stages/stage_1.png','game/assets/stage/stage_1.png'].map(p=>PNG.sync.read(fs.readFileSync(p)));
        const img=images[0],low=PNG.sync.read(fs.readFileSync('art/previews/stage-logical.png'));
        if(images.some(p=>p.width!==600||p.height!==300||sha(p.data)!==sha(img.data)))throw Error('Stage size/import mismatch');
        const pixelization=JSON.parse(fs.readFileSync('art/stages/STAGE_1_PIXELIZATION.json'));
        if(pixelization.passthrough||pixelization.settings.pixelSize!==3||sha(low.data)!==pixelization.outputRgbaSha256)throw Error('Missing real pixelization');
        for(let y=0;y<300;y++)for(let x=0;x<600;x++){
            const i=(y*600+x)*4,j=(Math.floor(y/2)*300+Math.floor(x/2))*4;
            for(let c=0;c<4;c++)if(img.data[i+c]!==low.data[j+c])throw Error('Broken 2x2 pixel block');
            if(img.data[i+3]!==0&&img.data[i+3]!==255)throw Error('Partial alpha');
        }
        const layout=JSON.parse(fs.readFileSync('art/stages/stage_1.layout.json'));
        if(!fs.readFileSync('art/stages/stage_1.layout.json').equals(fs.readFileSync('game/assets/stage/stage_1.layout.json')))throw Error('Layout mismatch');
        if(layout.surface.length!==2||layout.surface.some(p=>p[1]+layout.position.y!==334))throw Error('Deck is not flat');
        fs.writeFileSync('art/stages/STAGE_1_VERIFICATION.json',JSON.stringify({baseline:'440eef7',soldierAssetsAndControllerUnchanged:true,
            masterPreserved:true,width:600,height:300,logicalWidth:300,logicalHeight:150,pixelBlockSize:2,actualPixelloidReduction:true,
            exportPixelloidGameRgbaSha256:sha(img.data),deckLeft:214,deckRight:762,deckY:334,deckSections:1,sharedLayout:true},null,2)+'\n');
        console.log('PASS: real Pixelloid reduction, exact 2x2 blocks, flat deck, matching imports and unchanged character assets/controller.');
        """);
    }
}
