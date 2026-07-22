using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerColor))]
public class PlayerController : MonoBehaviourPun
{
    [Header("References")]
    [SerializeField] 
    private PlayerMovement movement;
    [SerializeField] 
    private PlayerColor color;
    [SerializeField] 
    private PhotonView myPhotonView;

    [Header("Network Settings")]
    [SerializeField] 
    private bool destroyRigidbodyOnRemote = true;

    [Header("Debug")]
    [SerializeField] 
    private bool enableDebugLogs = false;

    private Vector2 movementInput;
    private bool jumpPressed;
    private bool colorChangePressed;

    private void Awake()
    {
        ValidateComponents();
        InitNetwork();
    }

    private void ValidateComponents()
    {
        if (movement == null) movement = GetComponent<PlayerMovement>();

        if (color == null) color = GetComponent<PlayerColor>();

        if (myPhotonView == null) myPhotonView = GetComponent<PhotonView>();
    }

    private void InitNetwork()
    {
        if (!myPhotonView.IsMine)
        {
            /* Clear garbage comps from other players */
            if (destroyRigidbodyOnRemote)
            {
                var rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Destroy(rb);
                }
            }

            /* Disable audio same! */
            var listener = GetComponentInChildren<AudioListener>();
            if (listener != null)
            {
                listener.enabled = false;
            }
        }
    }

    #region Update Loop
    private void Update()
    {
        /* Only local */
        if (!myPhotonView.IsMine) return;

        ReadInput();
        HandleMovement();
        HandleJump();
        HandleColorChange();
    }
    #endregion

    #region Input Handling
    private void ReadInput()
    {
        movementInput = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );

        if (movementInput.magnitude > 1f)
        {
            movementInput.Normalize();
        }
        jumpPressed = Input.GetButtonDown("Jump");
        colorChangePressed = Input.GetKeyDown(KeyCode.Space);
    }
    #endregion

    #region Movement Handling
    private void HandleMovement()
    {
        if (movement == null) return;
        movement.Move(movementInput);
    }

    private void HandleJump()
    {
        if (!jumpPressed || movement == null) return;
        movement.Jump();
    }
    #endregion

    #region Color Handling
    private void HandleColorChange()
    {
        if (!colorChangePressed || color == null) return;
        color.ChangeToNextColor();
    }
    #endregion

    #region Public API
    public PlayerMovement GetMovement() => movement;
    public PlayerColor GetColor() => color;

    public void SetControlEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled)
        {
            movementInput = Vector2.zero;
            jumpPressed = false;
            colorChangePressed = false;
        }
    }
    #endregion
}