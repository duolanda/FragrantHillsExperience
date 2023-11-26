using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARAnchorManager))]
public class ARDrawManager : Singleton<ARDrawManager>
{
    [SerializeField] private LineSettings lineSettings;

    [SerializeField] private UnityEvent OnDraw;
    [SerializeField] private ARAnchorManager anchorManager;
    [SerializeField] private Camera arCamera;
    private List<ARAnchor> anchors = new List<ARAnchor>();
    private List<ARLine> lines = new List<ARLine>();
    private ARLine currentLine;
    private bool CanDraw { get; set; }
    private CollaborativeSession collaborativeSession;
    
    private GameObject currentRemotePrefab;

    
    void Start()
    {
        collaborativeSession = FindObjectOfType<CollaborativeSession>();
    }
    
    void Update()
    {
    #if !UNITY_EDITOR    
        DrawOnTouch();
        // collaborativeSession?.ReceiveLinesData(HandleReceiveLinesData);
    #else
        DrawOnMouse();
    #endif
    }

    public void AllowDraw(bool isAllow)
    {
        CanDraw = isAllow;
    }
    
    void DrawOnTouch()
    {
        if (!CanDraw) return;

        Touch touch = Input.GetTouch(0);
        Vector3 touchPosition = arCamera.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, lineSettings.distanceFromCamera));

        if (touch.phase == TouchPhase.Began)
        {
            OnDraw?.Invoke();

            ARAnchor anchor = anchorManager.AddAnchor(new Pose(touchPosition, Quaternion.identity));
            if (anchor == null)
            {
                Debug.LogError("Error creating reference point");
            }
            else
            {
                anchors.Add(anchor);
                ARDebugManager.Instance.LogInfo($"Anchor created & total of {anchors.Count} anchor(s)");
            }

            currentLine = new ARLine(lineSettings);
            currentLine.AddNewLineRenderer(transform, anchor, touchPosition);
        }
        else if(touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
        {
            currentLine.AddPoint(touchPosition);
        }
        else if(touch.phase == TouchPhase.Ended)
        {
            lines.Add(currentLine);
            SendCurrentLinesData();
        }
    }
    
    void DrawOnMouse()
    {
        if(!CanDraw) return;

        Vector3 mousePosition = arCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, lineSettings.distanceFromCamera));

        if(Input.GetMouseButton(0))
        {
            OnDraw?.Invoke();

            if(lines.Count == 0)
            {
                ARLine line = new ARLine(lineSettings);
                lines.Add(line);
                line.AddNewLineRenderer(transform, null, mousePosition);
            }
            else 
            {
                lines[0].AddPoint(mousePosition);
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            lines.RemoveAt(0);   
        }
    }

    GameObject[] GetAllLinesInScene()
    {
        return GameObject.FindGameObjectsWithTag("Line");
    }

    public void ClearLines()
    {
        GameObject[] currentLines = GetAllLinesInScene();
        foreach (GameObject currentLine in currentLines)
        {
            LineRenderer line = currentLine.GetComponent<LineRenderer>();
            Destroy(currentLine);
        }
        lines.Clear();
    }
    
    /// 序列化数据
    private static byte[] SerializeLinesData(List<ARLine> lineData)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        
        List<SerializableLineData> lineDataToSend = new List<SerializableLineData>();
        foreach (ARLine line in lineData)
        {
            lineDataToSend.Add(new SerializableLineData(line));
        }
        string json = JsonConvert.SerializeObject(lineDataToSend, settings);
        return System.Text.Encoding.UTF8.GetBytes(json);
    }
    
    private static List<SerializableLineData> DeserializeLinesData(byte[] data)
    {
        var json = System.Text.Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<List<SerializableLineData>>(json);
    }
    
    /// 发送数据
    private void SendCurrentLinesData()
    {
    #if UNITY_IOS && !UNITY_EDITOR
        byte[] serializedData = SerializeLinesData(lines);
        collaborativeSession?.SendLinesData(serializedData);
    #else
        Debug.Log("Collaborative sessions are an ARKit 3 feature; This platform does not support them.");
    #endif
    }
    
    public void HandleReceiveLinesData(byte[] data)
    {
        List<SerializableLineData> lineData = DeserializeLinesData(data);
        ARDebugManager.Instance.LogInfo($"Received {lineData.Count} lines");
        foreach (SerializableLineData line in lineData)
        {
            ARLine newLine = new ARLine(lineSettings);
            lines.Add(newLine);
            newLine.AddNewLineRenderer(line);
        }
        ARDebugManager.Instance.LogInfo($"Total number of lines is {lines.Count} now");
    }
    
    

}

