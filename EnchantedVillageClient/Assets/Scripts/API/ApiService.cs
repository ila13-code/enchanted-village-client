using Newtonsoft.Json;
using System.Text;
using System;
using Unical.Demacs.EnchantedVillage;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;

public class ApiService : MonoBehaviour
{
    private const string API_BASE_URL = "http://localhost:7001/api/v1";
    private static ApiService instance;
    private bool isRefreshing = false;

    public static ApiService Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("ApiService");
                instance = go.AddComponent<ApiService>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private UnityWebRequest CreateRequest(string endpoint, string method, object body = null)
    {
        string url = $"{API_BASE_URL}/{endpoint}";
        UnityWebRequest request = new UnityWebRequest(url, method);

        if (body != null)
        {
            string jsonBody = JsonConvert.SerializeObject(body);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        }

        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        string accessToken = PlayerPrefs.GetString("accessToken", "");
        if (!string.IsNullOrEmpty(accessToken))
        {
            accessToken = accessToken.Trim();
            if (!accessToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                accessToken = "Bearer " + accessToken;
            }
            request.SetRequestHeader("Authorization", accessToken);
            Debug.Log($"Request to {url} with token: {accessToken.Substring(0, Math.Min(accessToken.Length, 50))}...");
        }
        else
        {
            Debug.LogError("No access token found!");
            throw new UnauthorizedAccessException("No access token available");
        }

        return request;
    }

