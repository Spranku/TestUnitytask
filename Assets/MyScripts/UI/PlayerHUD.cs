using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class PlayerHUD : MonoBehaviourPunCallbacks
{
    public void OnLeaveButton()
    {
        if (PhotonNetwork.InRoom)
        {
            /* Destrtoy game manager before leaving */
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                Destroy(gm.gameObject);
            }

            /* Reset state for next game */
            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("IsSpawned"))
            {
                ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
                props.Add("IsSpawned", false);
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            }

            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.AutomaticallySyncScene = false;

        SceneManager.LoadScene(0);
    }
}
