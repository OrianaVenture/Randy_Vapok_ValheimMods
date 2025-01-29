using EpicLoot.Adventure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EpicLoot.src.Adventure.feature
{
    internal static class BountyLocationEarlyCache
    {
        // This could be shifted to multiple variable zsynced lists to preserve the generated values for future use.
        public static Dictionary<Heightmap.Biome, List<Vector3>> PotentialBiomeLocations = new Dictionary<Heightmap.Biome, List<Vector3>> { };

    internal static IEnumerator LazyCacheGetBiomePoint(Heightmap.Biome biome, AdventureSaveData saveData, Action<bool, Vector3, Vector3> onComplete)
        {
            MerchantPanel.ShowInputBlocker(true);
            var radiusRange = GetTreasureMapSpawnRadiusRange(biome, saveData);
            var tries = 0;
            bool spawn_set = false;
            // The ideal scenario, this is what happens most of the time.
            // When this is not triggered its because there is no data yet for the biome
            if (PotentialBiomeLocations.ContainsKey(biome) && PotentialBiomeLocations[biome].Count() > 1)
            {
                SelectSpawnPoint(biome, onComplete);
                spawn_set = true;
            }

            // Initial setup for the selected biome- and likely other biomes.
            // We don't need to build a large cache here because we just need to get a spawnpoint setup so that we can return it
            // The following run will expand the cache
            // Ultimately we don't need a lot of locations cached because the user can only accept them so often.


            // If this biome key does not exist, add it.
            if (!PotentialBiomeLocations.ContainsKey(biome)) { PotentialBiomeLocations.Add(biome, new List<Vector3>() { }); }

            while (PotentialBiomeLocations[biome].Count() < 3) {
                EpicLoot.Log($"Finding {biome} spawn point, currently sored: {PotentialBiomeLocations[biome].Count()} < 3");
                if (tries % 10 == 0 && tries > 1) {
                    yield return new WaitForSeconds(1f);
                }
                tries++;

                var temp_spawnPoint = SelectWorldPoint(radiusRange, tries);
                // Ensure the location is spawned.
                var zoneId = ZoneSystem.GetZone(temp_spawnPoint);
                while (!ZoneSystem.instance.SpawnZone(zoneId, ZoneSystem.SpawnMode.Client, out _)) {
                    // slow down this loop until the zone is spawned.
                    yield return new WaitForEndOfFrame();
                }
                bool valid_location = IsSpawnLocationValid(temp_spawnPoint, out Heightmap.Biome spawn_location_biome);
                EpicLoot.Log($"Trying to find a spawn point for biome {biome}: found {spawn_location_biome} - Attempt: {tries} - Location: {temp_spawnPoint}");
                if (!valid_location) {
                    continue;
                }
                if (!PotentialBiomeLocations.ContainsKey(spawn_location_biome)) { 
                    PotentialBiomeLocations.Add(spawn_location_biome, new List<Vector3>() { }); 
                }
                EpicLoot.Log($"Adding {spawn_location_biome} location.");
                PotentialBiomeLocations[spawn_location_biome].Add(temp_spawnPoint);
                // We want to run the loop to select a location once and trigger the bounty/treasure to continue
                // then we keep adding a few locations so future iterations are much faster.
                
                //only follow through with the spawn point for the biome we are looking for, since we might find other valid spawnpoints for other biomes first.
                if (spawn_set == false && spawn_location_biome == biome)
                {
                    SelectSpawnPoint(biome, onComplete);
                    spawn_set = true;
                }
            }
            yield break;
        }

        internal static void SelectSpawnPoint(Heightmap.Biome biome, Action<bool, Vector3, Vector3> onComplete)
        {
            MerchantPanel.ShowInputBlocker(false);
            List<Vector3> locations = PotentialBiomeLocations[biome];
            Vector3 selected_location = locations.First();
            ZoneSystem.instance.GetGroundData(ref selected_location, out var normal, out var foundBiome, out var biomeArea, out var hmap);
            bool removed = locations.Remove(selected_location);
            PotentialBiomeLocations[biome] = locations;
            EpicLoot.Log($"selected: x:{selected_location.x}, y:{selected_location.y}, z:{selected_location.z} | entry consumed: {removed}");
            // Place the spawn creator decently above terrain and ground objects.
            // This is to allow re-checking the location for validity once everything is loaded.
            selected_location.y += 100f;
            onComplete?.Invoke(true, selected_location, normal);
        }

        internal static Vector3 SelectWorldPoint(Tuple<float, float> range, int attempt)
        {
            // This uses a modulus operator to create oscilation between zero and 5, making our attempted range size vary.
            // This prevents us from running into the issue where we attempt to spawn outside of the map- assuming the default scan range is sane.
            int interval_range = attempt % 5;
            var randomPoint = UnityEngine.Random.insideUnitCircle;
            var mag = randomPoint.magnitude;
            var normalized = randomPoint.normalized;
            var actualMag = Mathf.Lerp(range.Item1 + (interval_range * 250), range.Item2 + (interval_range * 250), mag);
            randomPoint = normalized * actualMag;
            return new Vector3(randomPoint.x, 0, randomPoint.y);
        }

        internal static bool IsSpawnLocationValid(Vector3 location, out Heightmap.Biome biome)
        {
            biome = Heightmap.Biome.None;

            ZoneSystem.instance.GetGroundData(ref location, out var normal, out var foundBiome, out var biomeArea, out var hmap);
            // set the biome this relates to
            biome = foundBiome;

            float groundHeight = location.y;


            // Ashlands biome, and location is in lava | Don't spawn in lava
            if (foundBiome == Heightmap.Biome.AshLands && hmap.GetVegetationMask(location) > 0.6f)
            {
                EpicLoot.Log("Spawn Point rejected: In lava");
                return false;
            }

            var waterLevel = ZoneSystem.instance.m_waterLevel;
            // 5f is a buffer here becasue the swamp is very low to the water level
            if (biome != Heightmap.Biome.Ocean && ZoneSystem.instance.m_waterLevel > groundHeight + 5f)
            {
                EpicLoot.Log($"Spawn Point rejected: too deep underwater (waterLevel:{waterLevel}, groundHeight:{groundHeight})");
                return false;
            }

            // Is too near to player base
            if (EffectArea.IsPointInsideArea(location, EffectArea.Type.PlayerBase, AdventureDataManager.Config.TreasureMap.MinimapAreaRadius))
            {
                EpicLoot.Log("Spawn Point rejected: Too close to player base");
                return false;
            }

            // Is too near to player ward
            // This is kind of expensive, so lets avoid it if we can
            //var tooCloseToWard = PrivateArea.m_allAreas.Any(x => x.IsInside(location, AdventureDataManager.Config.TreasureMap.MinimapAreaRadius));
            //if (tooCloseToWard)
            //{
            //    EpicLoot.Log("Spawn Point rejected: too close to player ward");
            //    return false;
            //}

            return true;
        }

        private static Tuple<float, float> GetTreasureMapSpawnRadiusRange(Heightmap.Biome biome, AdventureSaveData saveData)
        {
            var biomeInfoConfig = GetBiomeInfoConfig(biome);
            if (biomeInfoConfig == null)
            {
                EpicLoot.LogError($"Could not get biome info for biome: {biome}!");
                EpicLoot.LogWarning($"> Current BiomeInfo ({AdventureDataManager.Config.TreasureMap.BiomeInfo.Count}):");
                foreach (var biomeInfo in AdventureDataManager.Config.TreasureMap.BiomeInfo)
                {
                    EpicLoot.Log($"- {biomeInfo.Biome}: min:{biomeInfo.MinRadius}, max:{biomeInfo.MaxRadius}");
                }

                return new Tuple<float, float>(-1, -1);
            }

            var minSearchRange = biomeInfoConfig.MinRadius;
            var maxSearchRange = biomeInfoConfig.MaxRadius;
            var searchBandWidth = AdventureDataManager.Config.TreasureMap.StartRadiusMax - AdventureDataManager.Config.TreasureMap.StartRadiusMin;
            var numberOfBounties = AdventureDataManager.CheatNumberOfBounties >= 0 ? AdventureDataManager.CheatNumberOfBounties : saveData.NumberOfTreasureMapsOrBountiesStarted;
            var increments = numberOfBounties / AdventureDataManager.Config.TreasureMap.IncreaseRadiusCount;
            var min1 = minSearchRange + (AdventureDataManager.Config.TreasureMap.StartRadiusMin + increments * AdventureDataManager.Config.TreasureMap.RadiusInterval);
            var max1 = min1 + searchBandWidth;
            var min = Mathf.Clamp(min1, minSearchRange, maxSearchRange - searchBandWidth);
            var max = Mathf.Clamp(max1, minSearchRange + searchBandWidth, maxSearchRange);
            EpicLoot.Log($"Got biome info for biome ({biome}) - Overall search range: {minSearchRange}-{maxSearchRange}. Current increments: {increments}. Current search band: {min}-{max} (width={searchBandWidth})");
            return new Tuple<float, float>(min, max);
        }

        private static TreasureMapBiomeInfoConfig GetBiomeInfoConfig(Heightmap.Biome biome)
        {
            return AdventureDataManager.Config.TreasureMap.BiomeInfo.Find(x => x.Biome == biome);
        }
    }
}
