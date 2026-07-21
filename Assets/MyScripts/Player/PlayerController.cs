using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.Cockpit;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    public float moveSpeed = 5.0f;

    [Header("Color")]
    [SerializeField]
    public Color[] colors;

    private PhotonView photonView;
    private Renderer playerRenderer;
    private int currentColorIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        /* Initialize comps */
        photonView = GetComponent<PhotonView>();

        playerRenderer = GetComponent<Renderer>();

        /* Do nothing? */
        if (!photonView.IsMine) Destroy(GetComponent<Rigidbody>()); 
    }

    // Update is called once per frame
    void Update()
    {
        /* Locally controlled */
        if (!photonView.IsMine) return;

        /* Simple movement */
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        var Direction = new Vector3(horizontal, 0, vertical).normalized;
        transform.Translate(Direction * moveSpeed * Time.deltaTime, Space.World);

        /* Colorize */
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (colors == null || colors.Length == 0)
            {
                Debug.LogWarning("Массив цветов пуст! Цвет не меняется.");
                return;
            }
            currentColorIndex = (currentColorIndex + 1) % colors.Length;
            Color newColor = colors[currentColorIndex];
            playerRenderer.material.color = newColor;

            /* RPC like multicast from UE */
            photonView.RPC("SyncColor", RpcTarget.Others, newColor.r, newColor.g, newColor.b);
        }

        [PunRPC]
        void SyncColor(float r, float g, float b)
        {
            playerRenderer.material.color = new Color(r, g, b);
        }
    }
}
