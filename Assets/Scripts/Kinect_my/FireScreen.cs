using UnityEngine;

//����ű�
[ExecuteInEditMode]
public class FireScreen : MonoBehaviour
{
    public Shader PostProcessingShader;
    private Material mat;
    public Material Mat
    {
        get
        {
            if (PostProcessingShader == null)
            {
                Debug.LogError("û�и���Shader");
                return null;
            }
            if (!PostProcessingShader.isSupported)
            {
                Debug.LogError("��ǰShader��֧��");
                return null;
            }
            //�������û�д����������Shader�������ʣ�������Ա������ֵ�洢
            if (mat == null)
            {
                Material _newMaterial = new Material(PostProcessingShader);
                _newMaterial.hideFlags = HideFlags.HideAndDontSave;
                mat = _newMaterial;
                return _newMaterial;
            }
            return mat;
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, Mat);
    }
}

