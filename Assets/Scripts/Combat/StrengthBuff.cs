using System;
using UnityEngine;

// A temporary damage bonus (the Strength Elixir). Only stores the bonus and when it ends;
// PlayerCombat asks bonusDamage on every hit, the item decides how much and how long.
// Like Health's i-frames it keeps a Time.time timestamp, so it needs no Update.
public class StrengthBuff : MonoBehaviour
{
    private int bonus;
    private float endTime;

    public bool isActive => Time.time < endTime;

    // Extra damage per hit right now (0 once the buff has run out).
    public int bonusDamage => isActive ? bonus : 0;

    // Seconds left, for a HUD icon later.
    public float timeLeft => Mathf.Max(0f, endTime - Time.time);

    // Fired when a buff starts or is refreshed (bonus, duration). A future HUD icon listens.
    public event Action<int, float> OnBuffApplied;

    // Drinking again refreshes the timer; buffs don't stack, the stronger bonus wins.
    public void Apply(int bonusDamage, float duration)
    {
        bonus = isActive ? Mathf.Max(bonus, bonusDamage) : bonusDamage;
        endTime = Mathf.Max(endTime, Time.time + duration);
        OnBuffApplied?.Invoke(bonus, duration);
    }
}
