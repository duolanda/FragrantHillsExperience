using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableLineData
{
    public List<Vector3> points;
    // public Color startColor;
    // public Color endColor;
    // public float startWidth;
    // public float endWidth;
    // 可以添加其他必要的字段

    public SerializableLineData(ARLine arLine)
    {
        // 从 arLine 中提取必要的信息
        this.points = new List<Vector3>(); // 假设您存储了线条的点
        for (int i = 0; i < arLine.LineRenderer.positionCount; i++)
        {
            this.points.Add(arLine.LineRenderer.GetPosition(i));
        }
        // this.startColor = arLine.LineRenderer.startColor;
        // this.endColor = arLine.LineRenderer.endColor;
        // this.startWidth = arLine.LineRenderer.startWidth;
        // this.endWidth = arLine.LineRenderer.endWidth;
        // 设置其他字段...
    }
}
