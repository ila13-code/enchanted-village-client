using UnityEngine;
using System.Threading.Tasks;

namespace Unical.Demacs.EnchantedVillage
{
    public class UIBuilding : MonoBehaviour
    {
        [SerializeField] private int _prefabIndex = 0;
        private Transform buildingsContainer;

        private void Awake()
        {
            GameObject map = GameObject.Find("Map");
            if (map != null)
            {
                buildingsContainer = map.transform.Find("Buildings").transform;
            }
        }

        public async void PlaceBuilding()
        {
            Debug.Log("Inizio PlaceBuilding");

            if (PlayerPrefsController.Instance.Gold < ShopItem.GetCostFromIndex(_prefabIndex))
            {
                Debug.Log("Risorse insufficienti");
                await ShowErrorDialog();
                return;
            }

            var cashDialog = UIController.Instance._dialogs.transform
                .Find("CashDialog").GetComponent<CashDialog>();

            bool confirmed = await cashDialog.ShowAndWait();

            if (confirmed)
            {
                Debug.Log("Confermato, piazzo edificio");
                PlayerPrefsController.Instance.Gold -= ShopItem.GetCostFromIndex(_prefabIndex);
                PlayerPrefsController.Instance.Exp += ShopItem.GetExperiencePoints(_prefabIndex);
                Vector3 position = Vector3.zero;
                Building building = Instantiate(UIController.Instance.Buildings[_prefabIndex],
                    position, Quaternion.identity, buildingsContainer);
                building.Id = System.Guid.NewGuid().ToString();
                building.UpdateGridPosition((int)position.x, (int)position.y);
            }
            else
            {
                Debug.Log("Annullato");
            }
        }

        private async Task<bool> ShowErrorDialog()
        {
            var dialog = UIController.Instance._dialogs.transform
                .Find("ErrorDialog").GetComponent<ErrorDialog>();
            dialog.Show();
            await Task.Delay(2000);
            dialog.Hide();
            return true;
        }

        public int getPrefabIndex() => _prefabIndex;
        public void setPrefabIndex(int prefabIndex) => _prefabIndex = prefabIndex;
    }
}