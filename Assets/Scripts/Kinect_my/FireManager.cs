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

    private Vector2 fireSize;
    private int extinguishedCount = 0; // 扑灭的火焰数量



    void Start()
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

        Vector2 spawnPosition = new Vector2(
            Random.Range(-spawnRangeX, spawnRangeX),
            Random.Range(-spawnRangeY, spawnRangeY)
        );

        GameObject fire = Instantiate(firePrefab, spawnPosition, Quaternion.identity, transform);
        fire.GetComponent<FireTrigger>().OnExtinguished += () => extinguishedCount++; // 订阅事件
    }
    

    IEnumerator SpawnFires()
    {
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
    }
}