    private IEnumerator SendRequest<T>(string endpoint, string method, object body, Action<T> onSuccess, Action<string> onError, bool isRetry = false)
    {
        if (!ServicesManager.Instance?.KeycloakService?.IsAuthenticated() ?? true)
        {
            Debug.LogError("User not authenticated");
            onError?.Invoke("User not authenticated");
            yield break;
        }

        while (isRefreshing)
        {
            yield return new WaitForSeconds(0.5f);
        }

        UnityWebRequest request = null;
        try
        {
            request = CreateRequest(endpoint, method, body);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error creating request: {e.Message}");
            onError?.Invoke(e.Message);
            yield break;
        }

        using (request)
        {
            yield return request.SendWebRequest();
            Debug.Log($"Response Code: {request.responseCode} for {endpoint}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = request.downloadHandler.text;
                    Debug.Log($"Success Response for {endpoint}: {responseText}");
                    T response = JsonConvert.DeserializeObject<T>(responseText);
                    onSuccess?.Invoke(response);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing response: {e.Message}");
                    onError?.Invoke($"Error parsing response: {e.Message}");
                }
            }
            else if ((request.responseCode == 401 || request.responseCode == 403) && !isRetry)
            {
                Debug.Log($"Auth error on {endpoint}, attempting refresh...");
                isRefreshing = true;
                yield return StartCoroutine(RefreshAndRetry(endpoint, method, body, onSuccess, onError));
            }
            else if (request.responseCode == 404)
            {
                // Pass null for 404 responses to indicate no data found
                if (typeof(T) == typeof(GameInformation))
                {
                    onSuccess?.Invoke(default(T));
                }
                else
                {
                    onError?.Invoke($"Resource not found: {endpoint}");
                }
            }
            else
            {
                string errorMessage = $"Request to {endpoint} failed: {request.error}";
                Debug.LogError(errorMessage);
                Debug.LogError($"Response: {request.downloadHandler.text}");
                onError?.Invoke(errorMessage);
            }
        }
    }
    private IEnumerator RefreshAndRetry<T>(string endpoint, string method, object body, Action<T> onSuccess, Action<string> onError)
    {
        try
        {
            bool refreshSuccess = false;
            yield return StartCoroutine(ServicesManager.Instance.KeycloakService.RefreshToken(
                onSuccess: () => refreshSuccess = true,
                onError: error => Debug.LogError($"Token refresh failed: {error}")
            ));

            isRefreshing = false;

            if (refreshSuccess)
            {
                Debug.Log("Token refreshed, retrying request...");
                yield return StartCoroutine(SendRequest(endpoint, method, body, onSuccess, onError, true));
            }
            else
            {
                Debug.LogError("Token refresh failed, cannot retry request");
                onError?.Invoke("Authentication failed");
            }
        }
        finally
        {
            isRefreshing = false;
        }
    }

    

    public IEnumerator GetGameInformation(Action<GameInformation> onSuccess, Action<string> onError)
    {
        string userEmail = PlayerPrefs.GetString("userEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            onError?.Invoke("User email not found");
            yield break;
        }

        string endpoint = $"game-information/getGameInformation?email={Uri.EscapeDataString(userEmail)}";
        Debug.Log($"Getting game information for email: {userEmail}");

        yield return StartCoroutine(SendRequest<GameInformation>(
            endpoint,
            "GET",
            null,
            (gameInfo) => {
                // Explicitly handle null case as "no data found"
                if (gameInfo == null)
                {
                    Debug.Log("No game information found on server (404)");
                    onSuccess?.Invoke(null);
                }
                else
                {
                    onSuccess?.Invoke(gameInfo);
                }
            },
            onError
        ));
    }


    public IEnumerator GetGameInformationByEmail(string userEmail, Action<GameInformation> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrEmpty(userEmail))
        {
            Debug.LogError("[GetGameInformationByEmail] Email is null or empty");
            onError?.Invoke("Email cannot be null or empty");
            yield break;
        }

        Debug.Log($"[GetGameInformationByEmail] Starting request for email: {userEmail}");
        string endpoint = $"game-information/getGameInformation?email={Uri.EscapeDataString(userEmail)}";
        Debug.Log($"[GetGameInformationByEmail] Full endpoint: {endpoint}");

        yield return StartCoroutine(SendRequest<GameInformation>(
            endpoint,
            "GET",
            null,
            (gameInfo) => {
                Debug.Log($"[GetGameInformationByEmail] Response received. GameInfo is null? {gameInfo == null}");
                if (gameInfo == null)
                {
                    Debug.Log("[GetGameInformationByEmail] No game information found on server (404)");
                    onSuccess?.Invoke(null);
                }
                else
                {
                    Debug.Log($"[GetGameInformationByEmail] Game info received: Level={gameInfo.level}, Buildings count={gameInfo.buildings?.Count ?? 0}");
                    onSuccess?.Invoke(gameInfo);
                }
            },
            (error) => {
                Debug.LogError($"[GetGameInformationByEmail] Error received: {error}");
                onError?.Invoke(error);
            }
        ));
    }

    public IEnumerator CreateGameInformation(GameInformation gameInfo, Action<GameInformation> onSuccess, Action<string> onError)
    {
        string userEmail = PlayerPrefs.GetString("userEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            onError?.Invoke("User email not found");
            yield break;
        }

        string endpoint = $"game-information/createGameInformation?email={Uri.EscapeDataString(userEmail)}";
        Debug.Log($"Creating game information for email: {userEmail}");
        yield return StartCoroutine(SendRequest<GameInformation>(endpoint, "POST", gameInfo, onSuccess, onError));
    }

    public IEnumerator UpdateGameInformation(GameInformation gameInfo, Action<GameInformation> onSuccess, Action<string> onError)
    {
        string userEmail = PlayerPrefs.GetString("userEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            onError?.Invoke("User email not found");
            yield break;
        }

        string endpoint = $"game-information/updateGameInformation?email={Uri.EscapeDataString(userEmail)}";
        Debug.Log($"Updating game information for email: {userEmail}");
        yield return StartCoroutine(SendRequest<GameInformation>(endpoint, "PATCH", gameInfo, onSuccess, onError));
    }

    public IEnumerator ExistsByEmail(string email, Action<bool> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrEmpty(email))
        {
            Debug.LogError("[ExistsByEmail] Email parameter is null or empty");
            onError?.Invoke("Email cannot be null or empty");
            yield break;
        }

        string endpoint = $"user/existsByEmail/{Uri.EscapeDataString(email)}";
        Debug.Log($"[ExistsByEmail] Checking existence for email: {email}");

        yield return StartCoroutine(SendRequest<bool>(
            endpoint,
            "GET",
            null,
            (exists) => {
                Debug.Log($"[ExistsByEmail] User existence check result for {email}: {exists}");
                onSuccess?.Invoke(exists);
            },
            (error) => {
                Debug.LogError($"[ExistsByEmail] Error checking user existence: {error}");
                onError?.Invoke(error);
            }
        ));
    }

}