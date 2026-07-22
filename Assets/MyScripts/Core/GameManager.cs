using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Settings")]
    public GameObject playerPrefab;
    public Vector3 spawnPosition = new Vector3(5, 0.5f, 0);

    [Header("Debug")]
    public bool debugMode = true;

    private static GameManager instance;
    private bool isSpawning = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start() { TrySpawnPlayer(); }

    public override void OnJoinedRoom() { TrySpawnPlayer(); }

    public override void OnLeftRoom() { LoadMainMenu(); }

    private void OnDestroy() { if (instance == this) instance = null;  }

    private void TrySpawnPlayer()
    {
        /* Safe check */
        if (isSpawning) return;
        if (playerPrefab == null) return;
        if (IsPlayerSpawned()) return;

        isSpawning = true;
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
        if (player != null) SetPlayerSpawned(true);
        isSpawning = false;
    }

    private bool IsPlayerSpawned()
    {
        if (PhotonNetwork.LocalPlayer == null) return false;

        return PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("IsSpawned") && (bool)PhotonNetwork.LocalPlayer.CustomProperties["IsSpawned"];
    }

    private void SetPlayerSpawned(bool value)
    {
        if (PhotonNetwork.LocalPlayer == null) return;

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["IsSpawned"] = value;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public void LeaveGame()
    {
        /* Reset state */
        SetPlayerSpawned(false);

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            /* if user is not in room - load menu */
            LoadMainMenu();
        }
    }

    private void LoadMainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;

        if (instance != null)
        {
            Destroy(instance.gameObject);
            instance = null;
        }
        SceneManager.LoadScene(0);
    }
}