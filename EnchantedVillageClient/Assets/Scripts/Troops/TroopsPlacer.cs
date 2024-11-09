using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Unical.Demacs.EnchantedVillage
{
    public class TroopsPlacer : MonoBehaviour
    {
        private static TroopsPlacer instance;
        [SerializeField] private Troops[] troops;
        private BuildGrid buildGrid;

        public static TroopsPlacer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<TroopsPlacer>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("TroopsPlacer");
                        instance = go.AddComponent<TroopsPlacer>();
                    }
                }
                return instance;
            }
        }


        public void Start()
        {
            buildGrid = FindObjectOfType<BuildGrid>();
        }

        public void PlaceArcher() => PlaceTroops(0);
        public void PlaceSwordMan() => PlaceTroops(1);
        public void PlaceViking() => PlaceTroops(2);

        private void PlaceTroops(int troopsType)
        {
            List<BuildingData> trainingBases = GetTrainingBases();
            if (trainingBases.Count == 0)
            {
                Debug.LogError("Nessun campo di addestramento trovato");
                return;
            }

            List<BuildingData> updatedBuildings = new List<BuildingData>();

            for (int k = 0; k < trainingBases.Count; k++)
            {
                BuildingData trainingBase = trainingBases[k];
                Debug.Log($"Campo di addestramento {k}: {trainingBase.GetUniqueId()}");
                string buildingId = trainingBase.GetUniqueId();

                List<TroopsData> troopsData = trainingBase.getTroopsData();
                if (troopsData == null)
                {
                    troopsData = new List<TroopsData>();
                    trainingBase.setTroopsData(troopsData);
                }

                if (troopsData.Count >= 5)
                {
                    Debug.Log($"Numero massimo di truppe raggiunto per l'edificio {buildingId}");
                    continue;
                }

                PlayerPrefsController.Instance.Elixir -= getCost(troopsType);

                // Troviamo l'edificio nel mondo
                Building building = FindBuildingInWorld(buildingId);
                if (building == null)
                {
                    Debug.LogError($"Impossibile trovare l'edificio {buildingId} nel mondo");
                    continue;
                }

                // Creiamo la truppa come figlia diretta dell'edificio
                Vector3 spawnPosition = new Vector3(trainingBase.getX(), 1f, trainingBase.getY());
                Troops troopInstance = Instantiate(troops[troopsType], spawnPosition, Quaternion.identity, building.transform);

                // Usiamo PlaceOnGrid per posizionare la truppa
                troopInstance.PlaceOnGrid(trainingBase.getX(), trainingBase.getY(), troopsData.Count + 1);

                // Aspettiamo un frame per assicurarci che PlaceOnGrid abbia terminato
                StartCoroutine(SaveTroopPosition(troopInstance, troopsData, troopsType, trainingBase, updatedBuildings));
                break;
            }
        }

        private Building FindBuildingInWorld(string buildingId)
        {
            Building[] allBuildings = FindObjectsOfType<Building>();
            return allBuildings.FirstOrDefault(b => b.Id == buildingId);
        }

        private IEnumerator SaveTroopPosition(Troops troopInstance, List<TroopsData> troopsData, int troopsType,
            BuildingData trainingBase, List<BuildingData> updatedBuildings)
        {
            yield return new WaitForEndOfFrame();

            // Salviamo la posizione finale effettiva dopo PlaceOnGrid
            TroopsData newTroop = new TroopsData(
                troopInstance.transform.localPosition.x,
                troopInstance.transform.localPosition.y,
                troopInstance.transform.localPosition.z,
                troopsType
            );

            troopsData.Add(newTroop);
            trainingBase.setTroopsData(troopsData);

            Debug.Log($"Truppa inserita nell'edificio {trainingBase.GetUniqueId()} alla posizione finale {troopInstance.transform.position}");
            updatedBuildings.Add(trainingBase);

            // Aggiorno gli edifici nella lista completa e salvo
            List<BuildingData> allBuildings = PlayerPrefsController.Instance.GetBuildings();
            int index = allBuildings.FindIndex(b => b.GetUniqueId() == trainingBase.GetUniqueId());
            if (index != -1)
            {
                allBuildings[index] = trainingBase;
                PlayerPrefsController.Instance.SaveBuildings(allBuildings);
            }
            else
            {
                Debug.LogError($"Impossibile trovare l'edificio con ID {trainingBase.GetUniqueId()}");
            }
        }

        public void LoadTroopsForBuilding(BuildingData buildingData, Building building)
        {
            List<TroopsData> troopsData = buildingData.getTroopsData();
            if (troopsData == null || troopsData.Count == 0) return;

            foreach (var troopData in troopsData)
            {
                // Creiamo la truppa come figlia diretta dell'edificio
                Vector3 savedPosition = new Vector3(troopData.getX(), troopData.getY(), troopData.getZ());
                Instantiate(troops[troopData.getType()], savedPosition, Quaternion.identity, building.transform);
            }
        }

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

        private int getCost(int type)
        {
            switch (type)
            {
                case 0: return 200;
                case 1: return 100;
                case 2: return 150;
                default: return 0;
            }
        }
    }
}