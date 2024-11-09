using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace Unical.Demacs.EnchantedVillage
{
    public class CashDialog : MonoBehaviour
    {
        private static TaskCompletionSource<bool> tcs;

        public void YesClicked()
        {
            Debug.Log("Yes clicked");
            if (tcs != null)
            {
                tcs.SetResult(true);
            }
            gameObject.SetActive(false);
        }
        public void NoClicked()
        {
            Debug.Log("No clicked");
            if (tcs != null)
            {
                tcs.SetResult(false);
            }
            gameObject.SetActive(false);
        }


        public async Task<bool> ShowAndWait()
        {
            tcs = new TaskCompletionSource<bool>();
            gameObject.SetActive(true);
            return await tcs.Task;
        }
    }
}