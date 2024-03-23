using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ScenicSpot
{
    public int id;
    public string name;
    public Vector3 position;
    public List<int> neighborIDs;   // 用于 JSON 序列化的邻居 ID 列表
    public Dictionary<ScenicSpot, float> distances;
    [NonSerialized] 
    public List<ScenicSpot> neighbors; // 用于内部寻路逻辑
    //A* 需要
    [NonSerialized] 
    public float gCost;
    [NonSerialized] 
    public float hCost;
    [NonSerialized] 
    public ScenicSpot parent;
    public float FCost { get { return gCost + hCost; } }

    public ScenicSpot(int _id, string _name, Vector3 _position) {
        id = _id;
        name = _name;
        position = _position;
        neighbors = new List<ScenicSpot>(); // 初始化邻接列表
        neighborIDs = new List<int>();
        distances = new Dictionary<ScenicSpot, float>();
    }

    public void AddNeighbor(ScenicSpot neighbor) {
        if (!neighbors.Contains(neighbor)) {
            neighbors.Add(neighbor);
            // 关系是双向的，也为邻居添加此景点作为邻居
            neighbor.neighbors.Add(this);
        }
    }
    
    public void SetDistanceToNeighbor(ScenicSpot neighbor, float distance) {
        distances[neighbor] = distance;
    }
    
    public void InitializeNeighbors(Dictionary<int, ScenicSpot> spotsDictionary) {
        neighbors = neighborIDs.Select(id => spotsDictionary[id]).ToList();
    }
}

