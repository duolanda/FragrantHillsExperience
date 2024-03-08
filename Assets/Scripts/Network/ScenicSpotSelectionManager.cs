using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenicSpotSelectionManager : Singleton<ScenicSpotSelectionManager>
{
    // �洢��ѡ��ľ���
    public List<GameObject> SelectedScenicSpots { get; private set; } = new List<GameObject>();

    // �洢��������Ӧ��ѡ��ָʾ��
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
}
