using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class GameFlowManager : MonoBehaviour
{
    [Header("Parameters")]
    [Tooltip("Duration of the fade-to-black at the end of the game")]
    public float endSceneLoadDelay = 3f;
    [Tooltip("The canvas group of the fade-to-black screen")]
    public CanvasGroup endGameFadeCanvasGroup;

    [Header("Win")]
    [Tooltip("This string has to be the name of the scene you want to load when winning")]
    public string winSceneName = "WinScene";
    [Tooltip("Duration of delay before the fade-to-black, if winning")]
    public float delayBeforeFadeToBlack = 4f;
    [Tooltip("Duration of delay before the win message")]
    public float delayBeforeWinMessage = 2f;
    [Tooltip("Sound played on win")]
    public AudioClip victorySound;
    [Tooltip("Prefab for the win game message")]
    public GameObject WinGameMessagePrefab;

    [Header("Lose")]
    [Tooltip("This string has to be the name of the scene you want to load when losing")]
    public string loseSceneName = "LoseScene";
    public GameObject floor;

    public bool gameIsEnding { get; private set; }

    PlayerCharacterController m_Player;
    NotificationHUDManager m_NotificationHUDManager;
    ObjectiveManager m_ObjectiveManager;
    float m_TimeEndBsod;
    string m_SceneToLoad;
    bool disableInvincibilityAfterBsod = false;

    void Start()
    {
        m_Player = FindObjectOfType<PlayerCharacterController>();
        DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, GameFlowManager>(m_Player, this);

        m_ObjectiveManager = FindObjectOfType<ObjectiveManager>();
		DebugUtility.HandleErrorIfNullFindObject<ObjectiveManager, GameFlowManager>(m_ObjectiveManager, this);

        AudioUtility.SetMasterVolume(1);
    }

    //FixedUpdate is required to move player (respawn)
    void FixedUpdate()
    {
        if (gameIsEnding)
        {
            float timeRatio = 1 - (m_TimeEndBsod - Time.time) / endSceneLoadDelay;
            endGameFadeCanvasGroup.alpha = timeRatio;
            AudioUtility.SetMasterVolume(1 - timeRatio);

            // See if it's time to respawn
            if (Time.time >= m_TimeEndBsod)
            {
                Health playerHealth = m_Player.GetComponent<Health>();
                playerHealth.Resurrect();
                playerHealth.invincible = true;
                disableInvincibilityAfterBsod = true;
                gameIsEnding = false;
                m_TimeEndBsod = Time.time + endSceneLoadDelay; //when should the fade in complete
            }
        } else if (endGameFadeCanvasGroup.alpha > 0) { //BSOD is active
            float timeRatio = (m_TimeEndBsod - Time.time) / endSceneLoadDelay;
            endGameFadeCanvasGroup.alpha = timeRatio; //1 = BSOD
            AudioUtility.SetMasterVolume(1 - timeRatio);
        } else if (disableInvincibilityAfterBsod) {
            Health playerHealth = m_Player.GetComponent<Health>();
            playerHealth.invincible = false;
            disableInvincibilityAfterBsod = false;
            endGameFadeCanvasGroup.gameObject.SetActive(false);
        }
        else
        {
            // Test if player died
            if (m_Player.isDead) {
                gameIsEnding = true;
                endGameFadeCanvasGroup.gameObject.SetActive(true);
                m_TimeEndBsod = Time.time + endSceneLoadDelay; //when should the fade out complete
            }
        }
    }
}
