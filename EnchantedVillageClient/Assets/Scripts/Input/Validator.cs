using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

namespace Unical.Demacs.EnchantedVillage
{
    public class Validator : MonoBehaviour
    {
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private Image errorIcon;

        private const string EMAIL_PATTERN = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        private void Start()
        {
            errorIcon.gameObject.SetActive(false);
            emailInput.onValueChanged.AddListener(OnEmailChanged);
        }

        private void OnEmailChanged(string email)
        {
            errorIcon.gameObject.SetActive(!ValidateEmail(email));
        }

        public bool ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;
            return Regex.IsMatch(email, EMAIL_PATTERN);
        }
    }
}