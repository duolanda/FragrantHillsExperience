﻿using System.Collections;
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
    private List<GameObject> characters = new List<GameObject>();
    private List<GameObject> afterChanges = new List<GameObject>();


    private InteractionManager interactionManager;

    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;

    private InteractionManager.HandEventType lastHandEvent = InteractionManager.HandEventType.None;
    private Vector3 screenNormalPos = Vector3.zero;

    private string checkGestureName = "";
    private string checkSpotName = "";
    private MyGestureListener gestureListener;
    private Dictionary<string, ScenicSpotInfo> scenicSpotInfos;

    private Sprite normalButtonSprite; // 正常状态的按钮图
    private Sprite pressedButtonSprite; // 按下状态的按钮图

    private bool isPanelActive = false;
    private Coroutine panelAutoCloseCoroutine = null;
    private bool allowGestureCheck = true;


    public struct ScenicSpotInfo
    {
        public string panelName;
        public string gestureName;
        public string instructionText;
        public string characterName;

        public ScenicSpotInfo(string panelName, string gestureName, string instructionText, string characterName)
        {
            this.panelName = panelName;
            this.gestureName = gestureName;
            this.instructionText = instructionText;
            this.characterName = characterName;
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
        var scenicSpotsParent = GameObject.Find("ScenicSpots");
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

        //添加五个角色
        Transform charactersParent = canvas.transform.Find("Characters");
        Transform[] characterChildren = charactersParent.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in characterChildren)
        {
            if (child != charactersParent.transform)
            {
                characters.Add(child.gameObject);
            }
        }

        //添加五个变化后的状态
        Transform afterChangesParent = canvas.transform.Find("AfterChanges");
        Transform[] afterChangesChildren = afterChangesParent.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in afterChangesChildren)
        {
            if (child != afterChangesParent.transform)
            {
                afterChanges.Add(child.gameObject);
            }
        }

        //初始化字典
        scenicSpotInfos = new Dictionary<string, ScenicSpotInfo>
        {
            { "香炉峰", new ScenicSpotInfo("XLFPanel", "pick", "请做出摘红叶的动作！", "Tourist") },
            { "香雾窟", new ScenicSpotInfo("XWKPanel", "write", "请做出题字的动作！", "Emperor") },
            { "双清别墅", new ScenicSpotInfo("SQBSPanel", "salute", "请做出敬礼的动作！", "Chairman") },
            { "碧云寺", new ScenicSpotInfo("BYSPanel", "wooden_fish", "请做出敲木鱼的动作！", "Monk") },
            { "香山慈幼院", new ScenicSpotInfo("CYYPanel", "piano", "请做出弹琴的动作！", "Student") }
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

        // 加载按钮图标
        normalButtonSprite = Resources.Load<Sprite>("Button/按钮");
        pressedButtonSprite = Resources.Load<Sprite>("Button/按钮-按下");
    }

    void Update()
    {
        HandleHoverDisplay();

        if (allowGestureCheck && checkGestureName != "" 
            && CheckGesture(checkGestureName) 
            && scenicSpotInfos.TryGetValue(checkSpotName, out ScenicSpotInfo info))
        {
            foreach (GameObject panel in bodyPanels)
            {
                if (panel.name == info.panelName)
                {
                    if (panelAutoCloseCoroutine != null)
                    {
                        StopCoroutine(panelAutoCloseCoroutine);
                        panelAutoCloseCoroutine = null; // 清除引用
                    }

                    GameObject character = characters.Find(obj => obj.name == info.characterName);
                    GameObject characterAfter = afterChanges.Find(obj => obj.name == info.characterName + "After");

                    if(checkSpotName == "香山慈幼院")
                    {
                        character.SetActive(false);
                        characterAfter.SetActive(true);
                    }
                    else
                    {
                        characterAfter.SetActive(true);
                    }
                    StartCoroutine(RestoreCharacter(character, characterAfter, checkSpotName)); //5s后恢复


                    panel.SetActive(false);
                    isPanelActive = false;
                    interactionManager.guiHandCursor.gameObject.SetActive(true);

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

                        allowGestureCheck = false;
                        StartCoroutine(EnableGestureCheckAfterDelay(3)); //3s后再比对手势

                        MyForegroundToRawImage fg = panel.GetComponentInChildren<MyForegroundToRawImage>();
                        fg.playerIndex = playerIndex; //设置成对应的 player id

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
        return new Vector3(screenPixelPos.x, screenPixelPos.y, 0.0f);
    }

    private void ShowHoverAt(Vector3 position)
    {
        hoverDisplay.SetActive(true);
        hoverDisplay.transform.position = position;
    }

    private void HideHover()
    {
        hoverDisplay.SetActive(false);
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

    IEnumerator EnableGestureCheckAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        allowGestureCheck = true;
    }

    IEnumerator RestoreCharacter(GameObject character, GameObject characterAfter, string spotName)
    {
        yield return new WaitForSeconds(5); // 等待5秒

        if (spotName == "香山慈幼院")
        {
            character.SetActive(true);
            characterAfter.SetActive(false);
        }
        characterAfter.SetActive(false);
    }

    private void HandleButton()
    {
        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = GetCursorPosition();
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("Button"))
            {
                switch (result.gameObject.name)
                {
                    case "MainMenuButton":
                        ChangeButtonSprite(result.gameObject, pressedButtonSprite);
                        ReturnToMainMenu();
                        break;
                    default:
                        return;
                }
                StartCoroutine(ResetButtonSprite(result.gameObject, normalButtonSprite));
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
        }
    }
}
