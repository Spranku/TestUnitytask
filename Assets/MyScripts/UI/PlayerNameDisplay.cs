using Photon.Pun;
using TMPro;
using UnityEngine;

public class PlayerNameDisplay : MonoBehaviourPun
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("Colors")]
    [SerializeField] private Color localPlayerColor = Color.green;
    [SerializeField] private Color otherPlayerColor = Color.blue;

    private void Start()
    {
        /* Check owner */
        if (photonView.Owner == null) return;

        /* Check ref */
        if (nameText == null) return;
        

        /* Get nickname */
        string playerName = photonView.Owner.NickName;
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = $"Player_{photonView.Owner.ActorNumber}";
        }

        /* Update text&color */
        nameText.text = playerName;
        nameText.color = photonView.IsMine ? localPlayerColor : otherPlayerColor;

        if (photonView.IsMine)
        {
            nameText.fontStyle = FontStyles.Bold;
        }
    }
}