using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class UserUIManager : MonoBehaviour
{
    public TextMeshProUGUI userChangeText;
    public TextMeshProUGUI userCountText;

    private KinectManager km;

    private Color32[] colors = new Color32[6]; 

    void Start()
    {
        colors[0] = new Color32(217, 112, 99, 255); //红
        colors[1] = new Color32(106, 130, 231, 255); //蓝
        colors[2] = new Color32(112, 164, 67, 255); //绿
        colors[3] = new Color32(238, 237, 90, 255); //黄
        colors[4] = new Color32(220, 154, 66, 255); //橙
        colors[5] = new Color32(141, 100, 161, 255); //紫


        GameObject KinectController = GameObject.Find("KinectController");
        if (KinectController != null)
        {
            km = KinectController.GetComponent<KinectManager>();
        }

        km.OnUserAdded.AddListener(UserAdd);
        km.OnUserRemoved.AddListener(UserRemove);

        userChangeText.overrideColorTags = true;
    }

    private void UserAdd(long userId, int userIndex)
    {
        userChangeText.text = "检测到游客加入";
        userChangeText.color = colors[userIndex];

        userCountText.text = "当前操纵游客数：" + km.GetUsersCount();
        StopAllCoroutines(); // 停止所有协程，以防之前的协程还在运行
        StartCoroutine(HideUserChangeText()); // 启动协程来隐藏文本
    }

    private void UserRemove(long userId, int userIndex)
    {
        userChangeText.text = "检测到游客离开";
        userChangeText.color = colors[userIndex];

        userCountText.text = "当前操纵游客数：" + km.GetUsersCount();
        StopAllCoroutines(); 
        StartCoroutine(HideUserChangeText()); 
    }

    IEnumerator HideUserChangeText()
    {
        yield return new WaitForSeconds(5); // 等待5秒
        userChangeText.text = ""; // 清空文本，隐藏文本框
    }
}
