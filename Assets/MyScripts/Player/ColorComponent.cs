using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(PhotonView))]
public class PlayerColor : MonoBehaviourPun
{
    [Header("Color Settings")]
    [SerializeField] 
    private Color[] availableColors;
    [SerializeField] 
    private string colorProperty = "_Color";

    private Renderer playerRenderer;
    private int currentColorIndex;

    /* Main action */
    public System.Action<Color> OnColorChanged;

    private void Awake()
    {
        playerRenderer = GetComponent<Renderer>();

        /* Init color */
        if (availableColors != null && availableColors.Length > 0) { SetColor(availableColors[0]); }
    }

    public void ChangeToNextColor()
    {
        if (availableColors == null || availableColors.Length == 0) return;

        currentColorIndex++;
        if(currentColorIndex >= availableColors.Length) currentColorIndex = 0;
        /* Local change color */
        var newColor = availableColors[currentColorIndex]; 
        SetColor(newColor);

        /* Net Sync RPC */
        if (photonView.IsMine) { photonView.RPC("RPC_SyncColor", RpcTarget.Others, newColor.r, newColor.g, newColor.b);  }
    }

    public void SetColor(Color newColor)
    {
        if (playerRenderer != null)
        {
            playerRenderer.material.SetColor(colorProperty, newColor);
            OnColorChanged?.Invoke(newColor);
        }
    }

    [PunRPC]
    private void RPC_SyncColor(float r, float g, float b)
    {
        /* Only for others */
        if (photonView.IsMine) return;

        SetColor(new Color(r, g, b));
    }
}