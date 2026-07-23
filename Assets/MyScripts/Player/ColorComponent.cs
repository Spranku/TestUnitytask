using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(PhotonView))]
public class PlayerColor : MonoBehaviourPunCallbacks
{
    [Header("Color Settings")]
    [SerializeField] private Color[] availableColors;
    [SerializeField] private string colorProperty = "_Color";

    private Renderer playerRenderer;
    private int currentColorIndex;
    private Color currentColor;
    private bool isInitialized = false;

    private System.Action<Color> OnColorChanged;

    private void Awake()
    {
        playerRenderer = GetComponent<Renderer>();

        if (availableColors != null && availableColors.Length > 0)
        {
            currentColor = availableColors[0];

            /* If its local player - set default color and save */
            if (photonView.IsMine)
            {
                SetColor(currentColor);
                /* Save color for other players */
                SaveColorToProperties(currentColor);
                isInitialized = true;
            }
        }
    }

    private void Start()
    {
        /* For other players load color from properties */
        if (!photonView.IsMine)
        {
            LoadColorFromProperties();
        }
    }

    #region Net
    private void SaveColorToProperties(Color color)
    {
        if (!photonView.IsMine) return;

        var props = new ExitGames.Client.Photon.Hashtable();
        props["ColorR"] = color.r;
        props["ColorG"] = color.g;
        props["ColorB"] = color.b;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
    /* Reset properties when leave */
    public override void OnLeftRoom()
    {
        if (!photonView.IsMine) return;

        var props = new ExitGames.Client.Photon.Hashtable();
        props["ColorR"] = null;
        props["ColorG"] = null;
        props["ColorB"] = null;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        /* Reset color to default */
        if (availableColors != null && availableColors.Length > 0)
        {
            SetColor(availableColors[0]);
            currentColorIndex = 0;
        }
    }

    [PunRPC]
    private void SyncColor(float r, float g, float b)
    {
        if (photonView.IsMine) return;

        var newColor = new Color(r, g, b);
        SetColor(newColor);
    }
    #endregion

    #region Color
    private void LoadColorFromProperties()
    {
        if (photonView.Owner == null) return;

        var props = photonView.Owner.CustomProperties;
        if (props.ContainsKey("ColorR") &&
            props.ContainsKey("ColorG") &&
            props.ContainsKey("ColorB") &&
            props["ColorR"] != null &&
            props["ColorG"] != null &&
            props["ColorB"] != null)
        {
            var loadedColor = new Color((float)props["ColorR"],(float)props["ColorG"],(float)props["ColorB"]);
            SetColor(loadedColor);
        }
    }

    public Color GetCurrentColor() => currentColor;

    public void ChangeToNextColor()
    {
        if (availableColors == null || availableColors.Length == 0) return;

        currentColorIndex = (currentColorIndex + 1) % availableColors.Length;
        currentColor = availableColors[currentColorIndex];

        SetColor(currentColor);

        if (photonView.IsMine)
        {
            SaveColorToProperties(currentColor);
            /* Send RPC event like NetMulticast from UE (except self) */
            photonView.RPC(nameof(SyncColor), RpcTarget.Others, currentColor.r, currentColor.g, currentColor.b);
        }
    }

    public void SetColor(Color newColor)
    {
        currentColor = newColor;
        if (playerRenderer != null)
        {
            playerRenderer.material.SetColor(colorProperty, newColor);
            OnColorChanged?.Invoke(newColor);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer == photonView.Owner && !photonView.IsMine)
        {
            if (changedProps.ContainsKey("ColorR") && changedProps["ColorR"] != null &&
                changedProps.ContainsKey("ColorG") && changedProps["ColorG"] != null &&
                changedProps.ContainsKey("ColorB") && changedProps["ColorB"] != null)
            {
                LoadColorFromProperties();
            }
        }
    }
    #endregion
}