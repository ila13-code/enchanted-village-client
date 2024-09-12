using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class TrainingDialog : MonoBehaviour
    {
        public static TrainingDialog Instance { get; private set; }
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private GameObject UIPanel;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void ShowDialog()
        {
            if (UIPanel != null) UIPanel.SetActive(false);
            if (dialogPanel != null) dialogPanel.SetActive(true);
        }

        public void HideDialog()
        {
            if (dialogPanel != null) dialogPanel.SetActive(false);
        }
    }
}