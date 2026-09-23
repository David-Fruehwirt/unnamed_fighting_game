using Godot;
using System;

namespace UnnamedFightingGame;

public partial class Soldier : CharacterBody2D, IDamageReceiver
{
    [Export] public float MoveSpeed { get; set; } = 235f;
    [Export] public float Acceleration { get; set; } = 1700f;
    [Export] public float Braking { get; set; } = 2200f;
    [Export] public float Gravity { get; set; } = 1350f;
    [Export] public float JumpSpeed { get; set; } = 465f;
    [Export] public float CoyoteTime { get; set; } = 0.11f;
    [Export] public float JumpBufferTime { get; set; } = 0.12f;
    [Export] public float GroundAttackSpeedFactor { get; set; } = 0.5f;
    [Export] public float AirAttackSpeedFactor { get; set; } = 1f;
    [Export] public float AttackAcceleration { get; set; } = 600f;
    [Export] public float GroundAttackBraking { get; set; } = 900f;
    [Export] public float AirAttackBraking { get; set; } = 120f;
    public DamageState Damage { get; } = new();
    public event Action? Respawned;
    private Node2D _numbers = null!;
    public SoldierVisual Visual { get; private set; } = null!;
    public Vector2 SpawnPosition { get; private set; }
    public float CoyoteLeft { get; set; }
    public float Facing { get; private set; } = 1;
    public string MotionState { get; private set; } = "idle";
    private float _jumpBuffer;
    private float _prepareLeft, _landingLeft;
    private readonly FistCombo _combo = new();
    public bool IsAttacking => _combo.IsAttacking;
    public bool AttackQueued => _combo.Queued;
    public int AttackNumber => _combo.AttackNumber;
    public ulong AttackSerial { get; private set; }
    private bool _releasedDuringPreparation;

    public override void _Ready()
    {
        Visual = GetNode<SoldierVisual>("Visual");
        _numbers = new Node2D { Name="DamageNumbers" };
        AddChild(_numbers);
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
        if (Input.IsActionJustPressed("reset") || KnockoutBounds.Outside(GlobalPosition))
        {
            Reset();
            return;
        }
        if(Damage.Stunned)
        {
            Damage.Tick(dt);
            Velocity=new Vector2(Velocity.X,Math.Min(Velocity.Y+Gravity*dt,800));
            MoveAndSlide();
            MotionState="hitstun";
            Visual.Position=GlobalPosition.Round()-GlobalPosition;
            Visual.Advance("idle",dt,1);
            return;
        }
        float axis = Input.GetAxis("move_left", "move_right");
        Vector2 velocity = Velocity;
        bool wasOnFloor = IsOnFloor();
        bool attackStarted = _combo.Advance(dt, Input.IsActionJustPressed("attack"));
        if (attackStarted) AttackSerial++;
        if (attackStarted && axis != 0) Facing = Math.Sign(axis);
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
        // Attack playback is independent of locomotion: keep momentum and vertical physics.
        bool groundedAttack = wasOnFloor && !takeoff;
        if(attackStarted&&!groundedAttack)
            velocity.X=Mathf.Clamp(velocity.X+Facing*(AttackNumber==2?100:70),-360,360);
        float targetSpeed = MoveSpeed * (IsAttacking ?
            (groundedAttack ? GroundAttackSpeedFactor : AirAttackSpeedFactor) : 1);
        if(IsAttacking&&!groundedAttack&&axis*velocity.X>0)
            targetSpeed=Math.Max(targetSpeed,Math.Abs(velocity.X));
        float steering = IsAttacking ? AttackAcceleration : Acceleration;
        float braking = IsAttacking ? (groundedAttack ? GroundAttackBraking : AirAttackBraking) : Braking;
        velocity.X = Mathf.MoveToward(velocity.X, axis * targetSpeed,
            (axis != 0 ? steering : braking) * dt);
        if (!IsAttacking && axis != 0) Facing = Math.Sign(axis);
        Velocity = velocity;
        MoveAndSlide();
        if (!wasOnFloor && IsOnFloor() && velocity.Y > 50) _landingLeft = 8f / 60;
        string movementState = _prepareLeft > 0 ? "prepare"
            : !IsOnFloor() ? (Velocity.Y < -20 ? "jump" : "fall")
            : _landingLeft > 0 ? "land"
            : Math.Abs(Velocity.X) > 15 ? "run" : "idle";
        MotionState = IsAttacking ? (AttackNumber == 2 ? "cross" : "attack") : movementState;
        Visual.Scale = new Vector2(Facing, 1);
        // Snap only the display: collision movement retains its full precision.
        Visual.Position = GlobalPosition.Round() - GlobalPosition;
        if (attackStarted) Visual.SetPose(MotionState, 0);
        else Visual.Advance(MotionState, dt,
            MotionState == "run" ? Mathf.Clamp(Math.Abs(Velocity.X) / MoveSpeed, 0.5f, 1.15f) : 1);
    }

    public int ReceiveHit(AttackHit hit,float facing)
    {
        if(hit.Percentage<=0)return 0;
        Velocity=Damage.Apply(hit,facing);
        _combo.Reset();AttackSerial++;
        _prepareLeft=_landingLeft=_jumpBuffer=CoyoteLeft=0;
        MotionState="hitstun";
        Visual.SetPose("idle",0);
        DamageFeedback.Show(_numbers,Damage.LastDamage,Damage.HitCount);
        return Damage.LastDamage;
    }

    public void SetSpawn(Vector2 position)
    {
        SpawnPosition = position;
        Reset();
    }

    public void Reset()
    {
        Damage.Reset();
        foreach(Node number in _numbers.GetChildren())number.QueueFree();
        GlobalPosition = SpawnPosition;
        Velocity = Vector2.Zero;
        CoyoteLeft = 0;
        _jumpBuffer = 0;
        _prepareLeft = _landingLeft = 0;
        _combo.Reset();
        AttackSerial++;
        _releasedDuringPreparation = false;
        Facing = 1;
        MotionState = "idle";
        Visual.Position = Vector2.Zero;
        Visual.Scale = Vector2.One;
        Visual.SetPose("idle", 0);
        Respawned?.Invoke();
    }
}
