using System.Text.Json;
using System.Text.Json.Nodes;
namespace PixelAuthor;
public static partial class Program
{
    static void BuildThrusters()
    {
        foreach(string kind in new[]{"boot","pack"})
        {
            string file=$"art/{kind}-thrusters.pixel.json";
            var project=Project(kind+"_thrusters",16,16,["flame"]);
            project["characters"]![0]!["origin"]=JsonSerializer.SerializeToNode(new{x=8,y=0});
            Save(file,project);
            Console.WriteLine(RunPix("inspect",file,"--json"));
            var ops=new List<object>();
            for(int i=0;i<4;i++)
            {
                var c=new Canvas(16,16);int length=(kind=="boot"?7:4)+new[]{0,2,1,3}[i];
                for(int y=0;y<length;y++)
                {
                    int half=kind=="boot"?(y<length-3?2:1):(y<length-2?1:0);
                    if(y==length-1)half=0;
                    for(int x=-half;x<=half;x++)c.Set(8+x,y,Math.Abs(x)==half?"blue_light":y<3?"cyan_light":"cyan");
                }
                c.Set(8,0,"light");
                ops.Add(new{type="addFrame",characterId=kind+"_thrusters",viewId="right",frame=new{id=$"flame_{i}",name=$"Flame {i}",durationTicks=3,
                    cels=new{flame=new{grid=new{width=16,height=16,cells=c.Cells},offset=new{x=0,y=0}}}}});
            }
            ops.Add(new{type="addAnimation",characterId=kind+"_thrusters",animation=new{id="thrust",name="Thrust",viewId="right",loop=true,tags=new[]{"pixel-grid"},
                frames=Enumerable.Range(0,4).Select(i=>new{frameId=$"flame_{i}",durationTicks=3}).ToArray()}});
            Save($"art/work/{kind}-thrusters.operations.json",ops);
            Console.WriteLine(RunPix("apply",file,"--operations",$"art/work/{kind}-thrusters.operations.json","--expected-hash",ProjectHash(file)));
            Console.WriteLine(RunPix("validate",file,"--json"));
            RunPix("sheet",file,"--animation","thrust","--layout","horizontal","--out",$"art/exports/effects/{kind}-thrusters.png");
            RunPix("gif",file,"--animation","thrust","--scale","8","--out",$"art/previews/{kind}-thrusters.gif");
        }
        var source=JsonNode.Parse(File.ReadAllText("art/soldier.pixel.json"))!["characters"]![0]!["views"]![0]!["frames"]!.AsArray();
        var poses=JsonNode.Parse(File.ReadAllText("art/poses.json"))!["frames"]!.AsArray();
        var sockets=new List<object>();
        foreach(var pose in poses)
        {
            var frame=source.First(f=>f!["id"]!.GetValue<string>()==pose!["id"]!.GetValue<string>())!;
            P Bottom(string part,double fraction)
            {
                var cells=frame["cels"]![part]!["grid"]!["cells"]!.AsArray();
                var points=Enumerable.Range(0,cells.Count).Where(i=>cells[i]!=null).Select(i=>new P(i%128,i/128)).ToArray();
                int min=points.Min(p=>p.X),max=points.Max(p=>p.X);
                int x=(int)Math.Round(min+(max-min)*fraction);
                return new(x,points.Where(p=>p.X==x).Max(p=>p.Y)+1);
            }
            sockets.Add(new{id=pose!["id"]!.GetValue<string>(),points=new[]{Bottom("far_foot",.5),Bottom("near_foot",.5),Bottom("backpack",.25),Bottom("backpack",.75)}});
        }
        Save("art/thruster-sockets.json",sockets);
    }
}
