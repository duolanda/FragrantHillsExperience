using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FireManager : MonoBehaviour
{
    public GameObject firePrefab;
    public float spawnInterval = 0.5f; // 设置生成间隔为0.5秒
    public float duration = 30f; // 设置持续时间为60秒
    public TextMeshProUGUI textCountDownSecond;
    public TextMeshProUGUI textExtinguishedCount;
    public Texture2D spawnMask;

    private Vector2 fireSize;
    private int extinguishedCount = 0; // 扑灭的火焰数量

    private SelectScenicSpotForFire panelControl;

    public bool gameStarted = false;


    void Start()
    {
        GameObject KinectController = GameObject.Find("KinectController");
        if (KinectController != null)
        {
            panelControl = KinectController.GetComponent<SelectScenicSpotForFire>();
        }
    }

    public void StartFire()
    {
        fireSize = firePrefab.GetComponent<SpriteRenderer>().bounds.size;
        StartCoroutine(SpawnFires());
    }

    void SpawnFire()
    {
        // 避免火焰生成在屏幕外
        Camera cam = Camera.main;
        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;

        //Debug.Log("height:" + height);
        //Debug.Log("width:" + width);

        width = width / 1.5f; //只在中间区域生成，除的数越小，生成区域越大


        float spawnRangeX = (width - fireSize.x) / 2; 
        float spawnRangeY = (height - fireSize.y) / 2;

        Vector2 spawnPosition = new Vector2(1920, 1080);

        for (int i = 0; i < 100; i++) //100是尝试次数，避免死循环
        {
            // 在遮罩范围内随机选择一个点
            float x = Random.Range(-spawnRangeX, spawnRangeX);
            float y = Random.Range(-spawnRangeY, spawnRangeY);
            Color pixelColor = spawnMask.GetPixel((int)x, (int)y);

            // 如果像素是白色，生成火焰
            if (pixelColor.r != 0) // 检查 r 值以确保不是黑色
            {
                spawnPosition = new Vector2(x, y);
                break; // 成功生成后退出循环
            }
        }

        GameObject fire = Instantiate(firePrefab, spawnPosition, Quaternion.identity, transform);
        fire.GetComponent<FireTrigger>().OnExtinguished += () => extinguishedCount++; // 订阅事件
    }

    public void DestroyAllFires()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }


    IEnumerator SpawnFires()
    {
        if (!gameStarted)
        {
            yield break; // 如果游戏尚未开始，则退出协程
        }

        float timer = duration;

        while (timer > 0)
        {
            SpawnFire();
            yield return new WaitForSeconds(spawnInterval);
            timer -= spawnInterval;
            textCountDownSecond.text = Mathf.CeilToInt(timer) + "s"; // 更新倒计时文本
            textExtinguishedCount.text = extinguishedCount.ToString(); // 更新扑灭火焰数量文本
        }

        textCountDownSecond.text = "0";

        DestroyAllFires();
        panelControl.GameOver(extinguishedCount);
    }
}
