using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ScenicSpotsManager : MonoBehaviour {
    public List<ScenicSpot> scenicSpots;
    public Dictionary<int, ScenicSpot> spotsDictionary;
    public PathDrawer pathDrawer;

    void Start() {
        // // 手动添加景点和邻居
        // scenicSpots = new List<ScenicSpot>();
        
        // var spot1 = new ScenicSpot("景点1", new Vector3(0, 0, 0));
        // var spot2 = new ScenicSpot("景点2", new Vector3(10, 0, 0));
        // scenicSpots.Add(spot1);
        // scenicSpots.Add(spot2);
        //
        // 
        // spot1.AddNeighbor(spot2); // 景点1与景点2相连
        ImportScenicSpots();
        
        List<ScenicSpot> result = FindPathCoveringAllSpots(new List<int>(){45,44,35,32,28,21,4,3});

        pathDrawer.DrawPath(result);

        foreach (ScenicSpot spot in result)
        {
            Debug.Log(spot.name);
        }
    }
    
    void ImportScenicSpots() {
        string json = File.ReadAllText(Application.dataPath + "/Json/ScenicSpots.json");
        ScenicSpot[] spotsArray = JsonHelper.FromJson<ScenicSpot>(json);
        scenicSpots = new List<ScenicSpot>(spotsArray);

        // 构建邻居关系
        spotsDictionary = scenicSpots.ToDictionary(spot => spot.id, spot => spot);
        foreach (var spot in scenicSpots) {
            spot.neighbors = new List<ScenicSpot>();
            // 只能单向添加邻居，替换成下面的遍历
            // spot.InitializeNeighbors((spotsDictionary));
            // TryGetValue 方法尝试从字典中获取与指定的键（在这个例子中是 neighborID）关联的值。如果找到了，它会将该值赋给 out 参数（这里是 neighborSpot）
            foreach (var neighborID in spot.neighborIDs) {
                ScenicSpot neighborSpot;
                if (spotsDictionary.TryGetValue(neighborID, out neighborSpot)) {
                    spot.neighbors.Add(neighborSpot);
                    // 确保双向关系
                    if (!neighborSpot.neighborIDs.Contains(spot.id)) {
                        neighborSpot.neighborIDs.Add(spot.id);
                        if (neighborSpot.neighbors==null) { neighborSpot.neighbors = new List<ScenicSpot>(); } //以防邻居的列表没有初始化
                        neighborSpot.neighbors.Add(spot);
                    }
                }
            }
        }
    }
    
    public List<ScenicSpot> FindPathCoveringAllSpots(List<int> spotIDs) {
        List<ScenicSpot> path = new List<ScenicSpot>();

        for (int i = 0; i < spotIDs.Count - 1; i++) {
            ScenicSpot currentSpot = spotsDictionary[spotIDs[i]];
            ScenicSpot nextSpot = spotsDictionary[spotIDs[i + 1]];

            List<ScenicSpot> subPath = GetPathBetweenSpots(currentSpot, nextSpot); // 使用你的路径查找算法
            path.AddRange(subPath);
        }
        return path;
    }

    private List<ScenicSpot> GetPathBetweenSpots(ScenicSpot start, ScenicSpot end) {
        //调用 A* 算法，找到两个景点之间的路径
        List<ScenicSpot> result = AStar.FindPath(start, end);

        // string temp = null;
        // foreach (ScenicSpot scene in result) { temp += (scene.name+", "); }
        // Debug.Log("start:"+start.name+" end:"+end.name);
        // Debug.Log(temp);
        return result;
    }
}
