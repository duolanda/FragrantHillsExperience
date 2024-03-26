using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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
    private ScenicSpotSelectionManager scenicSpotSelectionManager;

    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;

    private InteractionManager.HandEventType lastHandEvent = InteractionManager.HandEventType.None;
    private Vector3 screenNormalPos = Vector3.zero;
    
    private Sprite normalButtonSprite; // 正常状态的按钮图
    private Sprite pressedButtonSprite; // 按下状态的按钮图


    void OnEnable()
    {
        ScenicSpotSelectionManager.SpotUpdateEvent += UpdateSelectSpotShow;
    }

    void OnDisble()
    {
        ScenicSpotSelectionManager.SpotUpdateEvent -= UpdateSelectSpotShow;
    }

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

        scenicSpotsManager = ScenicSpotsManager.Instance;
        scenicSpotSelectionManager = ScenicSpotSelectionManager.Instance;
        //gestureListener = MapGestureListener.Instance;

        // 添加景点 gameobject
        var scenicSpotsParent = GameObject.Find("ScenicSpots");
        if (scenicSpotsParent != null)
        {
            Transform[] scenicSpotsChildren = scenicSpotsParent.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in scenicSpotsChildren)
            {
                if (child != scenicSpotsParent.transform) // 确保不包括父对象本身
                {
                    scenicSpots.Add(child.gameObject);
                }
            }
        }
        
        // 加载按钮图标
        normalButtonSprite = Resources.Load<Sprite>("Button/按钮");
        pressedButtonSprite = Resources.Load<Sprite>("Button/按钮-按下");


    }

    void Update()
    {
        HandleHoverDisplay();
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


    private void ToggleScenicSpotSelection()
    {
        foreach (GameObject scenicSpot in scenicSpots)
        {
            if (IsCursorNearObject(scenicSpot))    
            {
                List<GameObject> selectedScenicSpots = scenicSpotSelectionManager.SelectedScenicSpots;
                if (selectedScenicSpots.Contains(scenicSpot))
                {
                    DeselectAScenicSpot(scenicSpot);
                }
                else
                {
                    SelectAScenicSpot(scenicSpot);
                }
                break;
            }
        }
    }

    //选择了某个景点
    private void SelectAScenicSpot(GameObject scenicSpot, bool updateRemote = true)
    {
        GameObject indicator = Instantiate(selectionIndicatorPrefab, scenicSpot.transform.position+ new Vector3(0, 150, 0), Quaternion.identity, canvas.transform);
        scenicSpotSelectionManager.AddSelectedScenicSpot(scenicSpot, indicator, updateRemote); // 存储指示器
    }

    //取消选择某个景点
    private void DeselectAScenicSpot(GameObject scenicSpot)
    {
        scenicSpotSelectionManager.RemoveSelectedScenicSpot(scenicSpot);
    }

    private void DrawPath()
    {
        List<GameObject> selectedScenicSpots = scenicSpotSelectionManager.SelectedScenicSpots;
        HashSet<string> selectedSpotNames = new HashSet<string>(); 
        foreach (GameObject scenicSpot in selectedScenicSpots)
        {
            selectedSpotNames.Add(scenicSpot.name);
        }
        scenicSpotsManager.DrawPathByName(selectedSpotNames);
    }

    private void ResetSelectAndPath(bool updateRemote = true)
    {
        scenicSpotsManager.ClearDraw();
        scenicSpotSelectionManager.RemoveAllSelectedScenicSpot(updateRemote);
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
                    case "DrawPathButton":
                        ChangeButtonSprite(result.gameObject, pressedButtonSprite);
                        DrawPath();
                        break;
                    case "ResetButton":
                        ChangeButtonSprite(result.gameObject, pressedButtonSprite);
                        ResetSelectAndPath();
                        break;
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

    private void UpdateSelectSpotShow()
    {
        // 更新选择的景点的画面，加上同步后id的景点
        List<GameObject> selectedScenicSpots = new List<GameObject>();
        scenicSpotSelectionManager.SelectedScenicSpots.ForEach(i => selectedScenicSpots.Add(i)); //深拷贝才行

        ResetSelectAndPath(false); //必须再清一遍，不然会重复添加 id

        foreach (GameObject scenicSpot in selectedScenicSpots)
        {
            SelectAScenicSpot(scenicSpot, false);
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
