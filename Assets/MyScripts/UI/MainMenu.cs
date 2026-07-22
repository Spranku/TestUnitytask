using Photon.Pun;
using Photon.Pun.Demo.Cockpit.Forms;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;

public class MainMenu : MonoBehaviourPunCallbacks
{
    [Header("Buttons")]
    [SerializeField]
    public Button quitButton;
    [SerializeField]
    public Button playButton;
    [SerializeField]
    public Button closeButton;
    [SerializeField]
    public Button hostButton;
    [SerializeField]
    public Button clientButton;
    [SerializeField]
    public TMP_InputField playerNicknameText;

    [Header("VerticalGroups")]
    [SerializeField]
    public VerticalLayoutGroup mainGroup;
    [SerializeField]
    public VerticalLayoutGroup listGroup;
    [SerializeField]
    public VerticalLayoutGroup listGroupRooms;
    [SerializeField] 
    public HorizontalLayoutGroup roomHorizontalGroup;

    [Header("Animations")]
    [SerializeField]
    public AnimationClip openMainAnimation;
    [SerializeField]
    public AnimationClip closeMainAnimation;
    [SerializeField]
    public AnimationClip ListAnimation;

    [Header("Room Button Prefab")]
    [SerializeField] public GameObject roomButtonPrefab;

    private HorizontalLayoutGroup horizontalGroupGameMode;
    private byte currentPlayers;
    private byte maxPlayers;
    private string playerNicknameString;
    private bool pendingCreateRoom = false;

    /* Cache */
    private Dictionary<string, RoomInfo> roomList = new Dictionary<string, RoomInfo>();
    private List<GameObject> roomUIElements = new List<GameObject>();

    protected void Awake()
    {
        /* Disable play button at start*/
        if (playButton) playButton.gameObject.SetActive(false);

        /* Disable HOST & FIND buttons at start*/
        horizontalGroupGameMode = hostButton.GetComponentInParent<HorizontalLayoutGroup>();
        if(horizontalGroupGameMode) horizontalGroupGameMode.gameObject.SetActive(false);
    }

    void Start()
    {
        /* Photon will loading scenes */
        PhotonNetwork.AutomaticallySyncScene = true;

        /* Check connection state */
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("MainMenu::Start - PHOTON: Try to connect...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("MainMenu::Start - PHOTON: Already connected");
            PhotonNetwork.JoinLobby();
        }
    }

    /* Buttons */
    public void OnQuitButton() 
    {
        ///Debug.Log("MainMenu::OnQuitButton - Success quit");
        Application.Quit(); 
    }

    public void OnPlayButton() 
    { 
        /* TODO: Single game? */
        PhotonNetwork.LoadLevel(1);
    }

    public void OnCloseButton()
    {
        if(listGroup.IsActive())
        {
            var animComp = listGroup.GetComponent<Animator>();
            if (animComp)
            {
                animComp.Play(ListAnimation.name, 0, animComp.speed);
                /* Launch reverse animation for list of rooms */
                StartCoroutine(PlayAnimationReverse(animComp, ListAnimation.name,listGroup));
            }
        }

        /* User can change nickname before choice game mode */
        playerNicknameText.interactable = true;
    }

    public void OnHostButton()
    {
        ///Debug.Log("MainMenu::OnHostButton - OnHostButton pressed");

        /* Can`t change nickname when user choiced game mode */
        playerNicknameText.interactable = false;

        PhotonNetwork.NickName = playerNicknameString;

        /* Setup&create room settings */
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4; // TODO: variable?
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;

        if(PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.CreateRoom("TestRoom", roomOptions, TypedLobby.Default);
        }
    }

    public void OnClientButton()
    {
       ///Debug.Log("MainMenu::OnClientButton - OnClientButton pressed");

        /* Can`t change nickname when user choiced game mode */
        playerNicknameText.interactable = false;

        /* Open list of rooms */
        if (listGroup)
        {
            listGroup.gameObject.SetActive(true);

            /* Clear all rows */
            ClearRoomUI();
            roomList.Clear();

            /* Trying to create room row */
            if (PhotonNetwork.IsConnectedAndReady)
            {
                if (!PhotonNetwork.InLobby)
                {
                    ///Debug.Log("MainMenu::OnClientButton - Joint lobby for search any rooms...");
                    PhotonNetwork.JoinLobby();
                }
                else
                {
                    ///Debug.Log("MainMenu::OnClientButton - Update list of rooms (leave&join again)");
                    PhotonNetwork.LeaveLobby();
                    PhotonNetwork.JoinLobby();
                }
            }
        }
    }

    private void OnRoomButtonClicked(string roomName)
    {
        Debug.Log("MainMenu::OnRoomButtonClicked - Room button " + roomName + " pressed");

        if (string.IsNullOrEmpty(playerNicknameString))
        {
            ///Debug.LogError("MainMenu::OnRoomButtonClicked - Nickname wrong");
            return;
        }

        PhotonNetwork.NickName = playerNicknameString;
        PhotonNetwork.JoinRoom(roomName);
    }

