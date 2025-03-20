using EpicLoot.Adventure;
using EpicLoot.src.data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Heightmap;

namespace EpicLoot.src.Adventure.bounties
{
    internal class AdventureSpawnController : MonoBehaviour
    {
        protected ZNetView zNetView;
        private static BountyInfoZNetProperty bounty { get; set; }
        private static TreasureMapChestInfoZNetProperty treasure { get; set; }
        private static BoolZNetProperty placed { get; set; }
        private static BoolZNetProperty searching_for_spawn { get; set; }
        private static Vector3ZNetProperty spawnpoint { get; set; }

        private static int current_updates = 0;
        private static bool started_placement = false;
        private static Vector3 defaultspawn = new Vector3(1, 1, 1);
        private static BountyInfo defaultbounty = new BountyInfo();
        private static TreasureMapChestInfo defaulttreasure = new TreasureMapChestInfo();

        public void Awake()
        {
            if (this.gameObject.TryGetComponent<ZNetView>(out zNetView) == false)
            {
                this.gameObject.AddComponent<ZNetView>();
                zNetView = this.gameObject.GetComponent<ZNetView>();
                zNetView.m_persistent = true;
            }
            if ((bool)zNetView)
            {
                bounty = new BountyInfoZNetProperty("bount_spawn", zNetView, defaultbounty);
                treasure = new TreasureMapChestInfoZNetProperty("treasure_spawn", zNetView, defaulttreasure);
                placed = new BoolZNetProperty("placed", zNetView, false);
                searching_for_spawn = new BoolZNetProperty("searching_for_spawn", zNetView, false);
                spawnpoint = new Vector3ZNetProperty("spawnpoint", zNetView, defaultspawn);
            }
            EpicLoot.Log("Adventure Spawner Awake");
        }

        public void Update()
        {
            if (zNetView.IsValid() != true) {
                return;
            }
            if (zNetView.IsOwner() != true)
            {
                // Only want these things to happen once.
                return;
            }
            if ((bool)zNetView == false)
            {
                return;
            }

            // We've got to skip at least some updates because slow object spawning means that we can spawn inside of things- if those things are not spawned already
            // Zonesystem.Instance.IsZoneLoaded? - instead?
            if (current_updates < 300)
            {
                current_updates += 1;
                return;
            }

            // We've waited a small period of time, things should be all spawned in, lets evaluate if our spawn point is still good.
            // EpicLoot.Log($"Checking if location search is happening. {current_updates}");
            if (started_placement == false) {
                EpicLoot.Log("Starting search for valid spawn location.");
                searching_for_spawn.Set(true);
                started_placement = true;
                if (bounty.Get().PlayerID > 0) {
                    StartCoroutine(DetermineSpawnPoint(bounty.Get().Position, bounty.Get().Biome));
                }
                if (treasure.Get().PlayerID > 0) {
                    StartCoroutine(DetermineSpawnPoint(treasure.Get().Position, treasure.Get().Biome, true));
                }
            }

            // Spawnpoint is still unset, we are waiting for the coroutine to finish
            // EpicLoot.Log("Checking is spawn point is set.");
            if (searching_for_spawn.Get() == true && spawnpoint.Get() == defaultspawn)
            {
                // EpicLoot.Log("Waiting for spawn point to be set.");
                return;
            }


            if (bounty.Get() != defaultbounty && placed.Get() == false)
            {
                EpicLoot.Log("Spawning bounty");
                SpawnBountyTargets(bounty.Get());
            }
            if (treasure.Get() != defaulttreasure && placed.Get() == false)
            {
                EpicLoot.Log("Spawning Treasure");
                SpawnChest(treasure.Get());
            }
            if (placed.Get())
            {
                EpicLoot.Log("Destroying AdventureSpawnController");
                ZNetScene.instance.Destroy(this.gameObject);
            }
        }

        public void SetBounty(BountyInfo bountyInfo)
        {
            // EpicLoot.Log("Setting BountyInfo");
            bounty.ForceSet(bountyInfo);
        }

