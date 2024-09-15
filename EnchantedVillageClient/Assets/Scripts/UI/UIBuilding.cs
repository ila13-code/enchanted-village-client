using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static Unical.Demacs.EnchantedVillage.Building;
namespace Unical.Demacs.EnchantedVillage
{
    public class UIBuilding : MonoBehaviour
    {
        [SerializeField]private int _prefabIndex = 0;
        
        private Transform buildingsContainer;

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

        public async void PlaceBuilding()
        {
            Debug.Log("Inizio PlaceBuilding");
            if (PlayerPrefsController.Instance.Gold < ShopItem.GetCostFromIndex(_prefabIndex))
            {
                Debug.Log("L'utente non ha abbastanza risorse per acquistare l'edificio");
                await ShowErrorDialog();
                return;
            }
            else
            {
                bool confirmed = await ShowCashDialog();

                if (confirmed)
                {
                    Debug.Log("Conferma ricevuta, piazzamento edificio.");
                    PlayerPrefsController.Instance.Gold -= ShopItem.GetCostFromIndex(_prefabIndex);
                    PlayerPrefsController.Instance.Exp += ShopItem.GetExperiencePoints(_prefabIndex);
                    Vector3 position = Vector3.zero;
                    Building building = Instantiate(UIController.Instance.Buildings[_prefabIndex], position, Quaternion.identity, buildingsContainer);
                    building.Id= Guid.NewGuid().ToString();
                    building.UpdateGridPosition((int)position.x, (int)position.y);
                }
                else
                {
                    Debug.Log("L'utente ha annullato il posizionamento dell'edificio.");
                }
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

        private async Task<bool> ShowErrorDialog()
        {
            GameObject dialogInstance = UIController.Instance._dialogs.transform.Find("ErrorDialog").gameObject;
            ErrorDialog dialog = dialogInstance.GetComponent<ErrorDialog>();
            dialog.Show();
            Debug.Log("Error Dialog attivato");

            await Task.Delay(2000);

            dialog.Hide();

            Debug.Log("Completa ShowErrorDialog dopo 2 secondi");
            return true; 
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
