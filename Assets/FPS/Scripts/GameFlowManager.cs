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
    // public NavMeshSurface navMesh;


    public bool gameIsEnding { get; private set; }

    PlayerCharacterController m_Player;
    NotificationHUDManager m_NotificationHUDManager;
    ObjectiveManager m_ObjectiveManager;
    float m_TimeLoadEndGameScene;
    string m_SceneToLoad;
    SaveLoad saveLoad = new SaveLoad();
    float secondsSinceSave = 0;

    void Start()
    {
        //Debug.Log("Start() endGameFadeCanvasGroup.alpha="+endGameFadeCanvasGroup.alpha);

        m_Player = FindObjectOfType<PlayerCharacterController>();
        DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, GameFlowManager>(m_Player, this);

        m_ObjectiveManager = FindObjectOfType<ObjectiveManager>();
		DebugUtility.HandleErrorIfNullFindObject<ObjectiveManager, GameFlowManager>(m_ObjectiveManager, this);

        AudioUtility.SetMasterVolume(1);

        saveLoad.Load();
        // GenerateLevel();
    }

    //FixedUpdate is required to move player (respawn)
    void FixedUpdate()
    {
        // //Debug.Log("FixedUpdate(): m_Player.isDead = "+m_Player.isDead);

        if (gameIsEnding)
        {
            //Debug.Log("gameIsEnding / fading out gameIsEnding="+gameIsEnding+" m_Player.isDead="+m_Player.isDead);
            float timeRatio = 1 - (m_TimeLoadEndGameScene - Time.time) / endSceneLoadDelay;
            endGameFadeCanvasGroup.alpha = timeRatio;

            AudioUtility.SetMasterVolume(1 - timeRatio);

            // See if it's time to respawn
            if (Time.time >= m_TimeLoadEndGameScene)
            {
            //Debug.Log("Respawning gameIsEnding="+gameIsEnding+" m_Player.isDead="+m_Player.isDead);
                // SceneManager.LoadScene(m_SceneToLoad);
                UnityEngine.AI.NavMeshHit hit; // NavMesh Sampling Info Container
                // var playerPos = m_Player.transform.position;
                // from camera position find a nearest point on NavMesh surface in range of maxDistance
                UnityEngine.AI.NavMesh.SamplePosition(
                    m_Player.transform.position, out hit, Mathf.Infinity, UnityEngine.AI.NavMesh.AllAreas);
                m_Player.transform.position = hit.position;
                Health playerHealth = m_Player.GetComponent<Health>();
                // playerHealth.Heal(100);
                // playerHealth.m_IsDead = false;
                playerHealth.Resurrect();
                gameIsEnding = false;
                m_TimeLoadEndGameScene = Time.time + endSceneLoadDelay; //when should the fade in complete
            }
        } else if (endGameFadeCanvasGroup.alpha > 0) { //BSOD is active
            // //Debug.Log("Fading back in gameIsEnding="+gameIsEnding+" m_Player.isDead="+m_Player.isDead);
            float timeRatio = (m_TimeLoadEndGameScene - Time.time) / endSceneLoadDelay;
            endGameFadeCanvasGroup.alpha = timeRatio; //1 = BSOD

            AudioUtility.SetMasterVolume(1 - timeRatio);
        }
        else
        {
            if (m_ObjectiveManager.AreAllObjectivesCompleted())
                EndGame(true);

            // Test if player died
            if (m_Player.isDead) {
                EndGame(false);
            }

            secondsSinceSave += Time.deltaTime;
            if (secondsSinceSave > 10) {
                secondsSinceSave = 0;
                saveLoad.Save();
                // GenerateLevel();
            }
        }
    }

    // void GenerateLevel() {
    //     GameObject levelContainer = GameObject.Find("Level");
    //     GameObject player = GameObject.Find("Player");
    //     Vector3 playerPos = player.transform.position;
    //     int x = (int)playerPos.x;
    //     int y = (int)playerPos.y;
    //     int z = (int)playerPos.z;
    //     string name = "floor "+x+" , "+y+" , "+z;
    //     if (!GameObject.Find(name)) {
    //         GameObject newFloor = Instantiate(floor, new Vector3(x,y,z), Quaternion.identity, levelContainer.transform);
    //         newFloor.name = name;
    //         navMesh.BuildNavMesh();
    //     }
    // }

    void EndGame(bool win)
    {
        // unlocks the cursor before leaving the scene, to be able to click buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Remember that we need to load the appropriate end scene after a delay
        gameIsEnding = true;
        endGameFadeCanvasGroup.gameObject.SetActive(true);
        // if (win)
        // {
        //     m_SceneToLoad = winSceneName;
        //     m_TimeLoadEndGameScene = Time.time + endSceneLoadDelay + delayBeforeFadeToBlack;

        //     // play a sound on win
        //     var audioSource = gameObject.AddComponent<AudioSource>();
        //     audioSource.clip = victorySound;
        //     audioSource.playOnAwake = false;
        //     audioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.HUDVictory);
        //     audioSource.PlayScheduled(AudioSettings.dspTime + delayBeforeWinMessage);

        //     // create a game message
        //     var message = Instantiate(WinGameMessagePrefab).GetComponent<DisplayMessage>();
        //     if (message)
        //     {
        //         message.delayBeforeShowing = delayBeforeWinMessage;
        //         message.GetComponent<Transform>().SetAsLastSibling();
        //     }
        // }
        // else
        // {
            // m_SceneToLoad = loseSceneName;
            m_TimeLoadEndGameScene = Time.time + endSceneLoadDelay; //when should the fade out complete
        // }
    }
}