        public void SetTreasure(TreasureMapChestInfo treasureInfo)
        {
            // EpicLoot.Log("Setting TreasureInfo");
            treasure.ForceSet(treasureInfo);
        }

        private static void SpawnBountyTargets(BountyInfo bounty)
        {
            Vector3 spawnPoint = spawnpoint.Get();
            var mainPrefab = ZNetScene.instance.GetPrefab(bounty.Target.MonsterID);
            if (mainPrefab == null)
            {
                EpicLoot.LogError($"Could not find prefab for bounty target! BountyID: " +
                    $"{bounty.ID}, MonsterID: {bounty.Target.MonsterID}");
                return;
            }

            var prefabs = new List<GameObject>() { mainPrefab };
            foreach (var addConfig in bounty.Adds)
            {
                for (var i = 0; i < addConfig.Count; i++)
                {
                    var prefab = ZNetScene.instance.GetPrefab(addConfig.MonsterID);
                    if (prefab == null)
                    {
                        EpicLoot.LogError($"Could not find prefab for bounty add! BountyID: " +
                            $"{bounty.ID}, MonsterID: {addConfig.MonsterID}");
                        return;
                    }
                    prefabs.Add(prefab);
                }
            }

            for (var index = 0; index < prefabs.Count; index++)
            {
                var prefab = prefabs[index];
                var isAdd = index > 0;

                var creature = UnityEngine.Object.Instantiate(prefab, spawnPoint, Quaternion.identity);
                var bountyTarget = creature.AddComponent<BountyTarget>();
                bountyTarget.Initialize(bounty, prefab.name, isAdd);
            }
            placed.ForceSet(true);
        }

        private static void SpawnChest(TreasureMapChestInfo treasure)
        {
            Vector3 spawnPoint = spawnpoint.Get();

            const string treasureChestPrefabName = "piece_chest_wood";
            var treasureChestPrefab = ZNetScene.instance.GetPrefab(treasureChestPrefabName);
            ZoneSystem.instance.GetGroundData(ref spawnPoint, out var normal, out var foundBiome, out var biomeArea, out var hmap);
            var treasureChestObject = UnityEngine.Object.Instantiate(treasureChestPrefab, spawnPoint, Quaternion.FromToRotation(Vector3.up, normal));
            var treasureChest = treasureChestObject.AddComponent<TreasureMapChest>();
            Piece tpiece = treasureChestObject.GetComponent<Piece>();
            // Prevent the wildlife from attacking the chest and giving away its location
            tpiece.m_primaryTarget = false;
            tpiece.m_randomTarget = false;
            tpiece.m_targetNonPlayerBuilt = false;
            treasureChest.Setup(treasure.PlayerID, treasure.Biome, treasure.Interval);
            placed.ForceSet(true);
        }


