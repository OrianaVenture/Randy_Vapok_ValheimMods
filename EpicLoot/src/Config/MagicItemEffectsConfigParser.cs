using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace EpicLoot.Config
{
    internal static class MagicItemEffectsConfigParser
    {
        public static MagicItemEffectsList Deserialize(string json)
        {
            JObject root = JObject.Parse(json);
            MagicItemEffectsList result = new MagicItemEffectsList();
            JToken effectsToken = GetProperty(root, nameof(MagicItemEffectsList.MagicItemEffects));

            if (effectsToken == null || effectsToken.Type == JTokenType.Null)
            {
                return result;
            }

            if (effectsToken is not JArray effects)
            {
                throw new JsonSerializationException("MagicItemEffects must be an array.");
            }

            foreach (JToken effectToken in effects)
            {
                if (effectToken is not JObject effectObject)
                {
                    throw new JsonSerializationException("Each MagicItemEffects entry must be an object.");
                }

                result.MagicItemEffects.Add(ReadEffectDefinition(effectObject));
            }

            return result;
        }

        private static MagicItemEffectDefinition ReadEffectDefinition(JObject source)
        {
            MagicItemEffectDefinition effectDefinition = new MagicItemEffectDefinition();

            ReadString(source, nameof(MagicItemEffectDefinition.Type), value => effectDefinition.Type = value);
            ReadString(source, nameof(MagicItemEffectDefinition.DisplayText), value => effectDefinition.DisplayText = value);
            ReadString(source, nameof(MagicItemEffectDefinition.Description), value => effectDefinition.Description = value);
            ReadObject(source, nameof(MagicItemEffectDefinition.Requirements), value => effectDefinition.Requirements = value == null ? null : ReadRequirements(value));
            ReadObject(source, nameof(MagicItemEffectDefinition.ValuesPerRarity), value => effectDefinition.ValuesPerRarity = value == null ? null : ReadValuesPerRarity(value));
            ReadFloat(source, nameof(MagicItemEffectDefinition.SelectionWeight), value => effectDefinition.SelectionWeight = value);
            ReadBool(source, nameof(MagicItemEffectDefinition.CanBeAugmented), value => effectDefinition.CanBeAugmented = value);
            ReadBool(source, nameof(MagicItemEffectDefinition.CanBeDisenchanted), value => effectDefinition.CanBeDisenchanted = value);
            ReadBool(source, nameof(MagicItemEffectDefinition.CanBeRunified), value => effectDefinition.CanBeRunified = value);
            ReadString(source, nameof(MagicItemEffectDefinition.Comment), value => effectDefinition.Comment = value);
            ReadStringList(source, nameof(MagicItemEffectDefinition.Prefixes), value => effectDefinition.Prefixes = value);
            ReadStringList(source, nameof(MagicItemEffectDefinition.Suffixes), value => effectDefinition.Suffixes = value);
            ReadString(source, nameof(MagicItemEffectDefinition.EquipFx), value => effectDefinition.EquipFx = value);
            ReadEnum<FxAttachMode>(source, nameof(MagicItemEffectDefinition.EquipFxMode), value => effectDefinition.EquipFxMode = value);
            ReadString(source, nameof(MagicItemEffectDefinition.Ability), value => effectDefinition.Ability = value);
            ReadFloatDictionary(source, nameof(MagicItemEffectDefinition.Config), value => effectDefinition.Config = value);

            return effectDefinition;
        }

        private static MagicItemEffectRequirements ReadRequirements(JObject source)
        {
            MagicItemEffectRequirements requirements = new MagicItemEffectRequirements();

            ReadBool(source, nameof(MagicItemEffectRequirements.NoRoll), value => requirements.NoRoll = value);
            ReadBool(source, nameof(MagicItemEffectRequirements.ExclusiveSelf), value => requirements.ExclusiveSelf = value);
            ReadStringList(source, nameof(MagicItemEffectRequirements.ExclusiveEffectTypes), value => requirements.ExclusiveEffectTypes = value);
            ReadStringList(source, nameof(MagicItemEffectRequirements.MustHaveEffectTypes), value => requirements.MustHaveEffectTypes = value);
            ReadStringList(source, nameof(MagicItemEffectRequirements.AllowedItemTypes), value => requirements.AllowedItemTypes = value);
            ReadStringList(source, nameof(MagicItemEffectRequirements.ExcludedItemTypes), value => requirements.ExcludedItemTypes = value);
            ReadEnumList<ItemRarity>(source, nameof(MagicItemEffectRequirements.AllowedRarities), value => requirements.AllowedRarities = value);
            ReadEnumList<ItemRarity>(source, nameof(MagicItemEffectRequirements.ExcludedRarities), value => requirements.ExcludedRarities = value);
            ReadEnumList<Skills.SkillType>(source, nameof(MagicItemEffectRequirements.AllowedSkillTypes), value => requirements.AllowedSkillTypes = value);
            ReadEnumList<Skills.SkillType>(source, nameof(MagicItemEffectRequirements.ExcludedSkillTypes), value => requirements.ExcludedSkillTypes = value);
            ReadStringList(source, nameof(MagicItemEffectRequirements.AllowedItemNames), value => requirements.AllowedItemNames = value);
            ReadStringList(source, nameof(MagicItemEffectRequirements.ExcludedItemNames), value => requirements.ExcludedItemNames = value);
            ReadNullableBool(source, nameof(MagicItemEffectRequirements.ItemHasPhysicalDamage), value => requirements.ItemHasPhysicalDamage = value);
            ReadNullableBool(source, nameof(MagicItemEffectRequirements.ItemHasElementalDamage), value => requirements.ItemHasElementalDamage = value);
            ReadNullableBool(source, nameof(MagicItemEffectRequirements.ItemHasChopDamage), value => requirements.ItemHasChopDamage = value);
            ReadNullableBool(source, nameof(MagicItemEffectRequirements.ItemUsesDurability), value => requirements.ItemUsesDurability = value);
            ReadNullableBool(source, nameof(MagicItemEffectRequirements.ItemHasNegativeMovementSpeedModifier), value => requirements.ItemHasNegativeMovementSpeedModifier = value);
            ReadNullableBool(source, nameof(MagicItemEffectRequirements.ItemHasBlockPower), value => requirements.ItemHasBlockPower = value);
            ReadNullableBool(source, nameof(MagicItemEffectRequirements.ItemHasParryPower), value => requirements.ItemHasParryPower = value);
            ReadNullableBool(source, nameof(MagicItemEffectRequirements.ItemHasNoParryPower), value => requirements.ItemHasNoParryPower = value);
            ReadNullableBool(source, nameof(MagicItemEffectRequirements.ItemHasArmor), value => requirements.ItemHasArmor = value);
            ReadNullableBool(source, nameof(MagicItemEffectRequirements.ItemHasBackstabBonus), value => requirements.ItemHasBackstabBonus = value);
            ReadNullableBool(source, nameof(MagicItemEffectRequirements.ItemUsesStaminaOnAttack), value => requirements.ItemUsesStaminaOnAttack = value);
            ReadNullableBool(source, nameof(MagicItemEffectRequirements.ItemUsesEitrOnAttack), value => requirements.ItemUsesEitrOnAttack = value);
            ReadNullableBool(source, nameof(MagicItemEffectRequirements.ItemUsesHealthOnAttack), value => requirements.ItemUsesHealthOnAttack = value);
            ReadNullableBool(source, nameof(MagicItemEffectRequirements.ItemUsesDrawStaminaOnAttack), value => requirements.ItemUsesDrawStaminaOnAttack = value);
            ReadStringList(source, nameof(MagicItemEffectRequirements.CustomFlags), value => requirements.CustomFlags = value);

            return requirements;
        }

        private static MagicItemEffectDefinition.ValuesPerRarityDef ReadValuesPerRarity(JObject source)
        {
            MagicItemEffectDefinition.ValuesPerRarityDef valuesPerRarity = new MagicItemEffectDefinition.ValuesPerRarityDef();

            ReadValueDef(source, nameof(MagicItemEffectDefinition.ValuesPerRarityDef.Magic), value => valuesPerRarity.Magic = value);
            ReadValueDef(source, nameof(MagicItemEffectDefinition.ValuesPerRarityDef.Rare), value => valuesPerRarity.Rare = value);
            ReadValueDef(source, nameof(MagicItemEffectDefinition.ValuesPerRarityDef.Epic), value => valuesPerRarity.Epic = value);
            ReadValueDef(source, nameof(MagicItemEffectDefinition.ValuesPerRarityDef.Legendary), value => valuesPerRarity.Legendary = value);
            ReadValueDef(source, nameof(MagicItemEffectDefinition.ValuesPerRarityDef.Mythic), value => valuesPerRarity.Mythic = value);

            return valuesPerRarity;
        }

        private static void ReadValueDef(JObject source, string propertyName, Action<MagicItemEffectDefinition.ValueDef> assign)
        {
            JToken token = GetProperty(source, propertyName);

            if (token == null)
            {
                return;
            }

            if (token.Type == JTokenType.Null)
            {
                assign(null);
                return;
            }

            if (token is not JObject valueObject)
            {
                throw new JsonSerializationException($"{propertyName} must be an object.");
            }

            MagicItemEffectDefinition.ValueDef valueDef = new MagicItemEffectDefinition.ValueDef();
            ReadFloat(valueObject, nameof(MagicItemEffectDefinition.ValueDef.MinValue), value => valueDef.MinValue = value);
            ReadFloat(valueObject, nameof(MagicItemEffectDefinition.ValueDef.MaxValue), value => valueDef.MaxValue = value);
            ReadFloat(valueObject, nameof(MagicItemEffectDefinition.ValueDef.Increment), value => valueDef.Increment = value);
            assign(valueDef);
        }

        private static void ReadObject(JObject source, string propertyName, Action<JObject> assign)
        {
            JToken token = GetProperty(source, propertyName);

            if (token == null)
            {
                return;
            }

            if (token.Type == JTokenType.Null)
            {
                assign(null);
                return;
            }

            if (token is not JObject value)
            {
                throw new JsonSerializationException($"{propertyName} must be an object.");
            }

            assign(value);
        }

        private static void ReadString(JObject source, string propertyName, Action<string> assign)
        {
            JToken token = GetProperty(source, propertyName);

            if (token == null)
            {
                return;
            }

            assign(token.Type == JTokenType.Null ? null : token.Value<string>());
        }

        private static void ReadBool(JObject source, string propertyName, Action<bool> assign)
        {
            JToken token = GetProperty(source, propertyName);

            if (token == null || token.Type == JTokenType.Null)
            {
                return;
            }

            assign(token.Value<bool>());
        }

        private static void ReadNullableBool(JObject source, string propertyName, Action<bool?> assign)
        {
            JToken token = GetProperty(source, propertyName);

            if (token == null)
            {
                return;
            }

            assign(token.Type == JTokenType.Null ? null : token.Value<bool>());
        }

        private static void ReadFloat(JObject source, string propertyName, Action<float> assign)
        {
            JToken token = GetProperty(source, propertyName);

            if (token == null || token.Type == JTokenType.Null)
            {
                return;
            }

            assign(token.Value<float>());
        }

        private static void ReadEnum<TEnum>(JObject source, string propertyName, Action<TEnum> assign) where TEnum : struct
        {
            JToken token = GetProperty(source, propertyName);

            if (token == null || token.Type == JTokenType.Null)
            {
                return;
            }

            assign(ReadEnumToken<TEnum>(token, propertyName));
        }

        private static void ReadStringList(JObject source, string propertyName, Action<List<string>> assign)
        {
            JToken token = GetProperty(source, propertyName);

            if (token == null)
            {
                return;
            }

            if (token.Type == JTokenType.Null)
            {
                assign(null);
                return;
            }

            if (token is not JArray values)
            {
                throw new JsonSerializationException($"{propertyName} must be an array.");
            }

            List<string> result = new List<string>();
            foreach (JToken value in values)
            {
                result.Add(value.Type == JTokenType.Null ? null : value.Value<string>());
            }

            assign(result);
        }

        private static void ReadEnumList<TEnum>(JObject source, string propertyName, Action<List<TEnum>> assign) where TEnum : struct
        {
            JToken token = GetProperty(source, propertyName);

            if (token == null)
            {
                return;
            }

            if (token.Type == JTokenType.Null)
            {
                assign(null);
                return;
            }

            if (token is not JArray values)
            {
                throw new JsonSerializationException($"{propertyName} must be an array.");
            }

            List<TEnum> result = new List<TEnum>();
            foreach (JToken value in values)
            {
                result.Add(ReadEnumToken<TEnum>(value, propertyName));
            }

            assign(result);
        }

        private static void ReadFloatDictionary(JObject source, string propertyName, Action<Dictionary<string, float>> assign)
        {
            JToken token = GetProperty(source, propertyName);

            if (token == null)
            {
                return;
            }

            if (token.Type == JTokenType.Null)
            {
                assign(null);
                return;
            }

            if (token is not JObject dictionaryObject)
            {
                throw new JsonSerializationException($"{propertyName} must be an object.");
            }

            Dictionary<string, float> result = new Dictionary<string, float>();
            foreach (JProperty property in dictionaryObject.Properties())
            {
                result[property.Name] = property.Value.Value<float>();
            }

            assign(result);
        }

        private static TEnum ReadEnumToken<TEnum>(JToken token, string propertyName) where TEnum : struct
        {
            if (token.Type == JTokenType.Integer)
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), token.Value<int>());
            }

            string enumText = token.Value<string>();
            if (Enum.TryParse(enumText, true, out TEnum result))
            {
                return result;
            }

            throw new JsonSerializationException($"Invalid {typeof(TEnum).Name} value '{enumText}' in {propertyName}.");
        }

        private static JToken GetProperty(JObject source, string propertyName)
        {
            JProperty property = source.Property(propertyName, StringComparison.OrdinalIgnoreCase);
            return property?.Value;
        }
    }
}
