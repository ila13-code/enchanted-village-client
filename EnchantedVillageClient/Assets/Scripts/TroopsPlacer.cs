using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unical.Demacs.EnchantedVillage;
using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class TroopsPlacer : MonoBehaviour
    {
        private TroopsPlacer instance; 
        [SerializeField] private Troops[] troops;
        private BuildGrid buildGrid;
        [SerializeField] private Transform troopsContainer;

        public void Start()
        {
            buildGrid = FindObjectOfType<BuildGrid>();
        }

        public void PlaceArcher()
        {
            PlaceTroops(0);
        }

        public void PlaceSwordMan()
        {
            PlaceTroops(1);
        }

        public void PlaceViking()
        {
            PlaceTroops(2);
        }

        private void PlaceTroops(int troopsType)
        {
            List<BuildingData> trainingBases = GetTrainingBases();
            if (trainingBases.Count == 0)
            {
                Debug.LogError("Nessun campo di addestramento trovato");
                return;
            }
            for(int k = 0; k< trainingBases.Count; k++ )
            {
                BuildingData trainingBase = trainingBases[k];
                Debug.Log($"Campo di addestramento {k}: {trainingBase.GetUniqueId()}");
                string buildingId = trainingBase.GetUniqueId();

                List<TroopsData> troopsData = trainingBase.getTroopsData();
                if (troopsData == null)
                {
                    Debug.LogError($"Lista di truppe null per l'edificio {buildingId}");
                    continue;
                }

                if (troopsData.Count >= 5)
                {
                    Debug.LogError($"Numero massimo di truppe raggiunto per l'edificio {buildingId}");
                    continue;
                }

                // Inserimento della nuova truppa
                TroopsData newTroop = new TroopsData(0, 0, 0, troopsType);
                troopsData.Add(newTroop);
                trainingBase.setTroopsData(troopsData);

                Vector3 spawnPosition = new Vector3(trainingBase.getX(), 1f, trainingBase.getY());
                Troops troopInstance = Instantiate(troops[troopsType], spawnPosition, Quaternion.identity, troopsContainer);
                Debug.Log($"Truppa inserita nell'edificio {buildingId}. Totale truppe: {troopsData.Count}");
                troopInstance.PlaceOnGrid(trainingBase.getX(), trainingBase.getY(), troopsData.Count);

                // Aggiorna l'edificio nella lista completa degli edifici
                List<BuildingData> allBuildings = PlayerPrefsController.Instance.GetBuildings();
                int index = allBuildings.FindIndex(b => b.GetUniqueId() == buildingId);
                if (index != -1)
                {
                    allBuildings[index] = trainingBase;
                    PlayerPrefsController.Instance.SaveBuildings(allBuildings);
                    break;
                }
                else
                {
                    Debug.LogError($"Impossibile trovare l'edificio con ID {buildingId} nella lista completa degli edifici");
                }
            }
           
            
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
