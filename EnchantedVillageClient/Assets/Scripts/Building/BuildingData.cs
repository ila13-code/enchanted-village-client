using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
namespace Unical.Demacs.EnchantedVillage
{
    //classe che gestisce i dati degli edifici e permette di serializzarli in JSON 
    [System.Serializable]
    public class BuildingData
    {
        // Attributi esistenti
        [JsonProperty("_uniqueId")]
        private string _uniqueId;
        [JsonProperty("_prefabIndex")]
        private int _prefabIndex;
        [JsonProperty("_x")]
        private int _x;
        [JsonProperty("_y")]
        private int _y;
        [JsonProperty("_troopsData")]
        private List<TroopsData> _troopsData;

        // Costruttore esistente
        public BuildingData(string uniqueId, int prefabIndex, int x, int y)
        {
            _uniqueId = uniqueId;
            _prefabIndex = prefabIndex;
            _x = x;
            _y = y;
            _troopsData = new List<TroopsData>();
        }

        // Override di Equals e GetHashCode per confrontare correttamente gli edifici
        public override bool Equals(object obj)
        {
            if (obj is BuildingData other)
            {
                return _uniqueId == other._uniqueId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _uniqueId.GetHashCode();
        }

        // Metodi getter/setter modificati
        public List<TroopsData> getTroopsData()
        {
            return _troopsData ?? new List<TroopsData>();
        }

        public void setTroopsData(List<TroopsData> troops)
        {
            _troopsData = troops ?? new List<TroopsData>();
        }

        // Metodo per clonare l'edificio con le sue truppe
        public BuildingData Clone()
        {
            var clone = new BuildingData(_uniqueId, _prefabIndex, _x, _y);
            if (_troopsData != null)
            {
                clone._troopsData = _troopsData.Select(t => t.Clone()).ToList();
            }
            return clone;
        }
    
    public string GetUniqueId()
        {
            return _uniqueId;
        }
        public int getPrefabIndex()
        {
            return _prefabIndex;
        }
        public int getX()
        {
            return _x;
        }

        public int getY()
        {
            return _y;
        }


        public int getTroopsCount()
        {
            return _troopsData.Count;
        }



        public void setPrefabIndex(int prefabIndex)
        {
            _prefabIndex = prefabIndex;
        }

        public void setX(int x)
        {
            _x = x;
        }

        public void setY(int y)
        {
            _y = y;
        }
    }
}
