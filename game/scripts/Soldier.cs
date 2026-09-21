using Godot;
using System;

namespace UnnamedFightingGame;

public partial class Soldier : CharacterBody2D
{
    [Export] public float MoveSpeed { get; set; } = 235f;
    [Export] public float Acceleration { get; set; } = 1700f;
    [Export] public float Braking { get; set; } = 2200f;
    [Export] public float Gravity { get; set; } = 1350f;
    [Export] public float JumpSpeed { get; set; } = 465f;
    [Export] public float CoyoteTime { get; set; } = 0.11f;
    [Export] public float JumpBufferTime { get; set; } = 0.12f;
    public SoldierVisual Visual { get; private set; } = null!;
    public Vector2 SpawnPosition { get; private set; }
    public float CoyoteLeft { get; set; }
    public float Facing { get; private set; } = 1;
    public string MotionState { get; private set; } = "idle";
    private float _jumpBuffer;
    private float _prepareLeft, _landingLeft;
    private float _attackLeft;
    private bool _releasedDuringPreparation;

    public override void _Ready()
    {
        Visual = GetNode<SoldierVisual>("Visual");
        SpawnPosition = GlobalPosition;
        Bind("move_left", Key.A, Key.Left);
        Bind("move_right", Key.D, Key.Right);
        Bind("jump", Key.Space, Key.W, Key.Up);
        Bind("reset", Key.R);
        Bind("attack", Key.J);
    }

    private static void Bind(string action, params Key[] keys)
    {
        if (InputMap.HasAction(action)) return;
        InputMap.AddAction(action);
        foreach (var key in keys)
            InputMap.ActionAddEvent(action, new InputEventKey { PhysicalKeycode = key });
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        if (Input.IsActionJustPressed("reset") || GlobalPosition.Y > 700)
        {
            Reset();
            return;
        }
        float axis = Input.GetAxis("move_left", "move_right");
        Vector2 velocity = Velocity;
        bool wasOnFloor = IsOnFloor();
        bool attackStarted = false;
        _attackLeft = Math.Max(0, _attackLeft - dt);
        if (_attackLeft < .00001f) _attackLeft = 0;
        if (Input.IsActionJustPressed("attack") && _attackLeft == 0 && wasOnFloor &&
            _prepareLeft == 0 && !Input.IsActionJustPressed("jump"))
        {
            _attackLeft = (float)SoldierVisual.AttackSeconds;
            attackStarted = true;
        }
        _landingLeft = Math.Max(0, _landingLeft - dt);
        bool takeoff = false;
        if (_prepareLeft > 0)
        {
            _releasedDuringPreparation |= Input.IsActionJustReleased("jump");
            _prepareLeft = Math.Max(0, _prepareLeft - dt);
            if (_prepareLeft < .00001f) _prepareLeft = 0;
            takeoff = _prepareLeft == 0;
        }
        if (IsOnFloor()) CoyoteLeft = CoyoteTime;
        else
        {
            CoyoteLeft = Math.Max(0, CoyoteLeft - dt);
            velocity.Y = Math.Min(velocity.Y + Gravity * dt, 800);
        }
        _jumpBuffer = Math.Max(0, _jumpBuffer - dt);
        if (Input.IsActionJustPressed("jump")) _jumpBuffer = JumpBufferTime;
        if (_jumpBuffer > 0 && CoyoteLeft > 0 && _prepareLeft == 0 && !takeoff)
        {
            _attackLeft = 0;
            _jumpBuffer = 0;
            _landingLeft = 0;
            _releasedDuringPreparation = !Input.IsActionPressed("jump");
            // One grounded anticipation pose. Coyote jumps remain immediate.
            if (wasOnFloor) _prepareLeft = 4f / 60;
            else takeoff = true;
        }
        if (takeoff)
        {
            velocity.Y = _releasedDuringPreparation ? -170 : -JumpSpeed;
            CoyoteLeft = 0;
        }
        if (Input.IsActionJustReleased("jump") && velocity.Y < -170) velocity.Y = -170;
        if (!wasOnFloor) _attackLeft = 0;
        // Grounded jab plants the feet and holds facing. Jumping cancels it immediately.
        if (_attackLeft > 0) { axis = 0; velocity.X = 0; }
        velocity.X = Mathf.MoveToward(velocity.X, axis * MoveSpeed,
            (axis != 0 ? Acceleration : Braking) * dt);
        if (axis != 0) Facing = Math.Sign(axis);
        Velocity = velocity;
        MoveAndSlide();
        if (!wasOnFloor && IsOnFloor() && velocity.Y > 50) _landingLeft = 8f / 60;
        Position = new Vector2(Mathf.Clamp(Position.X, 22, 938), Position.Y);
        MotionState = _prepareLeft > 0 ? "prepare"
            : !IsOnFloor() ? (Velocity.Y < -20 ? "jump" : "fall")
            : _attackLeft > 0 ? "attack"
            : _landingLeft > 0 ? "land"
            : Math.Abs(Velocity.X) > 15 ? "run" : "idle";
        Visual.Scale = new Vector2(Facing, 1);
        // Snap only the display: collision movement retains its full precision.
        Visual.Position = GlobalPosition.Round() - GlobalPosition;
        if (MotionState == "attack" && attackStarted) Visual.SetPose("attack", 0);
        else Visual.Advance(MotionState, dt,
            MotionState == "run" ? Mathf.Clamp(Math.Abs(Velocity.X) / MoveSpeed, 0.5f, 1.15f) : 1);
    }

    public void SetSpawn(Vector2 position)
    {
        SpawnPosition = position;
        Reset();
    }

    public void Reset()
    {
        GlobalPosition = SpawnPosition;
        Velocity = Vector2.Zero;
        CoyoteLeft = 0;
        _jumpBuffer = 0;
        _prepareLeft = _landingLeft = 0;
        _attackLeft = 0;
        _releasedDuringPreparation = false;
        Facing = 1;
        MotionState = "idle";
        Visual.Position = Vector2.Zero;
        Visual.Scale = Vector2.One;
        Visual.SetPose("idle", 0);
    }
}
