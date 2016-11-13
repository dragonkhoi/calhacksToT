using UnityEngine;
using UnityEngine.VR.WSA.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.VR.WSA.WebCam;

public class GazeGestureManager : MonoBehaviour
{
	PhotoCapture photoCaptureObject = null;

	public static GazeGestureManager Instance { get; private set; }

	// Represents the hologram that is currently being gazed at.
	public GameObject FocusedObject { get; private set; }

	GestureRecognizer recognizer;

	// Use this for initialization
	void Start()
	{
		Instance = this;

		// Set up a GestureRecognizer to detect Select gestures.
		recognizer = new GestureRecognizer();
		recognizer.TappedEvent += (source, tapCount, ray) =>
		{
			StartPhotoCap();
			// Send an OnSelect message to the focused object and its ancestors.
			if (FocusedObject != null)
			{
				FocusedObject.SendMessageUpwards("OnSelect");
			}
		};
		recognizer.StartCapturingGestures();
	}

	void StartPhotoCap(){
		PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated); //hologram show, callback
	}

	void OnPhotoCaptureCreated(PhotoCapture captureObject)
	{
		photoCaptureObject = captureObject;

		List<Resolution> resolutions = new List<Resolution>(PhotoCapture.SupportedResolutions);
		Resolution cameraResolution = resolutions[0];

		CameraParameters c = new CameraParameters();
		c.hologramOpacity = 0.0f; //can't see it
		c.cameraResolutionWidth = cameraResolution.width;
		c.cameraResolutionHeight = cameraResolution.height;
		c.pixelFormat = CapturePixelFormat.BGRA32;

		captureObject.StartPhotoModeAsync(c, false, OnPhotoModeStarted);

	}

	void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
	{
		photoCaptureObject.Dispose();
		photoCaptureObject = null;
	}

	private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
	{
		if(result.success)
		{
			string filename = string.Format(@"CapturedImage{0}_n.jpg", Time.time);
			Debug.Log(Application.persistentDataPath);
			string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);

			photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
		}
		else
		{
			Debug.LogError("Unable to start photo mode!");
		}
	}

	void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
	{
		if (result.success)
		{
			Debug.Log("Saved Photo to disk!");
			photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
		}
		else
		{
			Debug.Log("Failed to save Photo to disk");
		}
	}

	// Update is called once per frame
	void Update()
	{
		// Figure out which hologram is focused this frame.
		GameObject oldFocusObject = FocusedObject;

		// Do a raycast into the world based on the user's
		// head position and orientation.
		var headPosition = Camera.main.transform.position;
		var gazeDirection = Camera.main.transform.forward;

		RaycastHit hitInfo;
		if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
		{
			// If the raycast hit a hologram, use that as the focused object.
			FocusedObject = hitInfo.collider.gameObject;
		}
		else
		{
			// If the raycast did not hit a hologram, clear the focused object.
			FocusedObject = null;
		}

		// If the focused object changed this frame,
		// start detecting fresh gestures again.
		if (FocusedObject != oldFocusObject)
		{
			recognizer.CancelGestures();
			recognizer.StartCapturingGestures();
		}
	}
}