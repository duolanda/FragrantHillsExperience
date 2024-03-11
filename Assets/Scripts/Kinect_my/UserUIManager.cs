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
        userChangeText.text = "��⵽�οͼ���";
        userCountText.text = "��ǰ�����ο�����" + km.GetUsersCount();
        StopAllCoroutines(); // ֹͣ����Э�̣��Է�֮ǰ��Э�̻�������
        StartCoroutine(HideUserChangeText()); // ����Э���������ı�
    }

    private void UserRemove(long userId, int userIndex)
    {
        userChangeText.text = "��⵽�ο��뿪";
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
