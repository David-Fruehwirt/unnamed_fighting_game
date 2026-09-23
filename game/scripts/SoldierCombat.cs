using Godot;
using System.Collections.Generic;

namespace UnnamedFightingGame;

/// <summary>Damage follows the visible fist only during impact poses, once per target per punch.</summary>
public partial class SoldierCombat : Node2D
{
    [Export] public int JabDamage { get; set; } = 5;
    [Export] public int CrossDamage { get; set; } = 7;
    private Soldier _soldier = null!;
    private readonly CircleShape2D _fist = new() { Radius=9 };
    private readonly HashSet<ulong> _hitTargets = new();
    private ulong _serial;

    public override void _Ready() { _soldier=GetParent<Soldier>();ProcessPhysicsPriority=1; }

    public override void _PhysicsProcess(double delta)
    {
        if(_serial!=_soldier.AttackSerial) { _serial=_soldier.AttackSerial;_hitTargets.Clear(); }
        if(!_soldier.IsAttacking) return;
        bool cross=_soldier.AttackNumber==2;
        int frame=_soldier.Visual.AtlasFrame;
        if(cross ? frame is not (32 or 33) : frame!=25) return;
        var socket=_soldier.Visual.GetNode<Marker2D>(cross?"NearWeaponSocket":"FarWeaponSocket");
        var query=new PhysicsShapeQueryParameters2D {
            Shape=_fist,Transform=new Transform2D(0,socket.GlobalPosition),CollisionMask=8,
            CollideWithAreas=true,CollideWithBodies=false
        };
        foreach(var result in GetWorld2D().DirectSpaceState.IntersectShape(query))
        {
            if(result["collider"].AsGodotObject() is not Area2D area || area.GetParent() is not IDamageReceiver target) continue;
            if(ReferenceEquals(target,_soldier)||!_hitTargets.Add(area.GetParent().GetInstanceId())) continue;
            var hit=(cross?AttackHit.Cross:AttackHit.Jab) with { Percentage=cross?CrossDamage:JabDamage };
            target.ReceiveHit(hit,_soldier.Facing);
        }
    }
}
