using UnityEngine;
using System;
using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.Networking;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using Newtonsoft.Json;
using Unity.VisualScripting;
using Newtonsoft.Json.Linq;

namespace Unical.Demacs.EnchantedVillage
{
    public class KeycloakService : MonoBehaviour
    {
        private const string STATE = "state";
        private const string CODE = "code";
        private const string CODE_VERIFIER_KEY = "codeVerifier";
        private const string ACCESS_TOKEN = "accessToken";
        private const string REFRESH_TOKEN = "refreshToken";
        private const string ID_TOKEN = "idToken";

        private const string KEYCLOAK_URL = "http://localhost:8080";
        private const string KEYCLOAK_REALM = "enchanted-village";
        private const string KEYCLOAK_CLIENT_ID = "enchanted-village";
        private const string KEYCLOAK_CLIENT_SECRET = "TsiWKKGums7TFstrBbbbI8o0MobPZolb";
        private const string KEYCLOAK_REDIRECT_URI_LOGIN = "http://localhost:8081/login-callback";
        private const string KEYCLOAK_REDIRECT_URI_LOGOUT = "http://localhost:8080/logout";

        private WebViewObject webViewObject;

        private void Start()
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                InitializeWebView();
            }
        }

        private void InitializeWebView()
        {
            webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
            webViewObject.Init(
                cb: (msg) =>
                {
                    Debug.Log(string.Format("CallFromJS[{0}]", msg));
                },
                err: (msg) =>
                {
                    Debug.Log(string.Format("CallOnError[{0}]", msg));
                },
                started: (msg) =>
                {
                    Debug.Log(string.Format("CallOnStarted[{0}]", msg));
                },
                hooked: (msg) =>
                {
                    Debug.Log(string.Format("CallOnHooked[{0}]", msg));
                },
                ld: (msg) =>
                {
                    Debug.Log(string.Format("CallOnLoaded[{0}]", msg));
                    // Use the 'msg' parameter which contains the loaded URL
                    if (msg.StartsWith(KEYCLOAK_REDIRECT_URI_LOGIN))
                    {
                        HandleAuthorizationResponse(msg);
                        webViewObject.SetVisibility(false);
                    }
                });
            webViewObject.AddCustomHeader("X-Unity-SHM", "1");
            webViewObject.EvaluateJS(@"
                window.addEventListener('load', function() {
                    window.unity.call('url:' + window.location.href);
                });
                (function() {
                    var originalPushState = history.pushState;
                    var originalReplaceState = history.replaceState;
                    history.pushState = function() {
                        originalPushState.apply(history, arguments);
                        window.unity.call('url:' + window.location.href);
                    };
                    history.replaceState = function() {
                        originalReplaceState.apply(history, arguments);
                        window.unity.call('url:' + window.location.href);
                    };
                })();
            ");
        }

        public void Login()
        {

            string authorizationUrl = GenerateAuthorizationURL();
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                webViewObject.LoadURL(authorizationUrl);
                webViewObject.SetVisibility(true);
            }
            else
            {
                OpenBrowser(authorizationUrl);
            }
        }

        public void Logout()
        {
            string logoutUrl = CreateLogoutUrl();
            OpenBrowser(logoutUrl);
        }

        private string GenerateAuthorizationURL()
        {
            string codeVerifier = GenerateCodeVerifier();
            string codeChallenge = GenerateCodeChallenge(codeVerifier);
            string state = GenerateState();
            SaveState(state);
            SaveCodeVerifier(codeVerifier);

            return $"{KEYCLOAK_URL}/realms/{KEYCLOAK_REALM}/protocol/openid-connect/auth" +
                   $"?client_id={KEYCLOAK_CLIENT_ID}" +
                   $"&redirect_uri={Uri.EscapeDataString(KEYCLOAK_REDIRECT_URI_LOGIN)}" +
                   $"&response_type=code" +
                   $"&scope=openid" +
                   $"&code_challenge={codeChallenge}" +
                   $"&code_challenge_method=S256" +
                   $"&state={state}";
        }

        private string CreateLogoutUrl()
        {
            string idToken = GetIdToken();
            string url = $"{KEYCLOAK_URL}/realms/{KEYCLOAK_REALM}/protocol/openid-connect/logout" +
                         $"?client_id={KEYCLOAK_CLIENT_ID}" +
                         $"&post_logout_redirect_uri={Uri.EscapeDataString(KEYCLOAK_REDIRECT_URI_LOGOUT)}";

            if (!string.IsNullOrEmpty(idToken))
            {
                url += $"&id_token_hint={idToken}";
            }

            //ClearTokens();
            return url;
        }

        private void OpenBrowser(string url)
        {
            try
            {
                Application.OpenURL(url);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Errore nell'apertura del browser: {ex.Message}");
            }
        }

        public void HandleAuthorizationResponse(string url)
        {
            Uri uri = new Uri(url);
            string code = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("code");
            string state = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("state");

            if (state != GetState())
            {
                Debug.LogError("Invalid state parameter");
                return;
            }

            StartCoroutine(GetAccessTokenFromCode(code));
        }

        private IEnumerator GetAccessTokenFromCode(string code)
        {
            string codeVerifier = GetCodeVerifier();
            WWWForm form = new WWWForm();
            form.AddField("client_id", KEYCLOAK_CLIENT_ID);
            form.AddField("client_secret", KEYCLOAK_CLIENT_SECRET);
            form.AddField("grant_type", "authorization_code");
            form.AddField("code", code);
            form.AddField("redirect_uri", KEYCLOAK_REDIRECT_URI_LOGIN);
            form.AddField("code_verifier", codeVerifier);

            string tokenUrl = $"{KEYCLOAK_URL}/realms/{KEYCLOAK_REALM}/protocol/openid-connect/token";
            using (UnityWebRequest www = UnityWebRequest.Post(tokenUrl, form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string responseText = www.downloadHandler.text;
                        TokenResponse tokenResponse = JsonUtility.FromJson<TokenResponse>(responseText);

                            SaveAccessToken(tokenResponse.access_token);
                            SaveRefreshToken(tokenResponse.refresh_token);
                            SaveIdToken(tokenResponse.id_token);

                            string email = GetEmailFromToken(tokenResponse.access_token);
                            PlayerPrefs.SetString("userEmail", email);
                            PlayerPrefs.SetString("isLogged", "true");
                            Debug.Log("Login completato con successo!");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Errore nel parsing del token: {ex.Message}");
                    }
                }
            }
        }

      
        private string GenerateCodeVerifier()
        {
            byte[] bytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            return Base64UrlEncode(bytes);
        }

        private string GenerateCodeChallenge(string codeVerifier)
        {
            using (var sha256 = SHA256.Create())
            {
                var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                return Base64UrlEncode(challengeBytes);
            }
        }

        private string GenerateState()
        {
            byte[] stateBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(stateBytes);
            }
            return Base64UrlEncode(stateBytes);
        }

        private string Base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.Split('=')[0];
            output = output.Replace('+', '-');
            output = output.Replace('/', '_');
            return output;
        }

        private void SaveAccessToken(string token) => PlayerPrefs.SetString(ACCESS_TOKEN, token);
        private string GetAccessToken() => PlayerPrefs.GetString(ACCESS_TOKEN);
        private void SaveRefreshToken(string token) => PlayerPrefs.SetString(REFRESH_TOKEN, token);
        private string GetRefreshToken() => PlayerPrefs.GetString(REFRESH_TOKEN);
        private void SaveIdToken(string token) => PlayerPrefs.SetString(ID_TOKEN, token);
        private string GetIdToken() => PlayerPrefs.GetString(ID_TOKEN);
        private void SaveState(string state) => PlayerPrefs.SetString(STATE, state);
        private string GetState() => PlayerPrefs.GetString(STATE);
        private void SaveCodeVerifier(string codeVerifier) => PlayerPrefs.SetString(CODE_VERIFIER_KEY, codeVerifier);
        private string GetCodeVerifier() => PlayerPrefs.GetString(CODE_VERIFIER_KEY);

        private string GetEmailFromToken(string token)
        {
            // Controllo se il token è valido
            if (string.IsNullOrEmpty(token) || !token.Contains('.'))
            {
                Debug.Log("Token non valido: " + token);
                return null;
            }

            // Estrazione del payload dal token
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                Debug.Log("Token JWT non valido, numero di parti: " + parts.Length);
                return null; // Non è un token JWT valido
            }

            var payload = parts[1];
            Debug.Log("Payload estratto: " + payload);

            // Aggiungere padding se necessario
            switch (payload.Length % 4)
            {
                case 2:
                    payload += "==";
                    Debug.Log("Aggiunto padding: " + payload);
                    break;
                case 3:
                    payload += "=";
                    Debug.Log("Aggiunto padding: " + payload);
                    break;
            }

            try
            {
                // Decodifica del payload
                var decodedPayload = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                Debug.Log("Payload decodificato: " + decodedPayload);

                // Deserializzazione del payload usando Newtonsoft.Json
                var jsonPayload = JsonConvert.DeserializeObject<JObject>(decodedPayload);
                Debug.Log("Payload JSON deserializzato: " + jsonPayload.ToString());

                // Estrazione del ruolo
                if (jsonPayload.TryGetValue("resource_access", out JToken resourceAccess))
                {
                    Debug.Log("resource_access trovato.");
                    if (resourceAccess["enchanted-village"] != null)
                    {
                        var enchantedVillage = resourceAccess["enchanted-village"];
                        Debug.Log("Ruolo trovato in 'enchanted-village'.");

                        var roles = enchantedVillage["roles"];
                        if (roles.HasValues)
                        {
                            var role = roles[0].ToString();
                            Debug.Log("Ruolo estratto: " + role);

                            // Salva il ruolo nella local storage
                            //localStorageService.SetItem("role", role);
                        }
                        else
                        {
                            Debug.Log("Nessun ruolo trovato.");
                        }
                    }
                    else
                    {
                        Debug.Log("'enchanted-village' non trovato in resource_access.");
                    }
                }
                else
                {
                    Debug.Log("resource_access non trovato nel payload.");
                }

                // Restituisce l'email
                if (jsonPayload.TryGetValue("email", out JToken email))
                {
                    Debug.Log("Email estratta: " + email.ToString());
                    return email.ToString();
                }
                else
                {
                    Debug.Log("Email non trovata nel payload.");
                }
            }
            catch (FormatException ex)
            {
                Debug.LogError("Errore di formato durante la decodifica Base64: " + ex.Message);
            }
            catch (JsonException ex)
            {
                Debug.LogError("Errore durante la deserializzazione JSON: " + ex.Message);
            }
            catch (Exception ex)
            {
                Debug.LogError("Errore imprevisto: " + ex.Message);
            }

            return null;
        }

        public bool IsTokenExpired(string token)
        {
            var parts = token.Split('.');
            var payload = parts[1];
            var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            var payloadData = JsonUtility.FromJson<Dictionary<string, object>>(payloadJson);

            if (payloadData.TryGetValue("exp", out object expObj))
            {
                long exp = Convert.ToInt64(expObj);
                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                return exp < currentTime;
            }
            return true;
        }

        public string GetRole()
        {
            return PlayerPrefs.GetString("role", "GUEST");
        }

        public bool IsAuthenticated()
        {
            return PlayerPrefs.HasKey("isLogged");
        }

        public IEnumerator RefreshToken(Action onSuccess = null, Action<string> onError = null)
        {
            string refreshToken = GetRefreshToken();

            if (string.IsNullOrEmpty(refreshToken))
            {
                string error = "No refresh token available";
                Debug.LogError(error);
                onError?.Invoke(error);
                Login();
                yield break;
            }

            WWWForm form = new WWWForm();
            form.AddField("client_id", KEYCLOAK_CLIENT_ID);
            form.AddField("client_secret", KEYCLOAK_CLIENT_SECRET);
            form.AddField("grant_type", "refresh_token");
            form.AddField("refresh_token", refreshToken);

            string tokenUrl = $"{KEYCLOAK_URL}/realms/{KEYCLOAK_REALM}/protocol/openid-connect/token";
            using (UnityWebRequest www = UnityWebRequest.Post(tokenUrl, form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string responseText = www.downloadHandler.text;
                        TokenResponse tokenResponse = JsonUtility.FromJson<TokenResponse>(responseText);

                        SaveAccessToken(tokenResponse.access_token);
                        SaveRefreshToken(tokenResponse.refresh_token);
                        SaveIdToken(tokenResponse.id_token);

                        string email = GetEmailFromToken(tokenResponse.access_token);
                        PlayerPrefs.SetString("userEmail", email);
                        Debug.Log(tokenResponse.access_token);
                        Debug.Log("Token refreshed successfully");
                        onSuccess?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        string error = $"Error parsing refresh token response: {ex.Message}";
                        Debug.LogError(error);
                        onError?.Invoke(error);
                        Login();
                    }
                }
                else
                {
                    string error = $"Token refresh failed: {www.error}";
                    Debug.LogError(error);
                    onError?.Invoke(error);

                    // Se il refresh fallisce, probabilmente il token è invalido
                    // quindi meglio reindirizzare al login
                    Login();
                }
            }
        }

        // Metodo helper per verificare se è necessario un refresh
        public bool NeedsTokenRefresh()
        {
            string accessToken = GetAccessToken();
            if (string.IsNullOrEmpty(accessToken))
                return true;

            try
            {
                return IsTokenExpired(accessToken);
            }
            catch (Exception)
            {
                return true;
            }
        }

    }

    // Classe per il parsing della risposta del token
    [Serializable]
    public class TokenResponse
    {
        public string access_token;
        public string refresh_token;
        public string id_token;
    }
}