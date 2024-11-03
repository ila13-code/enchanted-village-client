using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Unical.Demacs.EnchantedVillage
{
    [Serializable]
    public class GameInformation
    {
        [JsonProperty("level")]
        public int level { get; set; }

        [JsonProperty("experiencePoints")]
        public int experiencePoints { get; set; }

        [JsonProperty("elixir")]
        public int elixir { get; set; }

        [JsonProperty("gold")]
        public int gold { get; set; }

        [JsonProperty("buildings")]
        public List<BuildingData> buildings { get; set; }

        public GameInformation()
        {
            buildings = new List<BuildingData>();
        }

        public override string ToString()
        {
            return $"GameInformation[level={level}, exp={experiencePoints}, elixir={elixir}, gold={gold}, buildings={buildings?.Count ?? 0}]";
        }

        // Deep copy constructor
        public GameInformation Clone()
        {
            var clone = new GameInformation
            {
                level = this.level,
                experiencePoints = this.experiencePoints,
                elixir = this.elixir,
                gold = this.gold,
                buildings = new List<BuildingData>()
            };

            if (this.buildings != null)
            {
                foreach (var building in this.buildings)
                {
                    // Assumendo che BuildingData sia immutable o che non necessiti di deep copy
                    clone.buildings.Add(building);
                }
            }

            return clone;
        }

        // Helper methods for validation
        public bool IsValid()
        {
            return level >= 1 &&
                   experiencePoints >= 0 &&
                   elixir >= 0 &&
                   gold >= 0 &&
                   buildings != null;
        }

        public static GameInformation CreateDefault()
        {
            return new GameInformation
            {
                level = 1,
                experiencePoints = 0,
                elixir = 300,
                gold = 300,
                buildings = new List<BuildingData>()
            };
        }

        // Merge method to handle conflicts between local and server data
        public static GameInformation Merge(GameInformation local, GameInformation server)
        {
            if (server == null) return local;
            if (local == null) return server;

            var merged = new GameInformation
            {
                // Take the higher values for resources and progression
                level = Math.Max(local.level, server.level),
                experiencePoints = Math.Max(local.experiencePoints, server.experiencePoints),
                elixir = Math.Max(local.elixir, server.elixir),
                gold = Math.Max(local.gold, server.gold),
                buildings = new List<BuildingData>()
            };

            // Create a dictionary to track buildings by ID
            var buildingMap = new Dictionary<string, BuildingData>();

            // Add local buildings
            if (local.buildings != null)
            {
                foreach (var building in local.buildings)
                {
                    buildingMap[building.GetUniqueId()] = building;
                }
            }

            // Merge with server buildings, preferring server data in case of conflict
            if (server.buildings != null)
            {
                foreach (var serverBuilding in server.buildings)
                {
                    var buildingId = serverBuilding.GetUniqueId();
                    if (buildingMap.TryGetValue(buildingId, out var localBuilding))
                    {
                        // Special handling for training bases (type 4)
                        if (serverBuilding.getPrefabIndex() == 4 && localBuilding.getPrefabIndex() == 4)
                        {
                            // Keep the version with more troops
                            if ((serverBuilding.getTroopsData()?.Count ?? 0) > (localBuilding.getTroopsData()?.Count ?? 0))
                            {
                                buildingMap[buildingId] = serverBuilding;
                            }
                        }
                        else
                        {
                            // For other buildings, prefer server version
                            buildingMap[buildingId] = serverBuilding;
                        }
                    }
                    else
                    {
                        // Add new buildings from server
                        buildingMap[buildingId] = serverBuilding;
                    }
                }
            }

            // Convert final map back to list
            merged.buildings.AddRange(buildingMap.Values);

            return merged;
        }

        // Equality comparison
        public override bool Equals(object obj)
        {
            if (obj is not GameInformation other)
                return false;

            return this.level == other.level &&
                   this.experiencePoints == other.experiencePoints &&
                   this.elixir == other.elixir &&
                   this.gold == other.gold &&
                   CompareBuildingLists(this.buildings, other.buildings);
        }

        private bool CompareBuildingLists(List<BuildingData> list1, List<BuildingData> list2)
        {
            if (list1 == null && list2 == null) return true;
            if (list1 == null || list2 == null) return false;
            if (list1.Count != list2.Count) return false;

            var dict1 = new Dictionary<string, BuildingData>();
            foreach (var building in list1)
            {
                dict1[building.GetUniqueId()] = building;
            }

            foreach (var building in list2)
            {
                if (!dict1.ContainsKey(building.GetUniqueId()))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(level);
            hash.Add(experiencePoints);
            hash.Add(elixir);
            hash.Add(gold);
            if (buildings != null)
            {
                foreach (var building in buildings)
                {
                    hash.Add(building.GetUniqueId());
                }
            }
            return hash.ToHashCode();
        }
    }
}