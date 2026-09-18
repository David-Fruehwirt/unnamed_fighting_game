using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace UnnamedFightingGame;

/// <summary>Sixteen independently replaceable part sheets advance on one discrete frame clock.</summary>
public partial class SoldierVisual : Node2D
{
    public static readonly Dictionary<string, (int Start, int Count, int Ticks, bool Loop)> Clips = new()
    {
        ["idle"] = (0, 8, 6, true),
        ["run"] = (8, 8, 5, true),
        ["jump"] = (16, 3, 6, false),
        ["fall"] = (19, 3, 6, false)
    };
    // Reference frames 0–3: 100 ms; 4–7: 50 ms. One loop is 600 ms.
    private static readonly int[] IdleTicks = { 6, 6, 6, 6, 3, 3, 3, 3 };
    public int AtlasFrame { get; private set; }
    public int PartCount => _parts.Count;
    public string Clip { get; private set; } = "idle";
    private readonly List<Sprite2D> _parts = new();
    private readonly List<(Vector2 Near, Vector2 Far)> _handPositions = new();
    private Marker2D _nearSocket = null!;
    private Marker2D _farSocket = null!;
    private double _ticks;

    public override void _Ready()
    {
        foreach (Node node in GetChildren())
            if (node is Sprite2D sprite) _parts.Add(sprite);
        _nearSocket = GetNode<Marker2D>("NearWeaponSocket");
        _farSocket = GetNode<Marker2D>("FarWeaponSocket");
        using var poses = JsonDocument.Parse(FileAccess.GetFileAsString("res://assets/soldier_frames/poses.json"));
        foreach (var frame in poses.RootElement.GetProperty("frames").EnumerateArray())
        {
            var parts = frame.GetProperty("parts");
            _handPositions.Add((ReadPoint(parts,"near_hand"),ReadPoint(parts,"far_hand")));
        }
        SetPose("idle", 0);
    }

    private static Vector2 ReadPoint(JsonElement parts, string id)
    {
        var point = parts.GetProperty(id).GetProperty("Start");
        return new Vector2(point.GetProperty("X").GetInt32() - 64,
            point.GetProperty("Y").GetInt32() - 121);
    }

    public void Advance(string clip, double delta, float speed)
    {
        if (Clip != clip) { Clip = clip; _ticks = 0; }
        else _ticks += delta * 60 * speed;
        var animation = Clips[clip];
        int local = (int)(_ticks / animation.Ticks);
        local = animation.Loop ? local % animation.Count : Math.Min(local, animation.Count - 1);
        if (clip == "idle")
        {
            double phase = _ticks % 36;
            local = 0;
            while (local < IdleTicks.Length - 1 && phase >= IdleTicks[local])
                phase -= IdleTicks[local++];
        }
        ApplyFrame(animation.Start + local);
    }

    public void SetPose(string clip, int localFrame)
    {
        var animation = Clips[clip];
        Clip = clip;
        localFrame = Math.Clamp(localFrame, 0, animation.Count - 1);
        _ticks = localFrame * animation.Ticks;
        if (clip == "idle")
        {
            _ticks = 0;
            for (int i = 0; i < localFrame; i++) _ticks += IdleTicks[i];
        }
        ApplyFrame(animation.Start + localFrame);
    }

    private void ApplyFrame(int index)
    {
        AtlasFrame = index;
        foreach (var sprite in _parts) sprite.Frame = index;
        _nearSocket.Position = _handPositions[index].Near;
        _farSocket.Position = _handPositions[index].Far;
    }
}
