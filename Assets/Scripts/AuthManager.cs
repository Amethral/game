using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Amethral.common;
using TMPro;

public class AuthManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField userField;
    public TMP_InputField passField;
    public TMP_InputField emailField;
    public TextMeshProUGUI statusText;

    // Ensure port matches API (check launchSettings.json)
    private string baseUrl = "http://localhost:5191/api/auth";

    public void TryRegister() => StartCoroutine(SendRegister());
    public void TryLogin() => StartCoroutine(SendLogin());

    IEnumerator SendRegister()
    {
        var data = new RegisterRequest
        {
            Username = userField.text,
            Password = passField.text,
            Email = emailField.text
        };

        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest req = new UnityWebRequest(baseUrl + "/register", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var res = JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text);
                statusText.text = res.Message;
            }
            else
            {
                statusText.text = "Error: " + req.error;
            }
        }
    }

    IEnumerator SendLogin()
    {
        var data = new LoginRequest
        {
            Username = userField.text,
            Password = passField.text
        };

        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest req = new UnityWebRequest(baseUrl + "/login", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var res = JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text);
                if (res.Success)
                    statusText.text = "Success! Token: " + res.Token;
                else
                    statusText.text = res.Message;
            }
            else
            {
                statusText.text = "Error: " + req.error;
            }
        }
    }
}