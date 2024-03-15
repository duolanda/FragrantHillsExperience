using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectScenicSpotForExperience : MonoBehaviour, InteractionListenerInterface
{
    [Tooltip("Camera used for screen ray-casting. This is usually the main camera.")]
    public Camera screenCamera;

    [Tooltip("The hover icon to show near an scenic spot.")]
    public GameObject hoverDisplay;

    [Tooltip("Index of the player, tracked by the respective InteractionManager. 0 means the 1st player.")]
    public int playerIndex = 0;

    [Tooltip("Whether the left hand interaction is allowed by the respective InteractionManager.")]
    public bool leftHandInteraction = true;

    [Tooltip("Whether the right hand interaction is allowed by the respective InteractionManager.")]
    public bool rightHandInteraction = true;

    [Tooltip("Canvas for this scene")]
    public Canvas canvas;


    private List<GameObject> scenicSpots = new List<GameObject>();
    private GameObject activeScenicSpot;
    private List<GameObject> bodyPanels = new List<GameObject>();


    private InteractionManager interactionManager;

    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;

    private InteractionManager.HandEventType lastHandEvent = InteractionManager.HandEventType.None;
    private Vector3 screenNormalPos = Vector3.zero;

    private string checkGestureName = "";
    private string checkSpotName = "";
    private MyGestureListener gestureListener;
    private Dictionary<string, ScenicSpotInfo> scenicSpotInfos;

    private bool isPanelActive = false;
    private Coroutine panelAutoCloseCoroutine = null;


    public struct ScenicSpotInfo
    {
        public string panelName;
        public string gestureName;
        public string instructionText;

        public ScenicSpotInfo(string panelName, string gestureName, string instructionText)
        {
            this.panelName = panelName;
            this.gestureName = gestureName;
            this.instructionText = instructionText;
        }
    }

    void Awake()
    {
        raycaster = canvas.GetComponent<GraphicRaycaster>();
        eventSystem = canvas.GetComponent<EventSystem>();
    }

    void Start()
    {
        // 添加景点 gameobject
        var scenicSpotsParent = GameObject.Find("Map/ScenicSpots");
        if (scenicSpotsParent != null){
            Transform[] scenicSpotsChildren = scenicSpotsParent.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in scenicSpotsChildren)
            {
                if (child != scenicSpotsParent.transform) // 确保不包括父对象本身
                {
                    scenicSpots.Add(child.gameObject);
                }
            }
        }

        //添加五个 panel
        Transform infoPanelset = canvas.transform.Find("InfoPanelSet");
        Transform[] panelChildren = infoPanelset.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in panelChildren)
        {
            if (child.CompareTag("BodyPanel")) //只添加特定标签的 panel，避免添加 panel 的 children，如 text
            {
                bodyPanels.Add(child.gameObject);
            }
        }

        //初始化字典
        scenicSpotInfos = new Dictionary<string, ScenicSpotInfo>
        {
            { "香炉峰", new ScenicSpotInfo("XLFPanel", "pick", "请做出摘红叶的动作！") },
            { "香雾窟", new ScenicSpotInfo("XWKPanel", "write", "请做出题字的动作！") },
            { "双清别墅", new ScenicSpotInfo("SQBSPanel", "salute", "请做出敬礼的动作！") },
            { "碧云寺", new ScenicSpotInfo("BYSPanel", "wooden_fish", "请做出敲木鱼的动作！") },
            { "香山慈幼院", new ScenicSpotInfo("CYYPanel", "piano", "请做出弹琴的动作！") }
        };

        //获取 interactionManager 实例
        if (interactionManager == null)
        {
            interactionManager = GetInteractionManager();
        }

        if (gestureListener == null)
        {
            gestureListener = GetGestureListener();
        }
}

    void Update()
    {
        HandleHoverDisplay();

        if (checkGestureName != "" && CheckGesture(checkGestureName) && scenicSpotInfos.TryGetValue(checkSpotName, out ScenicSpotInfo info))
        {
            foreach (GameObject panel in bodyPanels)
            {
                if (panel.name == info.panelName)
                {
                    TextMeshProUGUI textComponent = panel.GetComponentInChildren<TextMeshProUGUI>();
                    textComponent.text = "干得漂亮！";
                    StartCoroutine(WaitClose(panel, textComponent, info.instructionText));

                    MyForegroundToRawImage rb = panel.GetComponentInChildren<MyForegroundToRawImage>();
                    rb.playerIndex = playerIndex;
                    break;
                }
            }
        }
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

    private MyGestureListener GetGestureListener()
    {
        MonoBehaviour[] monoScripts = FindObjectsOfType(typeof(MonoBehaviour)) as MonoBehaviour[];

        foreach (MonoBehaviour monoScript in monoScripts)
        {
            if ((monoScript is MyGestureListener) && monoScript.enabled)
            {
                MyGestureListener listener = (MyGestureListener)monoScript;

                if (listener.playerIndex == playerIndex)
                {
                    return listener;
                }
            }
        }
        return null;
    }

    private void HandleBodyPanel()
    {
        if (isPanelActive) return; // 如果已经有激活的面板，不处理光标事件

        foreach (GameObject scenicSpot in scenicSpots )
        {
            if (IsCursorNearObject(scenicSpot) && scenicSpotInfos.TryGetValue(scenicSpot.name, out ScenicSpotInfo info))
            {
                foreach(GameObject panel in bodyPanels)
                {
                    if(panel.name == info.panelName)
                    {
                        panel.SetActive(true);
                        isPanelActive = true;
                        interactionManager.guiHandCursor.gameObject.SetActive(false);
                        checkSpotName = scenicSpot.name;
                        checkGestureName = info.gestureName;

                        if (panelAutoCloseCoroutine != null)
                        {
                            StopCoroutine(panelAutoCloseCoroutine);
                        }
                        panelAutoCloseCoroutine = StartCoroutine(AutoClosePanel(panel, info.instructionText)); //超时自动关闭
                        break;
                    }
                }
                break;
            }
        }

    }

    private void HandleHoverDisplay()
    {
        bool isCursorNearObject = false;

        foreach (GameObject scenicSpot in scenicSpots)
        {
            if (IsCursorNearObject(scenicSpot))
            {
                ShowHoverAt(scenicSpot.transform.position);
                activeScenicSpot = scenicSpot;
                isCursorNearObject = true;
                break;
            }
        }

        if (!isCursorNearObject && activeScenicSpot != null)
        {
            HideHover();
            activeScenicSpot = null;
        }
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

    private void ShowHoverAt(Vector3 position)
    {
        // GUI 元素需要使用屏幕坐标
        Vector3 screenPosition = screenCamera.WorldToScreenPoint(position);

        hoverDisplay.SetActive(true);
        hoverDisplay.transform.position = screenPosition;
    }

    private void HideHover()
    {
        hoverDisplay.SetActive(false);
    }


    IEnumerator WaitClose(GameObject panel, TextMeshProUGUI textComponent, string instructionText)
    {
        if (panelAutoCloseCoroutine != null)
        {
            StopCoroutine(panelAutoCloseCoroutine);
            panelAutoCloseCoroutine = null; // 清除引用
        }

        yield return new WaitForSeconds(3f); // 等待3秒
        panel.SetActive(false);
        isPanelActive = false;
        interactionManager.guiHandCursor.gameObject.SetActive(true);
        textComponent.text = instructionText;
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
                switch (result.gameObject.name)
                {
                    case "MainMenuButton":
                        ReturnToMainMenu();
                        break;
                    default:
                        return;
                }
            }
            else
            {
                return;
            }
        }
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void HandGripDetected(long userId, int userIndex, bool isRightHand, bool isHandInteracting, Vector3 handScreenPos)
    {
        if (!isHandInteracting || !interactionManager)
            return;
        //Debug.Log("userID：" + userId);
        //Debug.Log("interactionManager.GetUserID()：" + interactionManager.GetUserID());
        //Debug.Log("userIndex：" + userIndex);
        //Debug.Log("分割线");

        //if (userId != interactionManager.GetUserID())
        //    return;
       

        lastHandEvent = InteractionManager.HandEventType.Grip;
        //isLeftHandDrag = !isRightHand;
        screenNormalPos = handScreenPos;

        HandleBodyPanel();
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

    public bool CheckGesture(string gesture_name)
    {
        switch (gesture_name)
        {
            case "piano":
                return gestureListener.IsPlayPiano();
            case "pick":
                return gestureListener.IsPickRedLeaf();
            case "salute":
                return gestureListener.IsSalute();
            case "wooden_fish":
                return gestureListener.IsStrikeWoodenFish();
            case "write":
                return gestureListener.IsWriteInAirh();
        }
        return false;
    }

    IEnumerator AutoClosePanel(GameObject panel, string instructionText)
    {
        yield return new WaitForSeconds(60f); // 等待60秒// 如果面板仍然激活，则关闭它if(panel.activeSelf)
        {
            panel.SetActive(false);
            isPanelActive = false;
            interactionManager.guiHandCursor.gameObject.SetActive(true);
            TextMeshProUGUI textComponent = panel.GetComponentInChildren<TextMeshProUGUI>(); if (textComponent != null)
            {
                textComponent.text = instructionText;
            }
        }
    }
}
