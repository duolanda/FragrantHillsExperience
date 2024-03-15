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
        colors[0] = new Color32(217, 112, 99, 255); //��
        colors[1] = new Color32(106, 130, 231, 255); //��
        colors[2] = new Color32(112, 164, 67, 255); //��
        colors[3] = new Color32(238, 237, 90, 255); //��
        colors[4] = new Color32(220, 154, 66, 255); //��
        colors[5] = new Color32(141, 100, 161, 255); //��


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
        userChangeText.text = "��⵽�οͼ���";
        userChangeText.color = colors[userIndex];

        userCountText.text = "��ǰ�����ο�����" + km.GetUsersCount();
        StopAllCoroutines(); // ֹͣ����Э�̣��Է�֮ǰ��Э�̻�������
        StartCoroutine(HideUserChangeText()); // ����Э���������ı�
    }

    private void UserRemove(long userId, int userIndex)
    {
        userChangeText.text = "��⵽�ο��뿪";
        userChangeText.color = colors[userIndex];

        userCountText.text = "��ǰ�����ο�����" + km.GetUsersCount();
        StopAllCoroutines(); 
        StartCoroutine(HideUserChangeText()); 
    }

    IEnumerator HideUserChangeText()
    {
        yield return new WaitForSeconds(5); // �ȴ�3��
        userChangeText.text = ""; // ����ı��������ı���
    }
}
