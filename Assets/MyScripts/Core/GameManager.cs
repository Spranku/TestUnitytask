using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public Vector3 spawnPosition = new Vector3(5, 0.5f, 0);

    [Header("Debug")]
    public bool debugMode = true;

    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnPlayer();
        }
        else
        {
            ///Debug.Log("GameManager::Start - Not ConnectedAndReady for spawn");
        }
    }

    public override void OnJoinedRoom()
    { 
        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("IsSpawned"))
        {
            SpawnPlayer();
        }
    }

    void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
           /// Debug.Log("GameManager::SpawnPlayer - prefab missing");
            return;
        }

        /* Do nothing? */
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("IsSpawned"))
        {
            ///if (debugMode) Debug.Log("GameManager::SpawnPlayer - player " + PhotonNetwork.NickName + " already created, skip spawn");
            return;
        }

        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
        if (player != null)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props.Add("IsSpawned", true);
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

            ///if (debugMode) Debug.Log("GameManager::SpawnPlayer - player " + PhotonNetwork.NickName + " success spawned");
        }
        else
        {
            ///Debug.Log("GameManager::SpawnPlayer - Failed to spawn player " + PhotonNetwork.NickName);
        }
    }

    ///public override void OnPlayerEnteredRoom(Player newPlayer) { if (debugMode) Debug.Log("GameManager::SpawnPlayer - Player " + PhotonNetwork.NickName + " joined room"); }

    ///public override void OnPlayerLeftRoom(Player otherPlayer) { if (debugMode) Debug.Log("GameManager::SpawnPlayer - Player " + PhotonNetwork.NickName + " leaved room"); }
}
