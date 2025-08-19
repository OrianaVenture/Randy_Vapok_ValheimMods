using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace EpicLootAPI;

[PublicAPI]
public class AbilityProxyDefinition
{
    public readonly AbilityDefinition Ability;
    public readonly Dictionary<string, Delegate> Callbacks = new();
    
    [Description("Register a complex ability behavior using Proxy class")]
    public AbilityProxyDefinition(string ID, AbilityActivationMode mode,  Proxy definition)
    {
        Ability = new  AbilityDefinition(ID, mode);
        RegisterInterfaceCallbacks(definition);
    }
    
    [Description("Register a complex ability behavior using Proxy class")]
    public AbilityProxyDefinition(string ID, AbilityActivationMode mode, Type type)
    {
        Ability = new  AbilityDefinition(ID, mode);
        if (!typeof(Proxy).IsAssignableFrom(type))
        {
            Debug.LogError($"Ability Proxy {ID} Type {type.Name} does not implement Proxy class");
            return;
        }
        try
        {
            var proxy = Activator.CreateInstance(type);
            RegisterInterfaceCallbacks((Proxy)proxy);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create instance of {type.Name}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Gets the method names and constructs delegates
    /// </summary>
    /// <param name="implementation"></param>
    private void RegisterInterfaceCallbacks(Proxy implementation)
    {
        Type implementationType = implementation.GetType();
        Type interfaceType = typeof(Proxy);
        MethodInfo[] interfaceMethods = interfaceType.GetMethods();
        
        foreach (MethodInfo interfaceMethod in interfaceMethods)
        {
            try
            {
                // Find the corresponding implementation method
                Type[] parameterTypes = interfaceMethod.GetParameters().Select(p => p.ParameterType).ToArray();
                MethodInfo? implementationMethod = implementationType.GetMethod(interfaceMethod.Name, parameterTypes);
                
                if (implementationMethod == null)
                {
                    Debug.LogWarning($"Method {interfaceMethod.Name} not found in implementation");
                    continue;
                }
                
                // Create proper delegate types based on method signature
                Type delegateType;
                
                if (interfaceMethod.ReturnType == typeof(void))
                {
                    // Action delegate
                    if (parameterTypes.Length == 0)
                        delegateType = typeof(Action);
                    else
                        delegateType = Expression.GetActionType(parameterTypes);
                }
                else
                {
                    // Func delegate
                    Type[] allTypes = parameterTypes.Concat(new[] { interfaceMethod.ReturnType }).ToArray();
                    delegateType = Expression.GetFuncType(allTypes);
                }
                
                Delegate del = Delegate.CreateDelegate(delegateType, implementation, implementationMethod);
                Callbacks[interfaceMethod.Name] = del;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to register callback for method {interfaceMethod.Name}: {ex.Message}");
            }
        }
    }
}

[PublicAPI]
public class Proxy
{
    public virtual string CooldownEndKey => $"EpicLoot.{AbilityID}.CooldownEnd";
    protected Player? Player;
    public string AbilityID = "";
    public float Cooldown;
    
    public virtual void Initialize(Player player, string id, float cooldown)
    {
        Player = player;
        AbilityID = id;
        Cooldown = cooldown;
    }

    public virtual void OnUpdate()
    {
        
    }

    public virtual bool ShouldTrigger()
    {
        return false;
    }

    public virtual bool IsOnCooldown()
    {
        if (HasCooldown())
        {
            return GetTime() < GetCooldownEndTime();
        }

        return false;
    }

    public virtual float TimeUntilCooldownEnds()
    {
        return Mathf.Max(0, GetCooldownEndTime() - GetTime());
    }

    public virtual float PercentCooldownComplete()
    {
        if (HasCooldown() && IsOnCooldown())
        {
            return 1.0f - TimeUntilCooldownEnds() / Cooldown;
        }

        return 1.0f;
    }

    public virtual bool CanActivate()
    {
        return !IsOnCooldown();
    }

    public virtual void TryActivate()
    {
        if (CanActivate())
        {
            Activate();
        }
    }

    public virtual void Activate()
    {
        if (HasCooldown())
        {
            var cooldownEndtime = GetTime() + Cooldown;
            SetCooldownEndTime(cooldownEndtime);
        }
    }

    public virtual void ActivateCustomAction()
    {
        
    }

    public virtual void ActivateStatusEffectAction()
    {
        
    }

    public virtual bool HasCooldown()
    {
        return Cooldown > 0;
    }

    public virtual void SetCooldownEndTime(float cooldownEndTime)
    {
        if (Player == null) return;
        Player.m_nview.GetZDO().Set(CooldownEndKey, cooldownEndTime);
    }

    public virtual float GetCooldownEndTime()
    {
        if (Player == null) return 0f;
        return Player.m_nview.GetZDO().GetFloat(CooldownEndKey, 0);
    }

    protected static float GetTime() => (float)ZNet.instance.GetTimeSeconds();
}