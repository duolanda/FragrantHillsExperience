using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireManager : MonoBehaviour
{
    public GameObject firePrefab;
    public float spawnInterval = 0.5f; // 设置生成间隔为0.5秒
    public float duration = 60f; // 设置持续时间为60秒
    private Vector2 fireSize;

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

        float spawnRangeX = (width - fireSize.x) / 2; 
        float spawnRangeY = (height - fireSize.y) / 2;

        Vector2 spawnPosition = new Vector2(
            Random.Range(-spawnRangeX, spawnRangeX),
            Random.Range(-spawnRangeY, spawnRangeY)
        );

        Instantiate(firePrefab, spawnPosition, Quaternion.identity, transform);
    }
    

    IEnumerator SpawnFires()
    {
        float timer = 0f;

        while (timer < duration)
        {
            SpawnFire();
            yield return new WaitForSeconds(spawnInterval); // 等待0.5秒
            timer += spawnInterval;
        }
    }
}
