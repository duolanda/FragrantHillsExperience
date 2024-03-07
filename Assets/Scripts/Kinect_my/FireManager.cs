using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FireManager : MonoBehaviour
{
    public GameObject firePrefab;
    public float spawnInterval = 0.5f; // �������ɼ��Ϊ0.5��
    public float duration = 30f; // ���ó���ʱ��Ϊ60��
    public TextMeshProUGUI textCountDownSecond;
    public TextMeshProUGUI textExtinguishedCount;

    private Vector2 fireSize;
    private int extinguishedCount = 0; // ����Ļ�������



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

        //Debug.Log("height:" + height);
        //Debug.Log("width:" + width);

        width = width / 1.5f; //ֻ���м��������ɣ�������ԽС����������Խ��


        float spawnRangeX = (width - fireSize.x) / 2; 
        float spawnRangeY = (height - fireSize.y) / 2;

        Vector2 spawnPosition = new Vector2(
            Random.Range(-spawnRangeX, spawnRangeX),
            Random.Range(-spawnRangeY, spawnRangeY)
        );

        GameObject fire = Instantiate(firePrefab, spawnPosition, Quaternion.identity, transform);
        fire.GetComponent<FireTrigger>().OnExtinguished += () => extinguishedCount++; // �����¼�
    }
    

    IEnumerator SpawnFires()
    {
        float timer = duration;

        while (timer > 0)
        {
            SpawnFire();
            yield return new WaitForSeconds(spawnInterval);
            timer -= spawnInterval;
            textCountDownSecond.text = Mathf.CeilToInt(timer) + "s"; // ���µ���ʱ�ı�
            textExtinguishedCount.text = extinguishedCount.ToString(); // ����������������ı�
        }

        textCountDownSecond.text = "0";
    }
}
