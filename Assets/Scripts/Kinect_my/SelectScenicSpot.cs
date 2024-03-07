using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectScenicSpot : MonoBehaviour, InteractionListenerInterface
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

    [Tooltip("Panel for showing scenic spot informaiton.")]
    public GameObject infoPanelPrefab;

    [Tooltip("Canvas for this scene")]
    public Canvas canvas;

    public GameObject selectionIndicatorPrefab;

    private List<GameObject> scenicSpots = new List<GameObject>();
    private GameObject activeScenicSpot;

    private InteractionManager interactionManager;
    private MapGestureListener gestureListener;
    private ScenicSpotsManager scenicSpotsManager;

    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;

    private InteractionManager.HandEventType lastHandEvent = InteractionManager.HandEventType.None;
    private Vector3 screenNormalPos = Vector3.zero;
    private bool isWaitingClose = false;
    private List<GameObject> selectedScenicSpots = new List<GameObject>(); //存储已选择的景点
    private Dictionary<GameObject, GameObject> selectionIndicators = new Dictionary<GameObject, GameObject>(); // 存储景点和其对应的选中指示器

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

        //获取 interactionManager 实例
        if (interactionManager == null)
        {
            interactionManager = GetInteractionManager();
        }

        scenicSpotsManager = ScenicSpotsManager.Instance;
        //gestureListener = MapGestureListener.Instance;

    }

    void Update()
    {
        HandleHoverDisplay();
        HandlePanelClose();
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

    private void HandleInfoPanel()
    {
        foreach (GameObject scenicSpot in scenicSpots)
        {
            if (IsCursorNearObject(scenicSpot))
            {
                CreatePanelAt(scenicSpot.transform.position, scenicSpot.name);
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

    private void HandlePanelClose()
    {
        //检测按下动作，注意，目前只检测右手
        if (!interactionManager.IsRightHandPress() || isWaitingClose)
        {
            return;
        }

        List<RaycastResult> results = new List<RaycastResult>();
        PointerEventData pointerEventData = new PointerEventData(eventSystem);

        pointerEventData.position = screenCamera.WorldToScreenPoint(GetCursorPosition());
        raycaster.Raycast(pointerEventData, results);

        foreach (RaycastResult result in results)
        {
            // 检测到的UI元素
            GameObject hitObject = result.gameObject;

            // 通过标签检查是否是面板
            if (hitObject.CompareTag("InfoPanel"))
            {
                //hitObject.SetActive(false);
                Destroy(hitObject);
                StartCoroutine(WaitBeforeNextClose()); // 等2s再关下一个，避免连续关闭
                break;
            }
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


    private void CreatePanelAt(Vector3 position, string scenicSpotName)
    {
        //创建面板
        Vector3 screenPosition = screenCamera.WorldToScreenPoint(position);
        GameObject infoPanel = Instantiate(infoPanelPrefab, screenPosition, Quaternion.identity);
        Transform infoPanelset = canvas.transform.Find("InfoPanelSet");
        infoPanel.transform.SetParent(infoPanelset, true); //创建在特定物件下以保证显示层级

        //修改景点名称
        Transform ScenicNameTransform = infoPanel.transform.Find("ScenicNameText");
        TextMeshProUGUI ScenicNameText = ScenicNameTransform.GetComponent<TextMeshProUGUI>();
        ScenicNameText.text = scenicSpotName;
    }

    IEnumerator WaitBeforeNextClose()
    {
        isWaitingClose = true; 
        yield return new WaitForSeconds(2f); // 等待2秒
        isWaitingClose = false; 
    }


    private void ToggleScenicSpotSelection()
    {
        foreach (GameObject scenicSpot in scenicSpots)
        {
            if (IsCursorNearObject(scenicSpot))
            {
                if (selectedScenicSpots.Contains(scenicSpot))
                {
                    selectedScenicSpots.Remove(scenicSpot);
                    DeselectAScenicSpot(scenicSpot);
                }
                else
                {
                    selectedScenicSpots.Add(scenicSpot);
                    SelectAScenicSpot(scenicSpot);
                }
                break;
            }
        }
    }

    //选择了某个景点
    private void SelectAScenicSpot(GameObject scenicSpot)
    {
        Vector3 screenPosition = screenCamera.WorldToScreenPoint(scenicSpot.transform.position);
        GameObject indicator = Instantiate(selectionIndicatorPrefab, screenPosition, Quaternion.identity, canvas.transform);
        selectionIndicators[scenicSpot] = indicator; // 存储指示器引用
    }

    //取消选择某个景点
    private void DeselectAScenicSpot(GameObject scenicSpot)
    {
        if (selectionIndicators.ContainsKey(scenicSpot))
        {
            Destroy(selectionIndicators[scenicSpot]);
            selectionIndicators.Remove(scenicSpot);
        }
    }

    private void DrawPath()
    {
        HashSet<string> selectedSpotNames = new HashSet<string>(); 
        foreach (GameObject scenicSpot in selectedScenicSpots)
        {
            selectedSpotNames.Add(scenicSpot.name);
        }
        scenicSpotsManager.DrawPathByName(selectedSpotNames);
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
                    case "DrawPathButton":
                        DrawPath();
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

        ToggleScenicSpotSelection();
        //HandleInfoPanel();
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
