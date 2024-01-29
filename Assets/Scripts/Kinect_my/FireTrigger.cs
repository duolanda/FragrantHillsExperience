using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTrigger : MonoBehaviour
{
	void OnTriggerEnter2D(Collider2D collider)
	{
		//Debug.Log("³É¹¦´¥·¢");
		Destroy(gameObject);
	}
}
