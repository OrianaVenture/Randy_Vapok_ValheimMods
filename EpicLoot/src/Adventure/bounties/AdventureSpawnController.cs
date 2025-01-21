using EpicLoot.Adventure;
using System;
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

        public void Awake()
        {
            if (this.gameObject.TryGetComponent<ZNetView>(out zNetView) == false)
            {
                this.gameObject.AddComponent<ZNetView>();
                zNetView = this.gameObject.GetComponent<ZNetView>();
                zNetView.m_persistent = true;
                // EpicLoot.Log("AdventureControllerSpawner missing znetview");
            }
            if ((bool)zNetView)
            {
                bounty = new BountyInfoZNetProperty("bount_spawn", zNetView, new BountyInfo());
                treasure = new TreasureMapChestInfoZNetProperty("treasure_spawn", zNetView, new TreasureMapChestInfo());
                // EpicLoot.Log("Adventure Spawner Initialized Zvalues");
            }
            // EpicLoot.Log("Adventure Spawner Awake");
        }

        public void Update()
        {
            if (zNetView.IsValid() != true) {
                // EpicLoot.Log("ZNetView is not valid");
                return;
            }

            //EpicLoot.Log($"ASC Init triggered. bounty? {(bounty.Get().PlayerID > 0)} treasure? {(treasure.Get().PlayerID > 0)}");
            if (bounty.Get().PlayerID > 0)
            {
                //EpicLoot.Log("Spawning bounty");
                SpawnBountyTargets(bounty.Get());
                //EpicLoot.Log("Destroying spawn controller");
                ZNetScene.instance.Destroy(this.gameObject);
                return;
            }
            if (treasure.Get().PlayerID > 0)
            {
                //EpicLoot.Log("Spawning Treasure");
                SpawnChest(treasure.Get());
                //EpicLoot.Log("Destroying spawn controller");
                ZNetScene.instance.Destroy(this.gameObject);
                return;
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
            Vector3 spawnPoint = bounty.Position;
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

            // Determine actual spawn point, and make sure it's on the ground
            // This is modified from the original target because now things like rocks and trees have spawned in
            spawnPoint = DetermineSpawnPoint(spawnPoint, bounty.Biome);

            for (var index = 0; index < prefabs.Count; index++)
            {
                var prefab = prefabs[index];
                var isAdd = index > 0;

                var creature = UnityEngine.Object.Instantiate(prefab, spawnPoint, Quaternion.identity);
                var bountyTarget = creature.AddComponent<BountyTarget>();
                bountyTarget.Initialize(bounty, prefab.name, isAdd);
            }
        }

        private static void SpawnChest(TreasureMapChestInfo treasure)
        {
            Vector3 spawnPoint = DetermineSpawnPoint(treasure.Position, treasure.Biome, true);

            const string treasureChestPrefabName = "piece_chest_wood";
            var treasureChestPrefab = ZNetScene.instance.GetPrefab(treasureChestPrefabName);
            ZoneSystem.instance.GetGroundData(ref spawnPoint, out var normal, out var foundBiome, out var biomeArea, out var hmap);
            var treasureChestObject = UnityEngine.Object.Instantiate(treasureChestPrefab, spawnPoint, Quaternion.FromToRotation(Vector3.up, normal));
            var treasureChest = treasureChestObject.AddComponent<TreasureMapChest>();
            treasureChest.Setup(treasure.PlayerID, treasure.Biome, treasure.Interval);
        }

        internal static Vector3 DetermineSpawnPoint(Vector3 startingSpawnPoint, Biome biome, bool do_not_spawn_in_water_override = false)
        {
            LayerMask terrain_lmsk = LayerMask.GetMask("terrain");
            float range_increment = 2f;
            float current_max_x = startingSpawnPoint.x + range_increment;
            float current_min_x = startingSpawnPoint.x - range_increment;
            float current_max_z = startingSpawnPoint.z + range_increment;
            float current_min_z = startingSpawnPoint.z - range_increment;
            Vector3 determined_spawn = startingSpawnPoint;
            int spawn_location_attempts = 0;
            while (spawn_location_attempts < 10)
            {
                if (spawn_location_attempts > 0)
                {
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
                Physics.Raycast(determined_spawn + Vector3.up * 1f, Vector3.down, out var terrain_hit, 1000f, terrain_lmsk);
                float terrain_diff = terrain_hit.point.y - determined_spawn.y;
                // The treetop check, this is to prevent spawns in trees
                if (Math.Abs(terrain_diff) > 1f)
                {
                    spawn_location_attempts += 1;
                    continue;
                }
                // Ideally the following checks should not be necessary, but they are here to prevent edge cases since we are moving the spawn slightly

                // Don't spawn in players bases
                if ((bool)EffectArea.IsPointInsideArea(determined_spawn, EffectArea.Type.PlayerBase))
                {
                    spawn_location_attempts += 1;
                    continue;
                }
                // This is a Y check which prevents spawns in a body of water
                // Does not apply for ocean spawns
                if (biome != Heightmap.Biome.Ocean && determined_spawn.y < 27)
                {
                    spawn_location_attempts += 1;
                    continue;
                }
                // Prevent spawns at water levels if this should not spawn in water
                if (do_not_spawn_in_water_override && determined_spawn.y < 27)
                {
                    spawn_location_attempts += 1;
                    continue;
                }

                // Nothing was wrong with the selected spawn point, lets use it.
                break;
            }

            return determined_spawn;
        }
    }
}
