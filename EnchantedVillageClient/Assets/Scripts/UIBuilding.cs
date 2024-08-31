using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
namespace Unical.Demacs.EnchantedVillage
{
    public class UIBuilding : MonoBehaviour
    {
        [SerializeField]private int _prefabIndex = 0;
        
        [SerializeField] private GameObject CashDialogPrefab;
        private Transform buildingsContainer;
        private Coroutine currentCoroutine;

        private void Awake()
        {
            GameObject map = GameObject.Find("Map");
            if (map != null)
            {
                buildingsContainer = map.transform.Find("Buildings").transform;
            }
            else
            {
                Debug.LogError("Map non trovato nella scena.");
            }
        }

       

        private async void PlaceBuilding()
        {
            Debug.Log("Inizio PlaceBuilding");
            bool confirmed = await ShowCashDialog();

            if (confirmed)
            {
                Debug.Log("Conferma ricevuta, piazzamento edificio.");
                Vector3 position = Vector3.zero;
                Building building = Instantiate(UIController.Instance.Buildings[_prefabIndex], position, Quaternion.identity, buildingsContainer);
            }
            else
            {
                Debug.Log("L'utente ha annullato il posizionamento dell'edificio.");
            }
        }

        private async Task<bool> ShowCashDialog()
        {
            GameObject dialogInstance = UIController.Instance._dialogs.transform.Find("CashDialog").gameObject;
            CashDialog dialog = dialogInstance.GetComponent<CashDialog>();
            dialog.Show();

            Debug.Log("Dialogo attivato");

            bool result = await dialog.WaitForUserInput();

            Debug.Log("Completa ShowCashDialog con: " + result);
            return result;
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
