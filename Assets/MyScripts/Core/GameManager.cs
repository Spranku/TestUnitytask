using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Settings")]
    public GameObject playerPrefab;
    public Vector3 spawnPosition = new Vector3(5, 5f, 5);

    private static GameManager instance;
    private GameObject localPlayerObject;

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

    private void OnDestroy() { if (instance == this) instance = null; }

    public override void OnLeftRoom()
    {
        localPlayerObject = null;
        LoadMainMenu();
    }

    private void TrySpawnPlayer()
    {
        /* Safe check */
        if (playerPrefab == null) return;
        if (localPlayerObject != null) return;
        if (IsPlayerSpawned()) return;

        /* Network check*/
        if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InRoom)
        {
            Invoke(nameof(TrySpawnPlayer), 0.1f);
            return;
        }

        /* Spawn */
        localPlayerObject = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);

        if (localPlayerObject != null)
        {
            SetPlayerSpawned(true);
        }
    }

    private bool IsPlayerSpawned()
    {
        if (PhotonNetwork.LocalPlayer == null) return false;

        return PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("IsSpawned") &&
               (bool)PhotonNetwork.LocalPlayer.CustomProperties["IsSpawned"];
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
        localPlayerObject = null;
        SetPlayerSpawned(false);

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
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