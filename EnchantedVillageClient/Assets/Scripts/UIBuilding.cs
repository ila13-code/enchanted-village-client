using System.Collections;
using System.Collections.Generic;
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

        public void PlaceBuilding()
        {
            // Se c'è una coroutine in corso, fermala
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }

            // Avvia una nuova coroutine
            currentCoroutine = StartCoroutine(PlaceBuildingCoroutine());
        }

        private IEnumerator PlaceBuildingCoroutine()
        {
            bool confirmed = false;

            // Log per verificare l'inizio della coroutine
            Debug.Log("Inizio PlaceBuildingCoroutine");

            yield return StartCoroutine(ShowCashDialogCoroutine(result =>
            {
                confirmed = result;
                Debug.Log("Risultato dialogo: " + confirmed);
            }));

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

            currentCoroutine = null;
        }

        private IEnumerator ShowCashDialogCoroutine(System.Action<bool> onComplete)
        {
            // Trova e attiva il dialogo
            GameObject dialogInstance = UIController.Instance._dialogs.transform.Find("CashDialog").gameObject;
            dialogInstance.SetActive(true);

            // Ottieni il componente CashDialog e resetta lo stato
            CashDialog dialog = dialogInstance.GetComponent<CashDialog>();
            dialog.ResetState();

            Debug.Log("Dialogo attivato");

            // Aspetta l'input dell'utente
            yield return StartCoroutine(dialog.WaitForUserInput());

            Debug.Log("Completa ShowCashDialogCoroutine con: " + dialog.UserConfirmed);

            // Passa il risultato al callback
            onComplete(dialog.UserConfirmed);

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
