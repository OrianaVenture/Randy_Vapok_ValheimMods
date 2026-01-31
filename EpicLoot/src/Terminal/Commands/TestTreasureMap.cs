using System;
using System.Collections;
using System.Linq;
using EpicLoot.Adventure;
using UnityEngine;

namespace EpicLoot;

public static partial class MagicCommands
{
    private static void TestTreasureMap(Terminal.ConsoleEventArgs args)
    {
        Player player = Player.m_localPlayer;

        int count = args.GetInt(2, 1);
        Heightmap.Biome biome = Heightmap.Biome.None;
        string land = args.GetString(3);
        if (!string.IsNullOrEmpty(land))
        {
            Enum.TryParse(land, out biome);
        }

        int overrideTreasureMapCount = args.GetInt(4, -1);

        AdventureDataManager.CheatNumberOfBounties = overrideTreasureMapCount;
        AdventureSaveData saveData = player.GetAdventureSaveData();
        player.StartCoroutine(TestTreasureMapCoroutine(saveData, biome, player, count));
    }    
    
    // TODO: update these tests
    private static IEnumerator TestTreasureMapCoroutine(AdventureSaveData saveData, Heightmap.Biome biome, Player player, int count)
    {
        Heightmap.Biome[] biomes = new[] { Heightmap.Biome.Meadows, Heightmap.Biome.BlackForest, Heightmap.Biome.Swamp,
            Heightmap.Biome.Mountain, Heightmap.Biome.Plains };

        saveData.DebugMode = true;
        int startInterval = saveData.TreasureMaps.Count == 0 ? -1 : saveData.TreasureMaps.Min(x => x.Interval) - 1;
        for (int i = 0; i < count; ++i)
        {
            saveData.IntervalOverride = startInterval - (i + 1);
            Heightmap.Biome selectedBiome = biome == Heightmap.Biome.None ? biomes[UnityEngine.Random.Range(0, biomes.Length)] : biome;
            yield return AdventureDataManager.TreasureMaps.SpawnTreasureChest(selectedBiome, player, 0, OnTreasureChestSpawnComplete);
        }
        saveData.DebugMode = false;
        AdventureDataManager.CheatNumberOfBounties = -1;
    }
    
    private static void OnTreasureChestSpawnComplete(int price, bool success, Vector3 spawnPoint)
    {
        string output = "> Failed to spawn treasure map chest";
        if (success)
        {
            output = $"> Spawning Treasure Map Chest at <{spawnPoint.x:0.#}, {spawnPoint.z:0.#}> (height:{spawnPoint.y:0.#})";
        }

        Console.instance.AddString(output);
        EpicLoot.LogWarning(output);
    }
}