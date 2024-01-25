using UnityEngine;
using System.Collections;
using System;

public class MapGestureListener : MonoBehaviour, KinectGestures.GestureListenerInterface
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player.")]
	public int playerIndex = 0;

	[Tooltip("UI-Text to display gesture-listener messages and gesture information.")]
	public UnityEngine.UI.Text gestureInfo;

	// singleton instance of the class
	private static MapGestureListener instance = null;

	// internal variables to track if progress message has been displayed
	private bool progressDisplayed;
	private float progressGestureTime;

	// whether the needed gesture has been detected or not
	private bool stopGesture = false;


	// Gets the singleton instance.
	public static MapGestureListener Instance
	{
		get
		{
			return instance;
		}
	}

	/// <summary>
	/// Determines whether stop gesture is detected.
	/// </summary>
	/// <returns><c>true</c> if stop gesture is detected; otherwise, <c>false</c>.</returns>
	public bool IsStop()
	{
		if (stopGesture)
		{
			stopGesture = false;
			return true;
		}

		return false;
	}


	/// <summary>
	/// Invoked when a new user is detected. Here you can start gesture tracking by invoking KinectManager.DetectGesture()-function.
	/// </summary>
	/// <param name="userId">User ID</param>
	/// <param name="userIndex">User index</param>
	public void UserDetected(long userId, int userIndex)
	{
		// the gestures are allowed for the primary user only
		KinectManager manager = KinectManager.Instance;
		if (!manager || (userIndex != playerIndex))
			return;

		// detect these user specific gestures
		manager.DetectGesture(userId, KinectGestures.Gestures.Stop);
	}

	/// <summary>
	/// Invoked when a user gets lost. All tracked gestures for this user are cleared automatically.
	/// </summary>
	/// <param name="userId">User ID</param>
	/// <param name="userIndex">User index</param>
	public void UserLost(long userId, int userIndex)
	{
		// the gestures are allowed for the primary user only
		if (userIndex != playerIndex)
			return;

		if (gestureInfo != null)
		{
			gestureInfo.text = string.Empty;
		}
	}

	/// <summary>
	/// Invoked when a gesture is in progress.
	/// </summary>
	/// <param name="userId">User ID</param>
	/// <param name="userIndex">User index</param>
	/// <param name="gesture">Gesture type</param>
	/// <param name="progress">Gesture progress [0..1]</param>
	/// <param name="joint">Joint type</param>
	/// <param name="screenPos">Normalized viewport position</param>
	public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture,
								  float progress, KinectInterop.JointType joint, Vector3 screenPos)
	{
		// the gestures are allowed for the primary user only
		if (userIndex != playerIndex)
			return;

		if ((gesture == KinectGestures.Gestures.ZoomOut || gesture == KinectGestures.Gestures.ZoomIn) && progress > 0.5f)
		{
			if (gestureInfo != null)
			{
				string sGestureText = string.Format("{0} - {1:F0}%", gesture, screenPos.z * 100f);
				gestureInfo.text = sGestureText;

				progressDisplayed = true;
				progressGestureTime = Time.realtimeSinceStartup;
			}
		}
		else if ((gesture == KinectGestures.Gestures.Wheel || gesture == KinectGestures.Gestures.LeanLeft ||
				 gesture == KinectGestures.Gestures.LeanRight) && progress > 0.5f)
		{
			if (gestureInfo != null)
			{
				string sGestureText = string.Format("{0} - {1:F0} degrees", gesture, screenPos.z);
				gestureInfo.text = sGestureText;

				progressDisplayed = true;
				progressGestureTime = Time.realtimeSinceStartup;
			}
		}
		else if (gesture == KinectGestures.Gestures.Run && progress > 0.5f)
		{
			if (gestureInfo != null)
			{
				string sGestureText = string.Format("{0} - progress: {1:F0}%", gesture, progress * 100);
				gestureInfo.text = sGestureText;

				progressDisplayed = true;
				progressGestureTime = Time.realtimeSinceStartup;
			}
		}
	}

	/// <summary>
	/// Invoked if a gesture is completed.
	/// </summary>
	/// <returns>true</returns>
	/// <c>false</c>
	/// <param name="userId">User ID</param>
	/// <param name="userIndex">User index</param>
	/// <param name="gesture">Gesture type</param>
	/// <param name="joint">Joint type</param>
	/// <param name="screenPos">Normalized viewport position</param>
	public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture,
								  KinectInterop.JointType joint, Vector3 screenPos)
	{
		// the gestures are allowed for the primary user only
		if (userIndex != playerIndex)
			return false;

		if (gestureInfo != null)
		{
			string sGestureText = gesture + " detected";
			gestureInfo.text = sGestureText;
		}

		if (gesture == KinectGestures.Gestures.Stop)
			stopGesture = true;
		return true;
	}

	/// <summary>
	/// Invoked if a gesture is cancelled.
	/// </summary>
	/// <returns>true</returns>
	/// <c>false</c>
	/// <param name="userId">User ID</param>
	/// <param name="userIndex">User index</param>
	/// <param name="gesture">Gesture type</param>
	/// <param name="joint">Joint type</param>
	public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture,
								  KinectInterop.JointType joint)
	{
		// the gestures are allowed for the primary user only
		if (userIndex != playerIndex)
			return false;

		if (progressDisplayed)
		{
			progressDisplayed = false;

			if (gestureInfo != null)
			{
				gestureInfo.text = String.Empty;
			}
		}

		return true;
	}


	void Awake()
	{
		instance = this;
	}

	void Update()
	{
		if (progressDisplayed && ((Time.realtimeSinceStartup - progressGestureTime) > 2f))
		{
			progressDisplayed = false;
			gestureInfo.text = String.Empty;

			Debug.Log("Forced progress to end.");
		}
	}

}
