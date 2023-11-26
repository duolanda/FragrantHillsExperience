using System;
using System.Collections;
using System.Collections.Generic;
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

    void Update()
    {
    #if !UNITY_EDITOR    
        DrawOnTouch();
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
        var json = JsonUtility.ToJson(lineData);
        return System.Text.Encoding.UTF8.GetBytes(json);
    }
    
    private static List<ARLine> DeserializeLinesData(byte[] data)
    {
        var json = System.Text.Encoding.UTF8.GetString(data);
        return JsonUtility.FromJson<List<ARLine>>(json);
    }
    
    /// 发送数据
    public void SendCurrentLinesData()
    {
        byte[] data = SerializeLinesData(lines); // 序列化
    }
    
    public void OnReceiveLineData(byte[] data)
    {
        List<ARLine> lineData = DeserializeLinesData(data);
        foreach (ARLine line in lineData)
        {
            ARLine newLine = new ARLine(lineSettings);
            lines.Add(newLine);
            newLine.AddNewLineRenderer(line.LineRendererObject);
        }
    }

}

