using Godot;
using System;
using System.Linq;

namespace UnnamedFightingGame;

public partial class Arena : Node2D
{
    private Soldier _soldier = null!;
    private Label _stateLabel = null!;

    public override void _Ready()
    {
        _soldier = GetNode<Soldier>("Soldier");
        _stateLabel = GetNode<Label>("HUD/State");
        string[] args = OS.GetCmdlineUserArgs();
        if (args.Contains("--capture")) Capture(args);
    }

    public override void _Process(double delta) => _stateLabel.Text = _soldier.MotionState.ToUpperInvariant();

    private async void Capture(string[] args)
    {
        try
        {
            await ToSignal(GetTree().CreateTimer(0.4), SceneTreeTimer.SignalName.Timeout);
            string path = Argument(args, "--capture", "user://arena.png");
            string pose = Argument(args, "--pose", args.Contains("--run-pose") ? "run" : "idle");
            int frame = int.Parse(Argument(args, "--frame", "0"));
            _soldier.SetPhysicsProcess(false);
            _soldier.Visual.SetPose(pose, frame);
            _stateLabel.Text = pose.ToUpperInvariant();
            SetProcess(false);
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            Error error = GetViewport().GetTexture().GetImage().SavePng(path);
            GD.Print($"Screenshot: {path}; result={error}");
            GetTree().Quit(error == Error.Ok ? 0 : 1);
        }
        catch (Exception ex) { GD.PushError(ex.ToString()); GetTree().Quit(1); }
    }

    private static string Argument(string[] args, string name, string fallback)
    {
        int index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : fallback;
    }
}
