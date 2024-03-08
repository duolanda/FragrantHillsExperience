using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenicSpotSelectionManager : Singleton<ScenicSpotSelectionManager>
{
    // 存储已选择的景点
    public List<GameObject> SelectedScenicSpots { get; private set; } = new List<GameObject>();

    // 存储景点和其对应的选中指示器
    public Dictionary<GameObject, GameObject> SelectionIndicators { get; private set; } = new Dictionary<GameObject, GameObject>();

    public void AddSelectedScenicSpot(GameObject scenicSpot, GameObject indicator)
    {
        SelectedScenicSpots.Add(scenicSpot);
        SelectionIndicators[scenicSpot] = indicator;
    }

    public void RemoveSelectedScenicSpot(GameObject scenicSpot)
    {
        SelectedScenicSpots.Remove(scenicSpot);
        if (SelectionIndicators.ContainsKey(scenicSpot))
        {
            Destroy(SelectionIndicators[scenicSpot]);
            SelectionIndicators.Remove(scenicSpot);
        }
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
    }
}
