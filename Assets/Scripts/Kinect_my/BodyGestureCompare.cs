using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BodyGestureCompare : MonoBehaviour 
{
	[Tooltip("Camera used for screen-to-world calculations. This is usually the main camera.")]
	public Camera screenCamera;

	private MyGestureListener gestureListener;


	void Start() 
	{
		// hide mouse cursor
		//Cursor.visible = false;
		
		// by default set the main-camera to be screen-camera
		if (screenCamera == null) 
		{
			screenCamera = Camera.main;
		}

		// get the gestures listener
		gestureListener = MyGestureListener.Instance;
	}
	
	void Update() 
	{
		// dont run Update() if there is no gesture listener
		if(!gestureListener)
			return;
	}
	
    public bool CheckGesture(string gesture_name)
    {
        switch (gesture_name)
        {
            case "piano":
                return gestureListener.IsPlayPiano();
            case "pick":
                return gestureListener.IsPickRedLeaf();
            case "salute":
                return gestureListener.IsSalute();
            case "wooden_fish":
                return gestureListener.IsStrikeWoodenFish();
            case "write":
                return gestureListener.IsWriteInAirh();
        }
        return false;
    }

}
