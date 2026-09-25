using Godot;
using System.Collections.Generic;

namespace UnnamedFightingGame;

/// <summary>Damage follows the active fist or foot only during impact poses, once per target per attack.</summary>
public partial class SoldierCombat : Node2D
{
    [Export] public int JabDamage { get; set; } = 5;
    [Export] public int CrossDamage { get; set; } = 7;
    [Export] public int KickDamage { get; set; } = 7;
    private Soldier _soldier = null!;
    private readonly CircleShape2D _fist = new() { Radius=9 };
    private readonly HashSet<ulong> _hitTargets = new();
    private ulong _serial;

    public override void _Ready() { _soldier=GetParent<Soldier>();ProcessPhysicsPriority=1; }

    public override void _PhysicsProcess(double delta)
    {
        if(_serial!=_soldier.AttackSerial) { _serial=_soldier.AttackSerial;_hitTargets.Clear(); }
        if(!_soldier.IsAttacking) return;
        bool cross=_soldier.AttackNumber==2, kick=_soldier.AttackNumber==3;
        int frame=_soldier.Visual.AtlasFrame;
        if(kick ? frame is not (47 or 48 or 49) : cross ? frame is not (32 or 33) : frame!=25) return;
        var socket=kick ? _soldier.Visual.KickSocket : _soldier.Visual.GetNode<Marker2D>(cross?"NearWeaponSocket":"FarWeaponSocket");
        var query=new PhysicsShapeQueryParameters2D {
            Shape=_fist,Transform=new Transform2D(0,socket.GlobalPosition),CollisionMask=8,
            CollideWithAreas=true,CollideWithBodies=false
        };
        foreach(var result in GetWorld2D().DirectSpaceState.IntersectShape(query))
        {
            if(result["collider"].AsGodotObject() is not Area2D area || area.GetParent() is not IDamageReceiver target) continue;
            if(ReferenceEquals(target,_soldier)||!_hitTargets.Add(area.GetParent().GetInstanceId())) continue;
            var hit=(kick?AttackHit.Kick:cross?AttackHit.Cross:AttackHit.Jab) with { Percentage=kick?KickDamage:cross?CrossDamage:JabDamage };
            target.ReceiveHit(hit,_soldier.Facing);
        }
    }
}
