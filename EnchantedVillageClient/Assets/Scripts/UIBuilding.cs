using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Unical.Demacs.EnchantedVillage
{
    public class UIBuilding : MonoBehaviour
    {
        [SerializeField]private int _prefabIndex = 0;
        private Transform container;

        private void Awake()
        {
            GameObject map = GameObject.Find("Map"); 
            if (map != null)
            {
                container = map.transform.Find("Buildings").transform; 
            }
            else
            {
                Debug.LogError("Map non trovato nella scena.");
            }
        }

        public void PlaceBuilding()
        {
            Vector3 position =Vector3.zero;
            Building building = Instantiate(UIController.Instance.Buildings[_prefabIndex], position, Quaternion.identity, container);
        }

        public int getPrefabIndex()
        {
            return _prefabIndex;
        }

        public void setPrefabIndex(int prefabIndex)
        {
            _prefabIndex = prefabIndex;
        }
    }
}
