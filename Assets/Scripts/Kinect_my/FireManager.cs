using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireManager : MonoBehaviour
{
    public GameObject firePrefab;
    public float spawnInterval = 0.5f; // �������ɼ��Ϊ0.5��
    public float duration = 60f; // ���ó���ʱ��Ϊ60��
    private Vector2 fireSize;

    void Start()
    {

        fireSize = firePrefab.GetComponent<SpriteRenderer>().bounds.size;
        
        StartCoroutine(SpawnFires());
    }

    void SpawnFire()
    {
        // ���������������Ļ��
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
            yield return new WaitForSeconds(spawnInterval); // �ȴ�0.5��
            timer += spawnInterval;
        }
    }
}
