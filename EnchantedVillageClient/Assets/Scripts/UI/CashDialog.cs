using UnityEngine;
using System.Threading.Tasks;

namespace Unical.Demacs.EnchantedVillage
{
    public class CashDialog : MonoBehaviour
    {
        private TaskCompletionSource<bool> tcs;

        public void Show()
        {
            gameObject.SetActive(true);
            tcs = new TaskCompletionSource<bool>();
        }

        public Task<bool> WaitForUserInput()
        {
            return tcs.Task;
        }

        public void OnConfirm()
        {
            Debug.Log("OnConfirm chiamato");
            tcs.TrySetResult(true);
            gameObject.SetActive(false);
        }

        public void OnCancel()
        {
            Debug.Log("OnCancel chiamato");
            tcs.TrySetResult(false);
            gameObject.SetActive(false);
        }
    }
}