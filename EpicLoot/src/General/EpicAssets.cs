namespace EpicLoot;

using Jotunn.Managers;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class EpicAssets
{
    public AssetBundle AssetBundle;
    public static Dictionary<string, Object> AssetCache = new Dictionary<string, Object>();

    public Sprite EquippedSprite;
    public Sprite AugaEquippedSprite;
    public Sprite GenericSetItemSprite;
    public Sprite AugaSetItemSprite;
    public Sprite GenericItemBgSprite;
    public Sprite AugaItemBgSprite;
    public Sprite EnchantmentSparkle;
    public GameObject[] MagicItemLootBeamPrefabs = new GameObject[5];
    public readonly Dictionary<string, GameObject[]> CraftingMaterialPrefabs = new Dictionary<string, GameObject[]>();
    public Sprite SmallButtonEnchantOverlay;
    public Sprite DodgeBuffSprite;
    public AudioClip[] MagicItemDropSFX = new AudioClip[5];
    public AudioClip ItemLoopSFX;
    public AudioClip AugmentItemSFX;
    public GameObject MerchantPanel;
    public Sprite MapIconTreasureMap;
    public Sprite MapIconBounty;
    public AudioClip AbandonBountySFX;
    public AudioClip DoubleJumpSFX;
    public AudioClip DodgeBuffSFX;
    public AudioClip OffSetSFX;
    public GameObject DebugTextPrefab;
    public GameObject AbilityBar;
    public GameObject WelcomMessagePrefab;

    public const string DummyName = "EL_DummyPrefab";
    public static GameObject DummyPrefab() => PrefabManager.Instance.GetPrefab(DummyName);
}
