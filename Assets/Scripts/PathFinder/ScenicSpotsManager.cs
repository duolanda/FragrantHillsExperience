using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ScenicSpotsManager : Singleton<ScenicSpotsManager> {
    public List<ScenicSpot> scenicSpots;
    public Dictionary<int, ScenicSpot> spotsDictionary;
    public PathDrawer pathDrawer;

    void Awake() {
        ImportScenicSpots();
        Debug.Log("Json 导入完毕");        

        //输出导入的景点
        //foreach (ScenicSpot spot in result)
        //{
        //    Debug.Log(spot.name);
        //}
    }

    public void DrawPathByName(HashSet<string> spotNames)
    {
        List<int> selectedSpotIDs = new List<int>();
        foreach (string name in spotNames)
        {
            ScenicSpot spot = scenicSpots.FirstOrDefault(s => s.name == name);
            if (spot != null)
            {
                selectedSpotIDs.Add(spot.id);
            }
        }

        List<ScenicSpot> path = FindPathCoveringAllSpots(selectedSpotIDs);
        pathDrawer.DrawPath(path);
    }
    
    public void ClearDraw()
    {
        pathDrawer.ClearPath();
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

            List<ScenicSpot> subPath = GetPathBetweenSpots(currentSpot, nextSpot);
            path.AddRange(subPath);
        }
        path.Insert(0, spotsDictionary[spotIDs[0]]);
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
