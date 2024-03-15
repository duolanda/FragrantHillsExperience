using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class MyForegroundToRawImage : MonoBehaviour 
{
	private RawImage rawImage;
    public Color showColor;
    public int playerIndex = 0;
    private Color32[] colors = new Color32[6];

    void Start()
	{
        colors[0] = new Color32(217, 112, 99, 255); //红
        colors[1] = new Color32(106, 130, 231, 255); //蓝
        colors[2] = new Color32(112, 164, 67, 255); //绿
        colors[3] = new Color32(238, 237, 90, 255); //黄
        colors[4] = new Color32(220, 154, 66, 255); //橙
        colors[5] = new Color32(141, 100, 161, 255); //紫

        rawImage = GetComponent<RawImage>();

    }


	void Update () 
	{
		if (rawImage && rawImage.texture == null) 
		{
			MyBackgroundRemovalManager myBackManager = MyBackgroundRemovalManager.Instance;
            BackgroundRemovalManager backManager = BackgroundRemovalManager.Instance;
            KinectManager kinectManager = KinectManager.Instance;

			if (kinectManager && backManager && backManager.enabled /**&& backManager.IsBackgroundRemovalInitialized()*/)
            {
                rawImage.texture = backManager.GetForegroundTex();  // user's foreground texture
                rawImage.rectTransform.localScale = kinectManager.GetColorImageScale();
				rawImage.color = showColor;

			}
            else if(kinectManager && myBackManager && myBackManager.enabled)
            {
                Texture playerTexture = myBackManager.GetPlayerForegroundTex(playerIndex);
                rawImage.texture = playerTexture;
                rawImage.rectTransform.localScale = kinectManager.GetColorImageScale();
                rawImage.color = colors[playerIndex];
            }
			else if(kinectManager /**&& kinectManager.IsInitialized()*/)
			{
				SimpleBackgroundRemoval simpleBR = GameObject.FindObjectOfType<SimpleBackgroundRemoval>();
				bool isSimpleBR = simpleBR && simpleBR.enabled;

				rawImage.texture = kinectManager.GetUsersClrTex();  // color camera texture
				rawImage.rectTransform.localScale = kinectManager.GetColorImageScale();
				rawImage.color = !isSimpleBR ? Color.white : Color.clear;
			}
		}
//		else if(rawImage && rawImage.texture != null)
//		{
//			KinectManager kinectManager = KinectManager.Instance;
//			if(kinectManager == null)
//			{
//				rawImage.texture = null;
//				rawImage.color = Color.clear;
//			}
//		}
	}


	void OnApplicationPause(bool isPaused)
	{
		// fix for app pause & restore (UWP)
		if(isPaused && rawImage && rawImage.texture != null)
		{
			rawImage.texture = null;
			rawImage.color = Color.clear;
		}
	}

}