        internal static IEnumerator DetermineSpawnPoint(Vector3 startingSpawnPoint, Biome biome, bool do_not_spawn_in_water_override = false)
        {
            // Invert bit mask to check collisions
            // lmsk |= (1 << 0); // ignore default
            // lmsk |= (1 << 1); // ignore transparentFX
            // lmsk |= (1 << 2); // ignore raycast ignore
            // lmsk |= (1 << 9); // ignore characters
            LayerMask lmsk = LayerMask.GetMask("Default", "TransparentFX", "character");
            LayerMask terrain = LayerMask.GetMask("terrain");
            lmsk = ~lmsk; // Invert default bitshift to avoid colliding with masked layers, but still collide with everything else
            float range_increment = 2f;
            float current_max_x = startingSpawnPoint.x + range_increment;
            float current_min_x = startingSpawnPoint.x - range_increment;
            float current_max_z = startingSpawnPoint.z + range_increment;
            float current_min_z = startingSpawnPoint.z - range_increment;
            Vector3 determined_spawn = startingSpawnPoint;
            int spawn_location_attempts = 0;
            while (true) {
                if (spawnpoint.Get() != defaultspawn) {
                    // We've already found a spawn point, no need to continue
                    yield break;
                }
                if (spawn_location_attempts % 10 == 0 && spawn_location_attempts > 1) {
                    // Sleep to let other things still happen
                    yield return new WaitForSeconds(1f);
                }
                if (spawn_location_attempts > 0)
                {
                    EpicLoot.Log($"Finding new spawn location.");
                    // Choose a new spawn point since the last one didn't fit
                    determined_spawn = new Vector3(UnityEngine.Random.Range(current_min_x, current_max_x), 200, UnityEngine.Random.Range(current_min_z, current_max_z));
                }
                // For next attempt we go a little further out
                current_max_x += range_increment;
                current_min_x -= range_increment;
                current_max_z += range_increment;
                current_min_z -= range_increment;

                float height;
                if (ZoneSystem.instance.FindFloor(determined_spawn, out height))
                {
                    determined_spawn.y = height;
                }
                Physics.Raycast(determined_spawn + Vector3.up * 1f, Vector3.down, out var terrain_hit, 1000f, terrain);

                Physics.Raycast(determined_spawn + Vector3.up * 1f, Vector3.down, out var solid_hit, 1000f, lmsk);
                float terrain_diff = terrain_hit.point.y - determined_spawn.y;
                float solid_hit_diff = solid_hit.point.y - determined_spawn.y;

                ZoneSystem.instance.GetGroundData(ref determined_spawn, out var normal, out var foundBiome, out var biomeArea, out var hmap);
                EpicLoot.Log($"Spawn Point terrain: {terrain_hit.point.y} solid: {solid_hit.point.y} veg: {hmap.GetVegetationMask(determined_spawn)} biome: {biome}");
                // The treetop check, this is to prevent spawns in trees and in general off the ground
                if (Math.Abs(terrain_diff) > 1f)
                {
                    EpicLoot.Log($"Selected spawn height diff too high, retrying.");
                    spawn_location_attempts += 1;
                    continue;
                }

                // The solid check, this is to prevent spawns inside of rocks and other solid objects
                if (solid_hit.point.y > terrain_hit.point.y + 0.5f)
                {
                    EpicLoot.Log($"Selected spawn inside a solid, retrying.");
                    spawn_location_attempts += 1;
                    continue;
                }
                // Ideally the following checks should not be necessary, but they are here to prevent edge cases since we are moving the spawn slightly

                // Don't spawn in players bases
                // This seems to trigger heavily in the Ashlands. All spawned structures in the Ashlands are player structures? So many of the zones can't be spawned in.
                // Who doesn't want to go into a fortress to get your treasure chest?!
                //if ((bool)EffectArea.IsPointInsideArea(determined_spawn, EffectArea.Type.PlayerBase))
                //{
                //    EpicLoot.Log($"Selected spawn in a player zone, retrying.");
                //    spawn_location_attempts += 1;
                //    continue;
                //}
                // This is a Y check which prevents spawns in a body of water
                // Does not apply for ocean spawns
                if (biome != Heightmap.Biome.Ocean && determined_spawn.y < 27 || do_not_spawn_in_water_override && determined_spawn.y < 27)
                {
                    EpicLoot.Log($"Selected spawn under water but should not be, retrying.");
                    spawn_location_attempts += 1;
                    continue;
                }

                // Prevent spawning in Lava
                // This is a slightly modified lava check which is a little more strict and should give us more spacing away from the lavas edge
                if (biome == Heightmap.Biome.AshLands && spawn_location_attempts < 5 && hmap.GetVegetationMask(determined_spawn) > 0.45f)
                {
                    EpicLoot.Log($"Selected spawn is in lava, retrying.");
                    spawn_location_attempts += 1;
                    continue;
                }

                // Nothing was wrong with the selected spawn point, lets use it.
                break;
            }

            EpicLoot.Log($"Selected Spawn point X {determined_spawn.x}, Y {determined_spawn.y}, Z {determined_spawn.z}");
            spawnpoint.ForceSet(determined_spawn);
            yield break;
        }
    }
}
