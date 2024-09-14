using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unical.Demacs.EnchantedVillage;
using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class TroopsPlacer : MonoBehaviour
    {
        [SerializeField] private GameObject[] troopsPrefabs;

        public void Start()
        {
            
        }

        public void PlaceTroops(int troopsType)
        {
            List<BuildingData> trainingBases = GetTrainingBases();
            if (trainingBases.Count == 0)
            {
                Debug.LogError("Nessun campo di addestramento trovato");
                return;
            }

            BuildingData trainingBase = trainingBases[0];
            List<TroopsData> troopsData = trainingBase.getTroopsData();
            if (troopsData == null)
            {
                Debug.LogError("Lista di truppe null");
                return;
            }

            if(trainingBase.getTroopsCount() >= 5)
            {
                Debug.LogError("Numero massimo di truppe raggiunto");
                return;
            }

            //altrimenti posso inserire la truppa
            TroopsData newTroop = new TroopsData(0, 0, 0, troopsType);
            troopsData.Add(newTroop);
            trainingBase.setTroopsData(troopsData);
            Vector3 spawnPosition = new Vector3(trainingBase.getX(), 0, trainingBase.getY());
            GameObject troopInstance = Instantiate(troopsPrefabs[troopsType], spawnPosition, Quaternion.identity);
            PlayerPrefsController.Instance.SaveBuildings(PlayerPrefsController.Instance.GetBuildings());
        }


        //recupera tutti i campi di addestramento in cui posso inserire truppe
        private List<BuildingData> GetTrainingBases()
        {
            int index = 4;
            if (PlayerPrefsController.Instance == null)
            {
                Debug.LogError("PlayerPrefsController.Instance è null");
                return new List<BuildingData>();
            }

            List<BuildingData> allBuildings = PlayerPrefsController.Instance.GetBuildings();
            if (allBuildings == null)
            {
                Debug.LogError("La lista di BuildingData è null");
                return new List<BuildingData>();
            }

            List<BuildingData> filteredBuildings = allBuildings.Where(b => b.getPrefabIndex() == index).ToList();

            Debug.Log($"Trovati {filteredBuildings.Count} edifici con index {index}");

            return filteredBuildings;
        }
    }
}
