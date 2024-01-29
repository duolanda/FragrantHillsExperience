using UnityEngine;

//后处理脚本
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
                Debug.LogError("没有赋予Shader");
                return null;
            }
            if (!PostProcessingShader.isSupported)
            {
                Debug.LogError("当前Shader不支持");
                return null;
            }
            //如果材质没有创建，则根据Shader创建材质，并给成员变量赋值存储
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

