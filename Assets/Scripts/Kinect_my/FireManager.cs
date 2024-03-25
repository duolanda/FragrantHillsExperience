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
    public Texture2D spawnMask;

    private Vector2 fireSize;
    private int extinguishedCount = 0; // ����Ļ�������

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
        // ���������������Ļ��
        Camera cam = Camera.main;
        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;

        //Debug.Log("height:" + height);
        //Debug.Log("width:" + width);

        width = width / 1.5f; //ֻ���м��������ɣ�������ԽС����������Խ��


        float spawnRangeX = (width - fireSize.x) / 2; 
        float spawnRangeY = (height - fireSize.y) / 2;

        Vector2 spawnPosition = new Vector2(1920, 1080);

        for (int i = 0; i < 100; i++) //100�ǳ��Դ�����������ѭ��
        {
            // �����ַ�Χ�����ѡ��һ����
            float x = Random.Range(-spawnRangeX, spawnRangeX);
            float y = Random.Range(-spawnRangeY, spawnRangeY);
            Color pixelColor = spawnMask.GetPixel((int)x, (int)y);

            // ��������ǰ�ɫ�����ɻ���
            if (pixelColor.r != 0) // ��� r ֵ��ȷ�����Ǻ�ɫ
            {
                spawnPosition = new Vector2(x, y);
                break; // �ɹ����ɺ��˳�ѭ��
            }
        }

        GameObject fire = Instantiate(firePrefab, spawnPosition, Quaternion.identity, transform);
        fire.GetComponent<FireTrigger>().OnExtinguished += () => extinguishedCount++; // �����¼�
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
            yield break; // �����Ϸ��δ��ʼ�����˳�Э��
        }

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

        DestroyAllFires();
        panelControl.GameOver(extinguishedCount);
    }
}
