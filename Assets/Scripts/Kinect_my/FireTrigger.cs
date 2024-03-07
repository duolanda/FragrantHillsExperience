using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTrigger : MonoBehaviour
{
    public event Action OnExtinguished; // 用来触发 UI 计数

    void OnTriggerEnter2D(Collider2D collider)
	{
        //Debug.Log("成功触发");
        OnExtinguished?.Invoke(); // 触发事件
        Destroy(gameObject);
	}
}
