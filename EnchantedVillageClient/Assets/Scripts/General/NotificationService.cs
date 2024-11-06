using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class NotificationService : MonoBehaviour
    {
        private static NotificationService instance;

        [SerializeField] private ErrorDialog errorDialog;
        [SerializeField] private TextMeshProUGUI messageText;

        public static NotificationService Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("NotificationService");
                    instance = go.AddComponent<NotificationService>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void ShowNotification(string message)
        {
            messageText.text = message;
            errorDialog.Show();
        }
    }
}
