using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;

namespace EpicLootAPI;

[Serializable][PublicAPI]
public enum AbilityActivationMode
{
    Passive,
    Triggerable,
    Activated
}

[Serializable][PublicAPI]
public enum AbilityAction
{
    Custom,
    StatusEffect
}

[Serializable][PublicAPI]
public class AbilityDefinition
{
    public string ID;
    public string IconAsset = ""; // Will need to tweak EpicLoot class to allow for custom icons to be passed
    public AbilityActivationMode ActivationMode; // Only Activate works, since Triggerable is unique per Ability
    public float Cooldown;
    public AbilityAction Action; // Always Status Effect since custom is too complex behavior to pass through
    public List<string> ActionParams = new List<string>();

    [Description("Register a status effect ability which activates on player input")]
    public AbilityDefinition(string ID, string iconAsset, float cooldown, string statusEffectName)
    {
        this.ID = ID;
        ActivationMode = AbilityActivationMode.Activated;
        Cooldown = cooldown;
        Action = AbilityAction.StatusEffect;
        ActionParams.Add(statusEffectName);
        EpicLoot.Abilities.Add(this);
    }
    
    internal AbilityDefinition(string ID, AbilityActivationMode mode)
    {
        this.ID = ID;
        ActivationMode = mode;
    }
}