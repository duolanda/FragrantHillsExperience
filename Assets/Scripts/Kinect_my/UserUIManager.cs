using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class UserUIManager : MonoBehaviour
{
    public TextMeshProUGUI userChangeText;
    public TextMeshProUGUI userCountText;

    private KinectManager km;

    void Start()
    {
        GameObject KinectController = GameObject.Find("KinectController");
        if (KinectController != null)
        {
            km = KinectController.GetComponent<KinectManager>();
        }

        km.OnUserAdded.AddListener(UserAdd);
        km.OnUserRemoved.AddListener(UserRemove);
    }

    private void UserAdd(long userId, int userIndex)
    {
        userChangeText.text = "检测到游客加入";
        userCountText.text = "当前操纵游客数：" + km.GetUsersCount();
        StopAllCoroutines(); // 停止所有协程，以防之前的协程还在运行
        StartCoroutine(HideUserChangeText()); // 启动协程来隐藏文本
    }

    private void UserRemove(long userId, int userIndex)
    {
        userChangeText.text = "检测到游客离开";
        userCountText.text = "当前操纵游客数：" + km.GetUsersCount();
        StopAllCoroutines(); 
        StartCoroutine(HideUserChangeText()); 
    }

    IEnumerator HideUserChangeText()
    {
        yield return new WaitForSeconds(5); // 等待3秒
        userChangeText.text = ""; // 清空文本，隐藏文本框
    }
}
