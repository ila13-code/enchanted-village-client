using System;
using System.Net;
using Unical.Demacs.EnchantedVillage;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleHttpServer : MonoBehaviour
{
    private HttpListener listener;
    private bool isListening = false;

    public void launce()
    {
        listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8081/login-callback/");
        listener.Start();
        isListening = true;
        Debug.Log("Listening...");
        StartListening();
    }

    private async void StartListening()
    {
        try
        {
            while (isListening)
            {
                var context = await listener.GetContextAsync();
                var requestUrl = context.Request.Url.ToString();
                Debug.Log($"Received request: {requestUrl}");

                if (ContainsAuthorizationParameters(requestUrl))
                {
                    KeycloakService keycloakService = FindObjectOfType<KeycloakService>();
                    keycloakService.HandleAuthorizationResponse(requestUrl);

                    string responseString = "Login successful! You can close this window.";
                    var response = context.Response;
                    response.ContentLength64 = System.Text.Encoding.UTF8.GetByteCount(responseString);
                    var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    response.OutputStream.Close();

                    // Imposta il flag a false prima di fermare il listener
                    isListening = false;
                    listener.Stop();
                    SceneManager.LoadSceneAsync(1);
                    break;
                }
                else
                {
                    Debug.Log("Waiting for authorization code and state in the URL...");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in HTTP listener: {ex.Message}");
            isListening = false;
            if (listener != null && listener.IsListening)
            {
                listener.Stop();
            }
        }
    }

    private void OnDestroy()
    {
        isListening = false;
        if (listener != null && listener.IsListening)
        {
            listener.Stop();
        }
    }

    private bool ContainsAuthorizationParameters(string url)
    {
        Uri uri = new Uri(url);
        var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
        return !string.IsNullOrEmpty(queryParams["code"]) && !string.IsNullOrEmpty(queryParams["state"]); 
    }
}