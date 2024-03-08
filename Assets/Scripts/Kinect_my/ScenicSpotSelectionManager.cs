using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScenicSpotSelectionManager : Singleton<ScenicSpotSelectionManager>
{
    public delegate void SpotUpdateDelegate();
    public static event SpotUpdateDelegate SpotUpdateEvent;
    private Dictionary<int, GameObject> id2SpotsObject = new Dictionary<int, GameObject>();
    private ScenicSpotsManager scenicSpotsManager;

    public List<GameObject> SelectedScenicSpots { get; private set; } = new List<GameObject>();

    public Dictionary<GameObject, GameObject> SelectionIndicators { get; private set; } = new Dictionary<GameObject, GameObject>();

    public GameObject scenicSpotsParent;

    private Server ServerControl;

    void Start()
    {
        // 启动服务器
        GameObject NetworkManager = GameObject.Find("NetworkManager");
        if (NetworkManager != null)
        {
            ServerControl = NetworkManager.GetComponent<Server>();
        }
        ServerControl.StartServer();

        scenicSpotsManager = ScenicSpotsManager.Instance;

        // 添加景点 gameobject
        if (scenicSpotsParent != null)
        {
            Transform[] scenicSpotsChildren = scenicSpotsParent.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in scenicSpotsChildren)
            {
                if (child != scenicSpotsParent.transform)
                {
                    //建立景点 object 和 id 对应
                    name = child.gameObject.name;
                    int id = scenicSpotsManager.spotsDictionary.FirstOrDefault(s => s.Value.name == name).Key;
                    id2SpotsObject[id] = child.gameObject;
                }
            }
        }
    }

    public void AddSelectedScenicSpot(GameObject scenicSpot, GameObject indicator)
    {
        SelectedScenicSpots.Add(scenicSpot);
        SelectionIndicators[scenicSpot] = indicator;

        ServerControl.UpdateGlobalIDs(GenerateIDs());
    }

    public void RemoveSelectedScenicSpot(GameObject scenicSpot)
    {
        SelectedScenicSpots.Remove(scenicSpot);
        if (SelectionIndicators.ContainsKey(scenicSpot))
        {
            Destroy(SelectionIndicators[scenicSpot]);
            SelectionIndicators.Remove(scenicSpot);
        }

        ServerControl.UpdateGlobalIDs(GenerateIDs());
    }

    public void RemoveAllSelectedScenicSpot()
    {
        for (int i = SelectedScenicSpots.Count - 1; i >= 0; i--)
        {
            var scenicSpot = SelectedScenicSpots[i];
            if (SelectionIndicators.ContainsKey(scenicSpot))
            {
                Destroy(SelectionIndicators[scenicSpot]);
                SelectionIndicators.Remove(scenicSpot);
            }
            SelectedScenicSpots.RemoveAt(i);
        }

        ServerControl.UpdateGlobalIDs(GenerateIDs());
    }

    public void UpdateSelectedScenicSpotIDList(List<int> idList)
    {
        SelectedScenicSpots.Clear();

        foreach(int id in idList)
        {
            id2SpotsObject.TryGetValue(id, out GameObject scenicSpot);
            SelectedScenicSpots.Add(scenicSpot);
        }

        SpotUpdateEvent?.Invoke(); //select scenic spot 脚本更新
    }

    private List<int> GenerateIDs()
    {
        List<int> SelectedScenicSpotIDs = new List<int>();

        foreach (GameObject spot in SelectedScenicSpots)
        {
            int id = id2SpotsObject.FirstOrDefault(s => s.Value.name == spot.name).Key;
            SelectedScenicSpotIDs.Add(id);
        }
        return SelectedScenicSpotIDs;
    }
}
