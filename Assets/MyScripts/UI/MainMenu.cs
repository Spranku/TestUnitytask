using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;
using WebSocketSharp;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField]
    public Button quitButton;
    [SerializeField]
    public Button playButton;
    [SerializeField]
    public Button closeButton;
    [SerializeField]
    public Button roomButton;
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

    [Header("Count of players")]
    [SerializeField]
    public TextMeshProUGUI currentPlayersText;
    [SerializeField]
    public TextMeshProUGUI maxPlayersText;

    [Header("Animations")]
    [SerializeField]
    public AnimationClip openMainAnimation;
    [SerializeField]
    public AnimationClip closeMainAnimation;
    [SerializeField]
    public AnimationClip ListAnimation;

    private HorizontalLayoutGroup horizontalGroupGameMode;
    private byte currentPlayers;
    private byte maxPlayers;
    private string playerNicknameString;

    protected void Awake()
    {
        if (playButton) playButton.gameObject.SetActive(false);

        horizontalGroupGameMode = hostButton.GetComponentInParent<HorizontalLayoutGroup>();
        if(horizontalGroupGameMode) horizontalGroupGameMode.gameObject.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void OnQuitButton() 
    {
        Debug.Log("MainMenu::OnQuitButton - Success quit");
        Application.Quit(); 
    }

    public void OnPlayButton() 
    { 
        SceneManager.LoadScene(1); 
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

    public void OnPlayerNicknameChanged()
    {
        if (!horizontalGroupGameMode) return;

        var userNickname = playerNicknameText.text;
        const byte minCountOfSymbols = 4;
        const byte maxCountOfSymbols = 15;

        if (userNickname.Contains(" "))
        {
            playerNicknameText.text = userNickname.Replace(" ", "");
            playButton.gameObject.SetActive(false);
            horizontalGroupGameMode.gameObject.SetActive(false);
            return;
        }

        if(userNickname.Length < minCountOfSymbols)
        {
            playButton.gameObject.SetActive(false);
            horizontalGroupGameMode.gameObject.SetActive(false);
            return;
        }

        if (userNickname.Length > maxCountOfSymbols)
        {
            playerNicknameText.text = userNickname.Substring(0, maxCountOfSymbols);
            playButton.gameObject.SetActive(false);
            horizontalGroupGameMode.gameObject.SetActive(false);
            return; 
        }

        if (userNickname.Length >= minCountOfSymbols)
        {
            playerNicknameString = playerNicknameText.text.ToString();

            playButton.gameObject.SetActive(true);
            horizontalGroupGameMode.gameObject.SetActive(true);
        }
    }

    public void OnHostButton()
    {
        Debug.Log("OnHostButtonPressed");
        playerNicknameText.interactable = false;
    }

    public void OnClientButton()
    {
        Debug.Log("OnHostButtonPressed");
        playerNicknameText.interactable = false;
        if (listGroup) listGroup.gameObject.SetActive(true);

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
