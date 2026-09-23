using Godot;
using System;

namespace UnnamedFightingGame;

public partial class TrainingDummy : CharacterBody2D
{
    [Export] public int MaxHealth { get; set; } = 100;
    public int Health { get; private set; }
    public int LastDamage { get; private set; }
    public int HitCount { get; private set; }
    public bool KnockbackEnabled { get; set; }
    public bool AutoResetPosition { get; set; } = true;
    public Vector2 HomePosition { get; private set; }
    public const double ReturnDelay = 2;
    public bool Recovering => _refillLeft > 0;
    private double _sinceHit, _refillLeft;
    private bool _returnPending;
    private Label _healthText = null!, _lastHit = null!;
    private Node2D _numbers = null!;
    private Sprite2D _art = null!;

    public override void _Ready()
    {
        HomePosition = GlobalPosition;
        _art = GetNode<Sprite2D>("Artwork");
        _healthText = MakeLabel(new(-60,-145),new(120,18),12);
        _lastHit = MakeLabel(new(-70,-163),new(140,18),11);
        _numbers = new Node2D { Name = "DamageNumbers" };
        AddChild(_numbers);
        ResetDummy();
    }

    private Label MakeLabel(Vector2 position,Vector2 size,int fontSize)
    {
        var label = new Label { Position=position,Size=size,HorizontalAlignment=HorizontalAlignment.Center,
            MouseFilter=Control.MouseFilterEnum.Ignore };
        label.AddThemeFontSizeOverride("font_size",fontSize);
        label.AddThemeColorOverride("font_shadow_color",Colors.Black);
        label.AddThemeConstantOverride("shadow_offset_x",1);
        label.AddThemeConstantOverride("shadow_offset_y",1);
        AddChild(label);
        return label;
    }

    public void SetHome(Vector2 position) { HomePosition=position;ResetDummy(); }

    public int TakeHit(int damage,string attack,Vector2 impulse)
    {
        if (damage <= 0 || Health == 0) return 0;
        LastDamage = Math.Min(damage,Health);
        Health -= LastDamage;
        HitCount++;
        _sinceHit=0;_returnPending=true;
        if(KnockbackEnabled) Velocity=impulse;
        _lastHit.Text=$"{attack.ToUpperInvariant()}  {LastDamage} DMG";
        var number = new Label { Text=$"−{LastDamage}",Position=new Vector2(HitCount%2==0?-45:20,-95),
            MouseFilter=Control.MouseFilterEnum.Ignore };
        number.AddThemeFontSizeOverride("font_size",18);
        number.AddThemeColorOverride("font_color",new Color("ffd787"));
        number.AddThemeColorOverride("font_outline_color",new Color("211b20"));
        number.AddThemeConstantOverride("outline_size",4);
        _numbers.AddChild(number);
        var tween=number.CreateTween().SetParallel();
        tween.TweenProperty(number,"position:y",number.Position.Y-38,.8);
        tween.TweenProperty(number,"modulate:a",0f,.8).SetDelay(.15);
        tween.Chain().TweenCallback(Callable.From(number.QueueFree));
        if(Health==0) _refillLeft=1;
        UpdateHealth();
        return LastDamage;
    }

    public override void _PhysicsProcess(double delta)
    {
        _sinceHit+=delta;
        if(_refillLeft>0)
        {
            _refillLeft=Math.Max(0,_refillLeft-delta);
            if(_refillLeft==0) { Health=MaxHealth;UpdateHealth(); }
        }
        if(AutoResetPosition&&_returnPending&&_sinceHit>=ReturnDelay) ResetPosition();
        if(GlobalPosition.Y>700) { ResetDummy();return; }
        var velocity=Velocity;
        velocity.X=KnockbackEnabled?Mathf.MoveToward(velocity.X,0,600*(float)delta):0;
        velocity.Y=Math.Min(velocity.Y+1350*(float)delta,800);
        Velocity=velocity;
        MoveAndSlide();
        _art.Position=GlobalPosition.Round()-GlobalPosition;
    }

    public void ResetPosition()
    {
        GlobalPosition=HomePosition;Velocity=Vector2.Zero;_returnPending=false;
    }

    public void ResetDummy()
    {
        ResetPosition();Health=MaxHealth;LastDamage=HitCount=0;_refillLeft=0;_sinceHit=0;
        _lastHit.Text="WOODEN DUMMY";
        foreach(Node number in _numbers.GetChildren()) number.QueueFree();
        UpdateHealth();
    }

    private void UpdateHealth() { _healthText.Text=$"{Health} / {MaxHealth} HP";QueueRedraw(); }

    public override void _Draw()
    {
        DrawRect(new Rect2(-38,-127,76,10),new Color("211b20"));
        DrawRect(new Rect2(-36,-125,72,6),new Color("49302a"));
        DrawRect(new Rect2(-36,-125,72*Health/(float)Math.Max(1,MaxHealth),6),
            new Color(Health>MaxHealth/3?"89cf91":"e17460"));
    }
}
