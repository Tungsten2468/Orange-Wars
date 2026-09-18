using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActiveEffect
{
    public effect type;
    public int endTick;
    public int actionTick;
    public int ticksBetweenActions;
    public Action onApply;
    public Action onTick;
    public List<Action> revertActions = new List<Action>();

    public void endEffect(statusEffectManager mgr)
    {
        foreach (Action r in revertActions)
            r();

        mgr.me.myGraphics.color = Color.white;
    }
}

public class statusEffectManager : MonoBehaviour
{
    public entity me;
    public SpriteRenderer changeAppearance;
    public Action functionToCallback;
    public statusEffect lastEffectApplied;
    private List<ActiveEffect> activeEffects = new List<ActiveEffect>();

    public void Start()
    {
        me = GetComponent<entity>();
        tickManager.onTick += effectsTick;
    }

    public void beginEffect(statusEffect eff)
    {
        me.myAudio.PlayOneShot(eff.effectSound);
        lastEffectApplied = eff;

        // prevent duplicate of same effect type; just extend duration
        ActiveEffect existing = activeEffects.Find(e => e.type == eff.thisEffect);
        if (existing != null)
        {
            existing.endTick = tickManager.tickCount + Mathf.RoundToInt(eff.duration);
            return;
        }

        ActiveEffect newEff = new ActiveEffect();
        newEff.type = eff.thisEffect;
        newEff.endTick = tickManager.tickCount + Mathf.RoundToInt(eff.duration);
        newEff.onApply = functionToCallback;
        newEff.ticksBetweenActions = eff.ticksBetweenAction;
        newEff.actionTick = tickManager.tickCount; // start immediately for DOT

        switch (eff.thisEffect)
        {
            case effect.slow:
                applySlow(eff.magnitude, newEff);
                break;
            case effect.boost:
                applyBoost(eff.magnitude, newEff);
                break;
            case effect.suffocation:
                applySuffocation(eff.magnitude, newEff);
                break;
            case effect.sunscreened:
                applySunscreened(newEff, eff.swappedItem);
                break;
            case effect.eyesting:
                applyEyesting(eff.magnitude, newEff);
                break;
        }

        newEff.onApply?.Invoke();
        activeEffects.Add(newEff);
    }

    // FLOAT MODS: delta-based, stack-safe
    private void addMod(ActiveEffect eff, Action<float> applier, float delta)
    {
        applier(delta);
        eff.revertActions.Add(() => applier(-delta));
    }

    // BOOL MODS: store original value, revert once per effect
    private void addBoolMod(ActiveEffect eff, Action<bool> setter, bool oldVal, bool newVal)
    {
        setter(newVal);
        eff.revertActions.Add(() => setter(oldVal));
    }

    // GAMEOBJECT MODS: store original reference, revert once per effect
    private void addGameObjectMod(ActiveEffect eff, Action<GameObject> setter, GameObject oldVal, GameObject newVal)
    {
        setter(newVal);
        eff.revertActions.Add(() => setter(oldVal));
    }

    private void effectsTick()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            ActiveEffect eff = activeEffects[i];

            // per-tick logic (DOT, etc.)
            eff.onTick?.Invoke();

            // end check
            if (tickManager.tickCount >= eff.endTick)
            {
                eff.endEffect(this);
                activeEffects.RemoveAt(i);
            }
        }
    }

    public void OnDestroy()
    {
        tickManager.onTick -= effectsTick;
    }

    // EFFECT IMPLEMENTATIONS

    private void applySlow(float magnitude, ActiveEffect eff)
    {
        me.myGraphics.color = Color.magenta;

        if (me.thisEntity == entityType.Troop)
        {
            troopBehaviors t = me as troopBehaviors;

            addMod(eff, v => t.ticksBeforeShootModifier += v, magnitude);
            addMod(eff, v => t.movementSpeedModifier += v, -magnitude);
            addMod(eff, v => t.ticksBetweenProductionModifier += v, magnitude);
            addMod(eff, v => t.autoProduceTicksModifier += v, magnitude);
        }

        if (me.thisEntity == entityType.Enemy)
        {
            enemyBehaviors e = me as enemyBehaviors;

            addMod(eff, v => e.ticksBeforeShootModifier += v, magnitude);
            addMod(eff, v => e.ticksBeforeMeleeModifier += v, magnitude);
            addMod(eff, v => e.movementSpeedModifier += v, -magnitude);
        }
    }

    private void applyBoost(float magnitude, ActiveEffect eff)
    {
        me.myGraphics.color = Color.yellow;

        if (me.thisEntity == entityType.Troop)
        {
            troopBehaviors t = me as troopBehaviors;

            addMod(eff, v => t.ticksBeforeShootModifier += v, -magnitude);
            addMod(eff, v => t.rangedForceModifier += v, magnitude);
            addMod(eff, v => t.movementSpeedModifier += v, magnitude);
            addMod(eff, v => t.ticksBetweenProductionModifier += v, -magnitude);
            addMod(eff, v => t.autoProduceTicksModifier += v, -magnitude);

            if (functionToCallback != null)
                functionToCallback();
        }
    }

    private void applySuffocation(float magnitude, ActiveEffect eff)
    {
        me.myGraphics.color = Color.blue;

        eff.onTick = () =>
        {
            if (tickManager.tickCount >= eff.actionTick)
            {
                me.takeDamage(magnitude);
                eff.actionTick = tickManager.tickCount + eff.ticksBetweenActions;
            }
        };
    }

    private void applySunscreened(ActiveEffect eff, GameObject newProj)
    {
        if (me.thisEntity == entityType.Enemy)
        {
            me.myGraphics.color = Color.yellow;
            enemyBehaviors en = me as enemyBehaviors;

            addGameObjectMod(eff, v => en.mainProjectile = v, en.mainProjectile, newProj);
        }
    }

    private void applyEyesting(float magnitude, ActiveEffect eff)
    {
        me.myGraphics.color = Color.gray;

        // stun ONCE when effect starts, revert when effect ends
        addBoolMod(eff, v => me.stunned = v, me.stunned, true);

        eff.onTick = () =>
        {
            if (tickManager.tickCount >= eff.actionTick)
            {
                me.takeDamage(magnitude);
                eff.actionTick = tickManager.tickCount + eff.ticksBetweenActions;
            }
        };
    }
}
