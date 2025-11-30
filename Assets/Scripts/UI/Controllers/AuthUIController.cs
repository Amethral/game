using UnityEngine;
using UnityEngine.UIElements;

public class AuthUIController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AppConfig _config; // Drag your AppConfig here
    [SerializeField] private GameObject _serverSelectorUI; // Drag your ServerSelector GameObject here

    private UIDocument _document;

    // UI Elements
    private TextField _emailInput;
    private TextField _passwordInput;
    private Button _signInBtn;
    private Label _registerLink; // The new text label
    private Label _errorLabel;

    private void OnEnable()
    {
        _document = GetComponent<UIDocument>();
        var root = _document.rootVisualElement;

        // Query Elements
        _emailInput = root.Q<TextField>("EmailInput");
        _passwordInput = root.Q<TextField>("PasswordInput");
        _signInBtn = root.Q<Button>("SignInButton");
        _registerLink = root.Q<Label>("RegisterLink");
        _errorLabel = root.Q<Label>("ErrorLabel");

        // Callbacks
        if (_signInBtn != null) 
            _signInBtn.RegisterCallback<ClickEvent>(OnSignInClicked);
            
        if (_registerLink != null) 
            _registerLink.RegisterCallback<ClickEvent>(OnRegisterLinkClicked);
    }

    private void Start()
    {
        if (_serverSelectorUI != null)
        {
            _serverSelectorUI.SetActive(false);
        }
    }

    private void OnRegisterLinkClicked(ClickEvent evt)
    {
        if (_config != null)
        {
            // Assuming AppConfig has a method or public string for the URL
            // Adjust "RegisterUrl" to match your actual variable name in AppConfig
            Application.OpenURL(_config.GetRegisterUrl()); 
        }
        else
        {
            Debug.LogError("AppConfig is missing in AuthUIController!");
            ShowError("Configuration error: Cannot open browser.");
        }
    }

    private async void OnSignInClicked(ClickEvent evt)
    {
        string email = _emailInput.value;
        string pass = _passwordInput.value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            ShowError("Please enter Email and Password.");
            return;
        }

        SetInteractivity(false);
        ShowError("Signing in...");

        var (success, message) = await AuthService.Instance.LoginAsync(email, pass);

        if (success)
        {
            Debug.Log("Token: " + AuthService.Instance.JwtToken);
            ShowSuccess("Success!");
            
            if (_serverSelectorUI != null)
            {
                gameObject.SetActive(false);
                _serverSelectorUI.SetActive(true);
            }
            else
            {
                Debug.LogWarning("ServerSelectorUI is not assigned in AuthUIController!");
                // Fallback or keep existing behavior if needed
                // SceneManager.LoadScene("Lobby");
            }
        }
        else
        {
            ShowError(message);
            SetInteractivity(true);
        }
    }

    // --- Helpers ---

    private void ShowError(string msg)
    {
        _errorLabel.text = msg;
        _errorLabel.style.color = new StyleColor(new Color(1f, 0.4f, 0.4f));
    }

    private void ShowSuccess(string msg)
    {
        _errorLabel.text = msg;
        _errorLabel.style.color = new StyleColor(Color.green);
    }

    private void SetInteractivity(bool enabled)
    {
        _signInBtn.SetEnabled(enabled);
        _emailInput.SetEnabled(enabled);
        _passwordInput.SetEnabled(enabled);
    }
}