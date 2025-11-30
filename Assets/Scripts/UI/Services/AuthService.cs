using UnityEngine;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Amethral.Common.DTOs;

public class AuthService : MonoBehaviour
{
    public static AuthService Instance { get; private set; }

    // On stockera l'URL ici (voir section Configuration plus bas)
    [SerializeField] private AppConfig _config;
    private string _apiBaseUrl;
    
    private HttpClient _client;
    
    // Stockage du Token JWT pour les futures requêtes
    public string JwtToken { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _client = new HttpClient();
        }
    }
    
    private void Start() 
    {
        _apiBaseUrl = _config.GetApiUrl();
    }

    public async Task<(bool success, string message)> LoginAsync(string email, string password)
    {
        var loginData = new LoginRequest { Email = email, Password = password };
        var json = JsonConvert.SerializeObject(loginData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _client.PostAsync($"{_apiBaseUrl}/login", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var data = JsonConvert.DeserializeObject<LoginResponse>(responseString);
                JwtToken = data.token; // Sauvegarde du token
                
                // Configurer le header pour les futures requêtes (ex: /me)
                _client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", JwtToken);
                
                return (true, "Login Successful");
            }
            else
            {
                var error = JsonConvert.DeserializeObject<ErrorResponse>(responseString);
                return (false, error?.message ?? "Login failed");
            }
        }
        catch (System.Exception ex)
        {
            return (false, "Connection error: " + ex.Message);
        }
    }

    public async Task<(bool success, string message)> RegisterAsync(string username, string email, string password)
    {
        var registerData = new RegisterRequest { Username = username, Email = email, Password = password };
        var json = JsonConvert.SerializeObject(registerData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _client.PostAsync($"{_apiBaseUrl}/register", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return (true, "Registration successful! Please login.");
            }
            else
            {
                // Gestion du Conflict (409) ou BadRequest (400)
                // Votre API renvoie { message = "..." } en cas d'erreur
                var error = JsonConvert.DeserializeObject<ErrorResponse>(responseString);
                return (false, error?.message ?? "Registration failed");
            }
        }
        catch (System.Exception ex)
        {
            return (false, "Connection error: " + ex.Message);
        }
    }
}