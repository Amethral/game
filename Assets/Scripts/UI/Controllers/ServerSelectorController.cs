using FishNet;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class ServerSelectorController : MonoBehaviour
{
    private UIDocument _document;
    private Button _mainServerBtn;
    private Button _devServerBtn;

    private void OnEnable()
    {
        _document = GetComponent<UIDocument>();
        if (_document == null)
        {
            Debug.LogError("UIDocument not found on ServerSelectorController!");
            return;
        }

        var root = _document.rootVisualElement;
        if (root == null) return;

        _mainServerBtn = root.Q<Button>("MainServerBtn");
        _devServerBtn = root.Q<Button>("DevServerBtn");

        if (_mainServerBtn != null)
            _mainServerBtn.RegisterCallback<ClickEvent>(OnMainServerClicked);

        if (_devServerBtn != null)
            _devServerBtn.RegisterCallback<ClickEvent>(OnDevServerClicked);
    }

    private void OnMainServerClicked(ClickEvent evt)
    {
        Connect("89.168.55.58");
    }

    private void OnDevServerClicked(ClickEvent evt)
    {
        Connect("localhost");
    }

    private void Connect(string address)
    {
        Debug.Log($"[ServerSelector] Attempting to connect to {address}...");
        
        if (InstanceFinder.NetworkManager != null)
        {
            Debug.Log("[ServerSelector] NetworkManager found. Setting address and starting connection.");
            InstanceFinder.TransportManager.Transport.SetClientAddress(address);
            InstanceFinder.ClientManager.StartConnection();
            Debug.Log("[ServerSelector] Connection started. Loading Game scene...");
            SceneManager.LoadScene("Game");
        }
        else
        {
            Debug.LogError("[ServerSelector] NetworkManager not found! Cannot connect.");
        }
    }
}
