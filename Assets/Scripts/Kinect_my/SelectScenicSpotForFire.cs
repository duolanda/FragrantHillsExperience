using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectScenicSpotForFire : MonoBehaviour, InteractionListenerInterface
{
    [Tooltip("Camera used for screen ray-casting. This is usually the main camera.")]
    public Camera screenCamera;

    [Tooltip("Whether the left hand interaction is allowed by the respective InteractionManager.")]
    public bool leftHandInteraction = true;

    [Tooltip("Whether the right hand interaction is allowed by the respective InteractionManager.")]
    public bool rightHandInteraction = true;

    [Tooltip("Canvas for this scene")]
    public Canvas canvas;

    public GameObject introductionPanel; // 玩法介绍面板
    public FireManager fireManager; // 火焰管理器
    public GameObject HandColliders; // 碰撞器
    public GameObject Silhouette; // 剪影

    public GameObject gameOverPanel; // 游戏结束面板
    public TextMeshProUGUI highScoreText; // 高分文本框

    private int playerIndex = 0;

    private InteractionManager interactionManager;

    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;

    private InteractionManager.HandEventType lastHandEvent = InteractionManager.HandEventType.None;
    private Vector3 screenNormalPos = Vector3.zero;

    private Sprite normalButtonSprite; // 正常状态的按钮图
    private Sprite pressedButtonSprite; // 按下状态的按钮图

    private int highScore;
    private bool isOver = false;


    void Awake()
    {
        raycaster = canvas.GetComponent<GraphicRaycaster>();
        eventSystem = canvas.GetComponent<EventSystem>();
    }

    void Start()
    {
        
        //获取 interactionManager 实例
        if (interactionManager == null)
        {
            interactionManager = GetInteractionManager();
        }

        introductionPanel.SetActive(true); 
        fireManager.enabled = false;
        HandColliders.SetActive(false);
        Silhouette.SetActive(false);

        // 加载按钮图标
        normalButtonSprite = Resources.Load<Sprite>("Button/按钮");
        pressedButtonSprite = Resources.Load<Sprite>("Button/按钮-按下");
    }

    void Update()
    {

    }


    private InteractionManager GetInteractionManager()
    {
        MonoBehaviour[] monoScripts = FindObjectsOfType(typeof(MonoBehaviour)) as MonoBehaviour[];

        foreach (MonoBehaviour monoScript in monoScripts)
        {
            if ((monoScript is InteractionManager) && monoScript.enabled)
            {
                InteractionManager manager = (InteractionManager)monoScript;

                if (manager.playerIndex == playerIndex && manager.leftHandInteraction == leftHandInteraction && manager.rightHandInteraction == rightHandInteraction)
                {
                    return manager;
                }
            }
        }
        return null;
    }


   

    private bool IsCursorNearObject(GameObject obj)
    {
        CircleCollider2D collider = obj.GetComponent<CircleCollider2D>();
        if (collider == null) return false;

        Vector3 cursorPosition = GetCursorPosition();
        return collider.bounds.Contains(cursorPosition);
    }

    private Vector3 GetCursorPosition()
    {

        // 默认获取右手位置:
        Vector3 screenNormalPos = interactionManager.GetRightHandScreenPos();
        Vector3 screenPixelPos = new Vector3(
            screenNormalPos.x * (screenCamera ? screenCamera.pixelWidth : Screen.width),
            screenNormalPos.y * (screenCamera ? screenCamera.pixelHeight : Screen.height),
            screenCamera.nearClipPlane
        );
        Vector3 worldPos = screenCamera.ScreenToWorldPoint(screenPixelPos);
        return new Vector3(worldPos.x, worldPos.y, 0.0f);
    }


    private void HandleButton()
    {
        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = screenCamera.WorldToScreenPoint(GetCursorPosition());
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);


        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("Button"))
            {
                if(result.gameObject.name == "StartButton")
                {
                    ChangeButtonSprite(result.gameObject, pressedButtonSprite);
                    StartGame();
                }

                if (isOver)
                {
                    if (result.gameObject.name == "RetryButton")
                    {
                        ChangeButtonSprite(result.gameObject, pressedButtonSprite);
                        RetryGame();
                    }
                    else if (result.gameObject.name == "MainMenuButton")
                    {
                        ChangeButtonSprite(result.gameObject, pressedButtonSprite);
                        ReturnToMainMenu();
                    }
                }
                StartCoroutine(ResetButtonSprite(result.gameObject, normalButtonSprite));

            }
            else
            {
                return;
            }
        }
    }


    private void StartGame()
    {
        introductionPanel.SetActive(false); 
        fireManager.enabled = true;
        HandColliders.SetActive(true);
        Silhouette.SetActive(true);
        interactionManager.guiHandCursor.gameObject.SetActive(false);

        fireManager.gameStarted = true;
        fireManager.StartFire();
    }

    public void GameOver(int score)
    {
        isOver = true;
        fireManager.enabled = false;
        HandColliders.SetActive(false);
        Silhouette.SetActive(false);
        interactionManager.guiHandCursor.gameObject.SetActive(true);

        LoadHighScore();

        gameOverPanel.SetActive(true);
        fireManager.gameStarted = false;

        if (score > highScore)
        {
            highScore = score;
            SaveHighScore(highScore);
        }

        highScoreText.text = "最高分数: " + highScore;
    }


    private void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private void SaveHighScore(int score)
    {
        PlayerPrefs.SetInt("HighScore", score);
        PlayerPrefs.Save();
    }

    private IEnumerator ResetButtonSprite(GameObject button, Sprite normalSprite)
    {
        yield return new WaitForSeconds(0.2f); // 等待一段时间
        ChangeButtonSprite(button, normalButtonSprite); // 恢复正常状态的图片
    }

    private void ChangeButtonSprite(GameObject button, Sprite newSprite)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.sprite = newSprite;
        }
    }


    public void HandGripDetected(long userId, int userIndex, bool isRightHand, bool isHandInteracting, Vector3 handScreenPos)
    {
        if (!isHandInteracting || !interactionManager)
            return;
       

        lastHandEvent = InteractionManager.HandEventType.Grip;
        //isLeftHandDrag = !isRightHand;
        screenNormalPos = handScreenPos;

        HandleButton();

    }

    public void HandReleaseDetected(long userId, int userIndex, bool isRightHand, bool isHandInteracting, Vector3 handScreenPos)
    {
        if (!isHandInteracting || !interactionManager)
            return;
        if (userId != interactionManager.GetUserID())
            return;

        lastHandEvent = InteractionManager.HandEventType.Release;
        //isLeftHandDrag = !isRightHand;
        screenNormalPos = handScreenPos;
    }

    public bool HandClickDetected(long userId, int userIndex, bool isRightHand, Vector3 handScreenPos)
    {
        return true;
    }

}
