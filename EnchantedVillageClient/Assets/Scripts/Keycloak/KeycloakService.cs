using UnityEngine;
using System;
using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.Networking;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;

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

        private const string KEYCLOAK_URL = "http://192.168.187.111:8080";
        private const string KEYCLOAK_REALM = "enchanted-village";
        private const string KEYCLOAK_CLIENT_ID = "enchanted-village";
        private const string KEYCLOAK_CLIENT_SECRET = ".";
        private const string KEYCLOAK_REDIRECT_URI_LOGIN = "http://192.168.187.111:8081/login-callback";
        private const string KEYCLOAK_REDIRECT_URI_LOGOUT = "http://192.168.187.111:8080/logout";


        public void Login()
        {
            string authorizationUrl = GenerateAuthorizationURL();
            OpenBrowser(authorizationUrl);
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

            ClearTokens();
            return url;
        }

        private void OpenBrowser(string url)
        {
            // Apri l'URL nel browser predefinito del sistema
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // Necessario per aprire l'URL nel browser
                });
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

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Error getting access token: {www.error}");
                }
                else
                {
                    string responseText = www.downloadHandler.text;
                    TokenResponse tokenResponse = JsonUtility.FromJson<TokenResponse>(responseText);

                    SaveAccessToken(tokenResponse.access_token);
                    SaveRefreshToken(tokenResponse.refresh_token);
                    SaveIdToken(tokenResponse.id_token);

                    string email = GetEmailFromToken(tokenResponse.access_token);
                    PlayerPrefs.SetString("userEmail", email);
                    PlayerPrefs.SetString("isLogged", "true");

                    Debug.Log(tokenResponse.access_token);
                    Debug.Log("Login completato con successo!");
                    //todo : aggiungere il caricamento della scena successiva
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

        private void ClearTokens()
        {
            PlayerPrefs.DeleteKey(ACCESS_TOKEN);
            PlayerPrefs.DeleteKey(REFRESH_TOKEN);
            PlayerPrefs.DeleteKey(ID_TOKEN);
            PlayerPrefs.DeleteKey(STATE);
            PlayerPrefs.DeleteKey(CODE_VERIFIER_KEY);
            PlayerPrefs.DeleteKey("userEmail");
            PlayerPrefs.DeleteKey("isLogged");
            PlayerPrefs.DeleteKey("role");
        }

        private string GetEmailFromToken(string token)
        {
            var parts = token.Split('.');
            var payload = parts[1];
            var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            var payloadData = JsonUtility.FromJson<Dictionary<string, object>>(payloadJson);

            if (payloadData.TryGetValue("email", out object email))
            {
                return email as string;
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
