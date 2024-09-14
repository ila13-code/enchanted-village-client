using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unical.Demacs.EnchantedVillage;
using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class TroopsPlacer : MonoBehaviour
    {
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
            //Vector3 spawnPosition = GetWorldPositionFromGrid(trainingBase.getX(), trainingBase.getY());
                //new Vector3(trainingBase.getX(), 0, trainingBase.getY());
            Troops troopInstance = Instantiate(troops[troopsType], spawnPosition, Quaternion.identity, troopsContainer);
            troopInstance.PlaceOnGrid(trainingBase.getX(), trainingBase.getY());
            PlayerPrefsController.Instance.SaveBuildings(PlayerPrefsController.Instance.GetBuildings());
        }


        private Vector3 GetWorldPositionFromGrid(int gridX, int gridY)
        {
            // Passo 1: Ottieni la posizione locale della cella nella griglia
            float localX = (gridX * buildGrid.CellSize) + (buildGrid.CellSize / 2f);
            float localZ = (gridY * buildGrid.CellSize) + (buildGrid.CellSize / 2f);

            // Crea un nuovo Vector3 per la posizione locale, imponendo che la Y rimanga costante
            Vector3 localGridPosition = new Vector3(localX, -0.1f, localZ);

            // Passo 2: Converti la posizione locale nel sistema di coordinate del mondo, tenendo conto della rotazione e traslazione della griglia
            Vector3 worldPosition = buildGrid.transform.TransformPoint(localGridPosition);

            // Passo 3: Compensare la rotazione dell'edificio (-45 gradi sull'asse Y)
            Quaternion buildingRotation = Quaternion.Euler(0, -45, 0);  // Rotazione inversa dell'edificio
            worldPosition = buildingRotation * (worldPosition - buildGrid.transform.position) + buildGrid.transform.position;

            return worldPosition;
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
