using System.Text.Json;

namespace PixelAuthor;

public static partial class Program
{
    // Prepare semantic operations; apply through Code as Pixel Art MCP (or CLI).
    static void PrepareDummy(string part)
    {
        string[] layers = ["stand", "body", "head"];
        if (!layers.Contains(part)) throw new Exception("Dummy part must be stand, body or head");
        const string file = "art/dummy.pixel.json";
        if (!File.Exists(file))
        {
            var project = Project("dummy",80,120,layers);
            project["palette"] = JsonSerializer.SerializeToNode(new[] {
                new{id="outline",name="Charred wood outline",color="#211b20"},
                new{id="deep",name="Wood crevice",color="#49302a"},
                new{id="shadow",name="Walnut shadow",color="#694334"},
                new{id="wood",name="Warm oak",color="#98623e"},
                new{id="mid",name="Honey oak",color="#be8953"},
                new{id="light",name="Cut wood edge",color="#e3b97a"},
                new{id="rope",name="Hemp binding",color="#f0d7a0"},
                new{id="iron",name="Iron strap",color="#3d454c"},
                new{id="steel",name="Iron edge",color="#7b8585"},
                new{id="red",name="Painted target",color="#b84e40"},
                new{id="red_dark",name="Target paint shadow",color="#73352f"}
            });
            Save(file,project);
        }
        var c = new Canvas(80,120);
        void Poly(string color, params P[] points) => c.Polygon(points,color);
        void Rect(string color,int x,int y,int w,int h) => Poly(color,new(x,y),new(x+w-1,y),new(x+w-1,y+h-1),new(x,y+h-1));
        void Line(string color,int x,int y,int x2,int y2) => c.Line(new(x,y),new(x2,y2),color);
        if (part == "stand")
        {
            Poly("outline",new(34,64),new(46,64),new(46,108),new(66,110),new(67,116),new(13,116),new(14,110),new(34,108));
            Rect("shadow",36,66,8,44);Rect("wood",37,66,4,44);Rect("light",37,67,1,41);
            Line("deep",42,81,41,98);Line("mid",39,96,39,107);
            Poly("wood",new(16,111),new(34,109),new(45,109),new(64,111),new(64,114),new(16,114));
            Line("light",18,111,61,111);Line("deep",18,114,62,114);
            Rect("iron",32,106,16,4);Rect("steel",33,106,14,1);
            c.Set(34,108,"steel");c.Set(45,108,"steel");
        }
        else if (part == "body")
        {
            Rect("outline",8,35,64,12);Rect("shadow",10,37,60,8);Rect("wood",11,37,58,5);
            Line("light",12,37,68,37);Line("deep",13,43,66,43);
            Line("shadow",14,40,27,40);Line("mid",54,39,65,39);
            Poly("outline",new(29,32),new(49,32),new(55,40),new(54,69),new(49,80),new(30,80),new(24,69),new(24,41));
            Poly("shadow",new(30,34),new(48,34),new(52,41),new(51,69),new(47,77),new(31,77),new(27,68),new(27,42));
            Poly("wood",new(30,36),new(45,35),new(47,41),new(47,72),new(44,77),new(31,76),new(29,68),new(29,41));
            Rect("mid",31,38,6,34);Line("light",31,39,31,65);
            Line("deep",38,36,38,76);Line("shadow",45,37,45,75);
            Line("shadow",34,42,35,48);Line("wood",33,60,34,70);
            Rect("deep",25,40,29,5);Rect("rope",26,40,27,2);Rect("mid",26,43,27,1);
            Rect("deep",27,70,26,5);Rect("rope",28,70,24,2);Rect("mid",28,73,23,1);
            // Octagonal target paint follows the integer pixel grid.
            Poly("red_dark",new(36,47),new(43,47),new(48,52),new(48,59),new(43,64),new(36,64),new(31,59),new(31,52));
            Poly("red",new(36,48),new(43,48),new(47,52),new(47,59),new(43,63),new(36,63),new(32,59),new(32,52));
            Poly("light",new(37,51),new(42,51),new(44,53),new(44,58),new(42,60),new(37,60),new(35,58),new(35,53));
            Rect("red",38,54,4,4);c.Set(38,54,"red_dark");
            Rect("iron",11,36,3,9);c.Set(12,38,"steel");c.Set(12,42,"steel");
            Rect("iron",66,36,3,9);c.Set(67,38,"steel");c.Set(67,42,"steel");
        }
        else
        {
            Rect("outline",35,26,10,11);Rect("shadow",37,27,6,8);Rect("wood",37,27,3,8);
            Poly("outline",new(32,8),new(47,8),new(52,13),new(52,25),new(47,31),new(32,31),new(27,25),new(27,13));
            Poly("shadow",new(33,10),new(46,10),new(50,14),new(50,24),new(46,29),new(33,29),new(29,24),new(29,14));
            Poly("wood",new(33,10),new(43,10),new(47,14),new(47,24),new(43,29),new(33,28),new(30,23),new(30,14));
            Line("light",33,11,43,11);Line("mid",31,14,31,23);
            Line("mid",35,13,35,17);Line("shadow",43,13,42,20);Line("deep",43,24,44,27);
            // Carved wooden face: eyes and a small centered notch.
            Rect("deep",33,18,3,2);Rect("deep",43,18,3,2);
            Line("light",33,20,35,20);Line("wood",43,20,45,20);
            Line("deep",37,25,41,25);c.Set(39,23,"shadow");
        }
        var ops = new List<object>();
        for(int y=0;y<120;y++)for(int x=0;x<80;x++)if(c.Cells[y*80+x] is string token)
            ops.Add(new{type="setPixel",characterId="dummy",viewId="right",frameId="neutral",layerId=part,x,y,tokenId=token});
        Save($"art/work/dummy-{part}.operations.json",ops);
        Console.WriteLine($"Prepared {part}: {ops.Count} semantic pixels; inspect and apply with expected hash.");
    }
}
