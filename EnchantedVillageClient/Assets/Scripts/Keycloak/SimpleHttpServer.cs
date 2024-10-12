using System;
using System.Net;
using Unical.Demacs.EnchantedVillage;
using UnityEngine;

public class SimpleHttpServer : MonoBehaviour
{
    private HttpListener listener;

    private void Start()
    {
        listener = new HttpListener();
        // Usa un endpoint separato per il tuo server di callback
        listener.Prefixes.Add("http://localhost:8081/login-callback/");
        listener.Start();
        Debug.Log("Listening...");
        StartListening();
    }

    private async void StartListening()
    {
        while (true)
        {
            var context = await listener.GetContextAsync();
            var requestUrl = context.Request.Url.ToString();
            Debug.Log($"Received request: {requestUrl}");

            // Verifica se l'URL contiene i parametri "code" e "state"
            if (ContainsAuthorizationParameters(requestUrl))
            {
                // Gestisci la risposta da Keycloak
                KeycloakService keycloakService = FindObjectOfType<KeycloakService>();
                keycloakService.HandleAuthorizationResponse(requestUrl);

                // Invia una risposta al browser
                string responseString = "Login successful! You can close this window.";
                var response = context.Response;
                response.ContentLength64 = System.Text.Encoding.UTF8.GetByteCount(responseString);
                var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
            else
            {
                Debug.Log("Waiting for authorization code and state in the URL...");
            }
        }
    }

    private bool ContainsAuthorizationParameters(string url)
    {
        Uri uri = new Uri(url);
        var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
        return !string.IsNullOrEmpty(queryParams["code"]) && !string.IsNullOrEmpty(queryParams["state"]);
    }

    private void OnDestroy()
    {
        listener.Stop();
    }
}
