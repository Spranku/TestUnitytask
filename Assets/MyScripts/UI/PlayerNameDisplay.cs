using Photon.Pun;
using TMPro;
using UnityEngine;

public class PlayerNameDisplay : MonoBehaviourPun
{
    [Header("References")]
    [SerializeField] 
    private TextMeshProUGUI nameText;
    [SerializeField] 
    private Transform nameUI; 

    [Header("Colors")]
    [SerializeField] 
    private Color localPlayerColor = Color.green;
    [SerializeField] 
    private Color otherPlayerColor = Color.blue;

    private Camera mainCamera;

    private void Start()
    {
        /* Check owner */
        if (photonView.Owner == null) return;

        /* Check ref */
        if (nameText == null) return;

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }

        /* Get nickname */
        var playerName = photonView.Owner.NickName;
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

    private void LateUpdate()
    {
        /* Rotate nickname to camera*/
        if (nameUI != null && mainCamera != null)
        {
            var direction = mainCamera.transform.position - nameUI.position;
            direction.y = 0; 

            if (direction != Vector3.zero)
            {
                nameUI.rotation = Quaternion.LookRotation(direction);

                /* Fix rotate text to camera*/
                nameUI.Rotate(0, 180, 0);
            }
        }
    }
}