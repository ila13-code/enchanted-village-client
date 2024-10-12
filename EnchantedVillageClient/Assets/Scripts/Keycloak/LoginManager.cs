using System.Collections;
using System.Collections.Generic;
using Unical.Demacs.EnchantedVillage;
using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class LoginManager : MonoBehaviour
    {
        private KeycloakService keycloakService;

        void Start()
        {
            keycloakService = FindObjectOfType<KeycloakService>();

            if (keycloakService == null)
            {
                Debug.LogError("KeycloakService non trovato nella scena!");
                return;
            }

        }

        public void Login()
        {
            keycloakService.Login();
        }

    }
}