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
            ARLine line = new ARLine(lineSettings);
            lines.Add(line);
            line.AddNewLineRenderer(transform, anchor, touchPosition);
        }
        else if(touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
        {
            lines[0].AddPoint(touchPosition);
        }
        else if(touch.phase == TouchPhase.Ended)
        {
            lines.RemoveAt(0);
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
}

