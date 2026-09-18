using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PixelAuthor;

public static partial class Program
{
    static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    static readonly string Root = Directory.GetCurrentDirectory();
    static readonly string Pix = Environment.GetEnvironmentVariable("PIX_CLI") ??
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".codex", "tools", "code-as-pixel-art", "packages", "cli", "dist", "bin.js");
    static JsonArray Palette => JsonNode.Parse(File.ReadAllText("art/palette.json"))!.AsArray();

    public static int Main(string[] args)
    {
        try
        {
            if (args.Length == 2 && args[0] == "part") BuildPart(args[1]);
            else if (args.Length == 1 && args[0] == "animate") BuildAnimations();
            else if (args.Length == 1 && args[0] == "idle") ReworkIdle();
            else if (args.Length == 1 && args[0] == "walk") ReworkWalk();
            else if (args.Length == 1 && args[0] == "jump") ReworkJump();
            else if (args.Length == 1 && args[0] == "verify-jump") VerifyJump();
            else if (args.Length == 1 && args[0] == "verify-walk") VerifyWalk();
            else if (args.Length == 1 && args[0] == "verify-idle") VerifyIdle();
            else if (args.Length == 1 && args[0] == "pixelloid") ProcessPixelloid();
            else throw new Exception("Usage: PixelAuthor part <name> | animate | idle | walk | jump | verify-idle | verify-walk | verify-jump | pixelloid");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }

    static string RunPix(params string[] args)
    {
        var info = new ProcessStartInfo("node") { RedirectStandardOutput = true, RedirectStandardError = true };
        info.ArgumentList.Add(Pix);
        foreach (var arg in args) info.ArgumentList.Add(arg);
        using var process = Process.Start(info) ?? throw new Exception("Cannot start Code as Pixel Art");
        string output = process.StandardOutput.ReadToEnd();
        string errors = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0) throw new Exception(output + errors);
        return output;
    }

    static void Save(string path, object value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(value, JsonOptions) + "\n");
    }

    static JsonObject Project(string name, int width, int height, string[] ids)
    {
        var point = new { x = width / 2, y = height - 4 };
        return JsonSerializer.SerializeToNode(new {
            schemaVersion = 1, id = name, name, ticksPerSecond = 60, palette = Palette,
            characters = new[] { new {
                id = name, name, width, height, origin = point, pivot = point,
                bounds = new { x = 0, y = 0, width, height },
                anchors = new { feet = point },
                parts = ids.Select(id => new { id, name = id, pivot = point }).ToArray(),
                layers = ids.Select((id, index) => new {
                    id, name = id, partId = id, zIndex = index, visible = true, locked = false, linked = false
                }).ToArray(),
                views = new[] { new {
                    id = "right", name = "Right",
                    frames = new[] { new {
                        id = "neutral", name = "Neutral", durationTicks = 8,
                        cels = ids.ToDictionary(id => id, id => new {
                            grid = new { width, height, cells = new string?[width * height] },
                            offset = new { x = 0, y = 0 }
                        })
                    } }
                } },
                poses = Array.Empty<object>(), variants = Array.Empty<object>(), animations = Array.Empty<object>(),
                metadata = new { authoring = "C# integer-grid polygons and pixel strokes; no image generation" }
            } },
            metadata = new { renderer = "Code as Pixel Art", pixelPitch = "1" }
        })!.AsObject();
    }

    static void BuildPart(string name)
    {
        string target = $"art/parts/{name}.pixel.json";
        if (File.Exists(target))
        {
            var existing = JsonNode.Parse(File.ReadAllText(target))!;
            var existingCells = existing["characters"]![0]!["views"]![0]!["frames"]![0]!["cels"]![name]!["grid"]!["cells"]!.AsArray();
            if (existingCells.Any(cell => cell is not null))
                throw new Exception($"Refusing to overwrite editable source: {target}");
        }
        var definition = JsonSerializer.Deserialize<PartDefinition>(File.ReadAllText($"art/draw/{name}.json"))!;
        var canvas = new Canvas(40, 40);
        Draw(canvas, definition, p => p);
        Save(target, Project(name, 40, 40, [name]));
        string inspection = RunPix("inspect", target, "--json");
        if (JsonNode.Parse(inspection)!["valid"]!.GetValue<bool>() != true)
            throw new Exception("Source inspection failed");
        var compact = JsonNode.Parse(File.ReadAllText(target))!.ToJsonString(new JsonSerializerOptions {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(compact))).ToLowerInvariant();
        var operations = new List<object>();
        for (int y = 0; y < 40; y++)
        for (int x = 0; x < 40; x++)
            if (canvas.Cells[y * 40 + x] is string token)
                operations.Add(new { type = "setPixel", characterId = name, viewId = "right",
                    frameId = "neutral", layerId = name, x, y, tokenId = token });
        string opPath = $"art/work/{name}.operations.json";
        Save(opPath, operations);
        RunPix("apply", target, "--operations", opPath, "--expected-hash", hash);
        Console.WriteLine(RunPix("validate", target, "--json"));
        Directory.CreateDirectory("art/exports/parts");
        Directory.CreateDirectory("art/previews/parts");
        RunPix("render", target, "--out", $"art/exports/parts/{name}.png");
        RunPix("render", target, "--scale", "8", "--out", $"art/previews/parts/{name}.png");
        Console.WriteLine($"Authored and validated {name}: {operations.Count} pixels");
    }

    static void Draw(Canvas canvas, PartDefinition definition, Func<P, P> transform)
    {
        foreach (var shape in definition.Shapes)
        {
            var points = shape.Points.Select(p => transform(new P(p[0], p[1]))).ToArray();
            if (points.Length == 1) canvas.Set(points[0].X, points[0].Y, shape.Color);
            else if (points.Length == 2) canvas.Line(points[0], points[1], shape.Color);
            else canvas.Polygon(points, shape.Color);
        }
    }

    public record P(int X, int Y);
    public sealed class PartDefinition
    {
        public string Name { get; set; } = "";
        public int[] Anchor { get; set; } = [20, 10];
        public int[] End { get; set; } = [20, 30];
        public Shape[] Shapes { get; set; } = [];
    }
    public sealed class Shape
    {
        public string Color { get; set; } = "outline";
        public int[][] Points { get; set; } = [];
    }
    public sealed class Canvas(int width, int height)
    {
        public int Width => width;
        public int Height => height;
        public string?[] Cells { get; } = new string?[width * height];
        public void Set(int x, int y, string color)
        {
            if (x >= 0 && y >= 0 && x < width && y < height) Cells[y * width + x] = color;
        }
        public void Line(P a, P b, string color)
        {
            int x = a.X, y = a.Y, dx = Math.Abs(b.X - x), dy = -Math.Abs(b.Y - y);
            int sx = x < b.X ? 1 : -1, sy = y < b.Y ? 1 : -1, error = dx + dy;
            while (true)
            {
                Set(x, y, color);
                if (x == b.X && y == b.Y) break;
                int twice = error * 2;
                if (twice >= dy) { error += dy; x += sx; }
                if (twice <= dx) { error += dx; y += sy; }
            }
        }
        public void Polygon(P[] points, string color)
        {
            int minY = points.Min(p => p.Y), maxY = points.Max(p => p.Y);
            for (int y = minY; y <= maxY; y++)
            {
                var crossings = new List<double>();
                for (int i = 0; i < points.Length; i++)
                {
                    var a = points[i]; var b = points[(i + 1) % points.Length];
                    if ((a.Y <= y && b.Y > y) || (b.Y <= y && a.Y > y))
                        crossings.Add(a.X + (double)(y - a.Y) * (b.X - a.X) / (b.Y - a.Y));
                }
                crossings.Sort();
                for (int i = 0; i + 1 < crossings.Count; i += 2)
                    for (int x = (int)Math.Ceiling(crossings[i]); x <= (int)Math.Floor(crossings[i + 1]); x++)
                        Set(x, y, color);
            }
            for (int i = 0; i < points.Length; i++) Line(points[i], points[(i + 1) % points.Length], color);
        }
    }
}
