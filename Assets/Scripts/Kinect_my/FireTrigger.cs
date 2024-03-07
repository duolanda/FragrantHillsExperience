using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTrigger : MonoBehaviour
{
    public event Action OnExtinguished; // �������� UI ����

    void OnTriggerEnter2D(Collider2D collider)
	{
        //Debug.Log("�ɹ�����");
        OnExtinguished?.Invoke(); // �����¼�
        Destroy(gameObject);
	}
}
