using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace UnnamedFightingGame;

/// <summary>Sixteen independently replaceable part sheets advance on one discrete frame clock.</summary>
public partial class SoldierVisual : Node2D
{
    public static readonly Dictionary<string, (int Start, int Count, double Ticks, bool Loop)> Clips = new()
    {
        ["idle"] = (0, 8, 6, true),
        ["run"] = (8, 8, 8.4, true), // 140 ms per reference walk pose on the 60 Hz runtime clock.
        ["prepare"] = (16, 1, 4, false),
        ["jump"] = (17, 4, 4, false),
        ["fall"] = (21, 2, 6, false),
        ["land"] = (23, 1, 8, false),
        ["jump_sequence"] = (16, 8, 4, false),
        ["attack"] = (24, 5, 2, false),
        ["cross"] = (29, 8, 2, false)
    };
    // Reference frames 0–3: 100 ms; 4–7: 50 ms. One loop is 600 ms.
    private static readonly int[] IdleTicks = { 6, 6, 6, 6, 3, 3, 3, 3 };
    // Runtime punches play at 1.5x reference speed; source artwork/timing stays intact.
    private static readonly int[] AttackTicks = { 2, 4, 2, 2, 2 };
    private static readonly int[] CrossTicks = { 2, 2, 2, 2, 2, 4, 2, 2 };
    public const double CrossSeconds = 18.0 / 60;
    private Texture2D _jabTexture = null!, _crossTexture = null!;
    public const double AttackSeconds = 12.0 / 60;
    public Sprite2D AttackEffect { get; private set; } = null!;
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
        AttackEffect = GetNode<Sprite2D>("AttackEffects/Impact");
        _jabTexture = AttackEffect.Texture;
        _crossTexture = GD.Load<Texture2D>("res://assets/soldier_effects/cross.png");
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
        if (clip is "attack" or "cross")
        {
            double phase = _ticks;
            local = 0;
            var durations = clip == "cross" ? CrossTicks : AttackTicks;
            while (local < durations.Length - 1 && phase + .000001 >= durations[local])
                phase -= durations[local++];
        }
        ApplyFrame(animation.Start + local);
    }

    public void SetPose(string clip, int localFrame)
    {
        var animation = Clips[clip];
        Clip = clip;
        localFrame = Math.Clamp(localFrame, 0, animation.Count - 1);
        _ticks = localFrame * animation.Ticks;
        if (clip is "idle" or "attack" or "cross")
        {
            _ticks = 0;
            var durations = clip == "idle" ? IdleTicks : clip == "cross" ? CrossTicks : AttackTicks;
            for (int i = 0; i < localFrame; i++) _ticks += durations[i];
        }
        ApplyFrame(animation.Start + localFrame);
    }

    private void ApplyFrame(int index)
    {
        AtlasFrame = index;
        foreach (var sprite in _parts) sprite.Frame = index;
        AttackEffect.Visible = Clip is "attack" or "cross";
        if (AttackEffect.Visible)
        {
            var texture = Clip == "cross" ? _crossTexture : _jabTexture;
            if (AttackEffect.Texture != texture)
            {
                AttackEffect.Frame = 0;
                AttackEffect.Texture = texture;
                AttackEffect.Hframes = Clips[Clip].Count;
            }
            AttackEffect.Frame = index - Clips[Clip].Start;
        }
        _nearSocket.Position = _handPositions[index].Near;
        _farSocket.Position = _handPositions[index].Far;
    }
}
