using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ScenicSpotsManager : Singleton<ScenicSpotsManager> {
    public List<ScenicSpot> scenicSpots;
    public Dictionary<int, ScenicSpot> spotsDictionary;
    public Transform imagesParent; // 用于存放所有Image GameObjects的父对象

    void Awake()
    {
        ImportScenicSpots();
        Debug.Log("Json 导入完毕");

        ImportDistances();

        //输出导入的景点
        // foreach (ScenicSpot spot in scenicSpots)
        // {
        //     Debug.Log(spot.name);
        // }

        //路径绘制测试
        // List<ScenicSpot> path = FindPathCoveringAllSpots(new List<int>{1,6,8,12,20,24,33}); 
        // List<int> pathID = path.Select(spot => spot.id).ToList();
        // List<string> pathName = path.Select(spot => spot.name).ToList();
        // Debug.Log("途径景点："+string.Join(", ", pathName));
        // Debug.Log("路线："+string.Join(", ", pathID));
        // DrawRoad(pathID);
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
        List<int> pathID = path.Select(spot => spot.id).ToList();
        DrawRoad(pathID);
    }

    private void DrawRoad(List<int> indices)
    {
        for (int i = 0; i < indices.Count - 1; i++)
        {
            int start = Mathf.Min(indices[i], indices[i + 1]);
            int end = Mathf.Max(indices[i], indices[i + 1]);
            string imageName = "Road/" + start + "-" + end; //总是用小号在前
            Sprite newSprite = Resources.Load<Sprite>(imageName);

            if (newSprite != null)
            {
                GameObject newImageGO = new GameObject(imageName);
                newImageGO.transform.SetParent(imagesParent, false);
                Image newImage = newImageGO.AddComponent<Image>();
                newImage.sprite = newSprite;
                newImage.color = new Color(255/255f,154/255f,4/255f,1);
                
                RectTransform rectTransform = newImageGO.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(3952, 4297); // 设置大小
                rectTransform.localScale = new Vector3(0.48f, 0.48f, 1); // 设置缩放

            }
            else
            {
                Debug.LogWarning("Sprite not found for: " + imageName);
            }
        }
    }
    
    public void ClearDraw()
    {
        foreach (Transform child in imagesParent)
        {
            Destroy(child.gameObject);
        }
    }

    void ImportScenicSpots() {
        string json = File.ReadAllText(Application.dataPath + "/Json/ScenicSpots.json");
        ScenicSpot[] spotsArray = JsonHelper.FromJson<ScenicSpot>(json);
        scenicSpots = new List<ScenicSpot>(spotsArray);
        
        // 初始化距离字典
        foreach (var spot in spotsArray) {
            spot.distances = new Dictionary<ScenicSpot, float>();
        }

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
    
    private void ImportDistances()
    {
        string[] lines = File.ReadAllLines("Assets/Resources/distance.csv");

        foreach (string line in lines)
        {
            string[] parts = line.Split(',');
            int sourceId = int.Parse(parts[0]);
            int targetId = int.Parse(parts[1]);
            float distance = float.Parse(parts[2]);

            ScenicSpot sourceSpot = scenicSpots.FirstOrDefault(spot => spot.id == sourceId);
            ScenicSpot targetSpot = scenicSpots.FirstOrDefault(spot => spot.id == targetId);

            if (sourceSpot != null && targetSpot != null)
            {
                sourceSpot.SetDistanceToNeighbor(targetSpot, distance);
                targetSpot.SetDistanceToNeighbor(sourceSpot, distance);
            }
        }
    }
    
}