    public void OnPlayerNicknameChanged()
    {
        if (!horizontalGroupGameMode) return;

        var userNickname = playerNicknameText.text;
        const byte minCountOfSymbols = 4;
        const byte maxCountOfSymbols = 15;

        /* Check empty */
        if (userNickname.Contains(" "))
        {
            playerNicknameText.text = userNickname.Replace(" ", "");
            playButton.gameObject.SetActive(false);
            horizontalGroupGameMode.gameObject.SetActive(false);
            return;
        }

        /* Check less*/
        if(userNickname.Length < minCountOfSymbols)
        {
            playButton.gameObject.SetActive(false);
            horizontalGroupGameMode.gameObject.SetActive(false);
            return;
        }

        /* Check more */
        if (userNickname.Length > maxCountOfSymbols)
        {
            playerNicknameText.text = userNickname.Substring(0, maxCountOfSymbols);
            playButton.gameObject.SetActive(false);
            horizontalGroupGameMode.gameObject.SetActive(false);
            return; 
        }

        /* Enable HOST&FIND buttons */
        if (userNickname.Length >= minCountOfSymbols)
        {
            playerNicknameString = playerNicknameText.text.ToString();

            playButton.gameObject.SetActive(true);
            horizontalGroupGameMode.gameObject.SetActive(true);
        }
    }

    /* Network */
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("MainMenu::OnRoomListUpdate - List of rooms: " + roomList.Count + " rooms");

        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                if (this.roomList.ContainsKey(room.Name))
                {
                    this.roomList.Remove(room.Name);
                }
                continue;
            }

            if (this.roomList.ContainsKey(room.Name))
            {
                this.roomList[room.Name] = room;
            }
            else
            {
                this.roomList.Add(room.Name, room);
            }
        }
        /* Show rows in UI */
        DisplayRooms();
    }

    public override void OnCreatedRoom()
    {
        ///Debug.Log("MainMenu::OnCreatedRoom - The room" + PhotonNetwork.CurrentRoom.Name + " was created!");
        PhotonNetwork.LoadLevel(1);
    }

    public override void OnJoinedRoom()
    {
        ///Debug.Log("MainMenu::OnJoinedRoom - Success join to" + PhotonNetwork.CurrentRoom.Name + " room");

        foreach (var player in PhotonNetwork.CurrentRoom.Players)
        {
            Debug.Log($"  - {player.Value.NickName} (ID: {player.Key})");
        }
    }

    public override void OnConnectedToMaster()
    {
        ///Debug.Log("MainMenu::OnConnectedToMaster - Success connect tot master-server Photon");
        PhotonNetwork.JoinLobby();
    }

    ///public override void OnDisconnected(DisconnectCause cause) { Debug.Log("MainMenu::OnDisconnected - Disconected from Photon, error: " + cause); }

    ///public override void OnJoinRandomFailed(short returnCode, string message) { Debug.Log("MainMenu::OnJoinRandomFailed - there are no any open rooms"); }

    ///public override void OnJoinRoomFailed(short returnCode, string message) { Debug.Log("MainMenu::OnJoinRoomFailed - Connected to room failed, error code: " + returnCode + " Message: " + message); }
    
    /* UI */
    private void DisplayRooms()
    {
        ClearRoomUI();

        if (roomList.Count == 0)
        {
            Debug.Log("MainMenu::DisplayRooms - roomList empty");
            return;
        }

        /* Create row for every room */
        foreach (var roomEntry in roomList)
        {
            RoomInfo room = roomEntry.Value;
            CreateRoomUI(room);
        }
        Debug.Log("MainMenu::DisplayRooms - Show " + roomList.Count + " rooms");
    }

    private void CreateRoomUI(RoomInfo room)
    {
        if (roomHorizontalGroup == null)
        {
            Debug.Log("MainMenu::CreateRoomUI - roomHorizontalGroup prefab are missing");
            return;
        }

        if (listGroupRooms == null)
        {
            Debug.Log("MainMenu::CreateRoomUI - listGroupRooms are missing");
            return;
        }

        /* Create a copy */
        GameObject roomElement = Instantiate(roomHorizontalGroup.gameObject, listGroupRooms.transform);
        roomElement.name = $"Room_{room.Name}";

        /* Button settings */
        Button roomButton = roomElement.GetComponentInChildren<Button>();
        if (roomButton != null)
        {
            string roomName = room.Name;
            roomButton.onClick.RemoveAllListeners();
            roomButton.onClick.AddListener(() => OnRoomButtonClicked(roomName));

            /* Change color of the button */
            TextMeshProUGUI buttonText = roomButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = room.Name;
            }
        }

        /* Count of players settings */
        TextMeshProUGUI[] allTexts = roomElement.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text.transform.parent != roomButton?.transform)
            {
                text.text = $"{room.PlayerCount}/{room.MaxPlayers}";
            }
        }

        /* Saving for clearing */
        roomUIElements.Add(roomElement);

        ///Debug.Log("MainMenu::CreateRoomUI - Was created row for room: " + room.Name + " cur: " + room.PlayerCount + " max:" + room.MaxPlayers);
    }

    private void ClearRoomUI()
    {
        foreach (GameObject element in roomUIElements)
        {
            if (element != null)
                Destroy(element);
        }
        roomUIElements.Clear();

        /* Additional clering */
        foreach (Transform child in listGroupRooms.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private IEnumerator PlayAnimationReverse(Animator anim, string animationName, VerticalLayoutGroup groupToClose)
    {
        float animationLength = anim.GetCurrentAnimatorStateInfo(0).length;
        float currentTime = animationLength;

        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            float normalizedTime = currentTime / animationLength;
            anim.Play(animationName, 0, normalizedTime);
            yield return null;
        }
        /* Disable list of rooms */
        groupToClose.gameObject.SetActive(false);
        anim.Play(animationName, 0, 0f);
    }
}
