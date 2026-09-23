using System;

namespace UnnamedFightingGame;

/// <summary>Two edge-triggered punches with one buffer, a grace window and a hard ending.</summary>
public sealed class FistCombo
{
    public const double GraceSeconds = .200;
    public const double CooldownSeconds = .150;
    private enum Phase { Ready, First, Grace, Second, Cooldown }
    private Phase _phase;
    private double _left;
    public bool IsAttacking => _phase is Phase.First or Phase.Second;
    public int AttackNumber => _phase == Phase.First ? 1 : _phase == Phase.Second ? 2 : 0;
    public bool Queued { get; private set; }

    public bool Advance(double delta, bool pressed)
    {
        bool started = false;
        // Carry over elapsed time; exact boundaries belong to the next phase.
        while (_phase != Phase.Ready && delta + 1e-7 >= _left)
        {
            delta = Math.Max(0, delta - _left);
            switch (_phase)
            {
                case Phase.First:
                    if (Queued) { Start(Phase.Second, SoldierVisual.CrossSeconds); started = true; }
                    else Start(Phase.Grace, GraceSeconds);
                    Queued = false;
                    break;
                case Phase.Grace: _phase = Phase.Ready; break;
                case Phase.Second: Start(Phase.Cooldown, CooldownSeconds); started = false; break;
                case Phase.Cooldown: _phase = Phase.Ready; break;
            }
        }
        if (_phase != Phase.Ready) _left -= delta;
        if (pressed)
        {
            if (_phase == Phase.First) Queued = true;
            else if (_phase == Phase.Grace) { Start(Phase.Second, SoldierVisual.CrossSeconds); started = true; }
            else if (_phase == Phase.Ready) { Start(Phase.First, SoldierVisual.AttackSeconds); started = true; }
        }
        return started;
    }

    private void Start(Phase phase, double seconds) { _phase = phase; _left = seconds; }
    public void Reset() { _phase = Phase.Ready; _left = 0; Queued = false; }
}
