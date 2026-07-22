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
        if (playerPrefab == null) return;
        

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
        }
    }
}
