using System.Collections;
using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class CashDialog : MonoBehaviour
    {
        public bool UserConfirmed { get; private set; }
        private bool inputReceived = false;

        public IEnumerator WaitForUserInput()
        {
            Debug.Log("In attesa dell'input utente...");

            // Aspetta finché l'utente non fa una scelta
            while (!inputReceived)
            {
                yield return null;  // Aspetta il frame successivo
            }

            Debug.Log("Input utente ricevuto: " + UserConfirmed);
        }

        public void OnConfirm()
        {
            Debug.Log("OnConfirm chiamato");
            UserConfirmed = true;
            inputReceived = true;
        }

        public void OnCancel()
        {
            Debug.Log("OnCancel chiamato");
            UserConfirmed = false;
            inputReceived = true;
        }

        public void ResetState()
        {
            UserConfirmed = false;
            inputReceived = false;
        }
    }
}

