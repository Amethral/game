using FishNet;
using FishNet.Connection;
using FishNet.Transporting;
using FishNet.Object;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomPlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;

    [Tooltip("Name of the scene where the player should be spawned.")]
    [SerializeField] private string _targetSceneName = "Game";

    private void Start()
    {
        // Subscribe to connection state changes on the server.
        if (InstanceFinder.ServerManager != null)
        {
            InstanceFinder.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
        }
        else
        {
            Debug.LogError("[CustomPlayerSpawner] ServerManager not found in Start(). Ensure this runs on the server side.");
        }

        // Listen for scene load events to know when the target scene becomes active.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (InstanceFinder.ServerManager != null)
        {
            InstanceFinder.ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void ServerManager_OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        // Only act on the server when a client has fully started.
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            if (IsTargetSceneLoaded())
            {
                Debug.Log($"[CustomPlayerSpawner] Client {conn.ClientId} connected and target scene is loaded. Spawning player.");
                SpawnPlayer(conn);
            }
            else
            {
                Debug.Log($"[CustomPlayerSpawner] Client {conn.ClientId} connected but target scene '{_targetSceneName}' not loaded yet. Will spawn when scene loads.");
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != _targetSceneName) return;

        Debug.Log($"[CustomPlayerSpawner] Target scene '{_targetSceneName}' loaded. Spawning players for existing connections.");
        if (InstanceFinder.ServerManager == null) return;

        foreach (var kvp in InstanceFinder.ServerManager.Clients)
        {
            var conn = kvp.Value;
            // If this connection hasn't got a player yet, spawn one.
            if (conn.FirstObject == null)
            {
                SpawnPlayer(conn);
            }
        }
    }

    private bool IsTargetSceneLoaded()
    {
        var scene = SceneManager.GetSceneByName(_targetSceneName);
        return scene.IsValid() && scene.isLoaded;
    }

    private void SpawnPlayer(NetworkConnection conn)
    {
        if (_playerPrefab == null)
        {
            Debug.LogWarning("[CustomPlayerSpawner] Player prefab not assigned. Cannot spawn player.");
            return;
        }

        var scene = SceneManager.GetSceneByName(_targetSceneName);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError($"[CustomPlayerSpawner] Attempted to spawn player but target scene '{_targetSceneName}' is not loaded.");
            return;
        }

        // Use FishNet's pooling/instantiation.
        NetworkObject netObj = InstanceFinder.NetworkManager.GetPooledInstantiated(_playerPrefab, asServer: true);
        if (netObj == null)
        {
            Debug.LogError("[CustomPlayerSpawner] Failed to instantiate player prefab as NetworkObject.");
            return;
        }

        // Spawn the player into the target scene for the given connection.
        InstanceFinder.ServerManager.Spawn(netObj, conn, scene);
        Debug.Log($"[CustomPlayerSpawner] Player spawned for client {conn.ClientId} in scene '{scene.name}'.");
    }
}
