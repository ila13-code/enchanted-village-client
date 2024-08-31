using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace Unical.Demacs.EnchantedVillage
{
    public class ErrorDialog : MonoBehaviour
    {
        private TaskCompletionSource<bool> tcs;

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        public void Show()
        {
            gameObject.SetActive(true);
            tcs = new TaskCompletionSource<bool>();
        }
    }
}
