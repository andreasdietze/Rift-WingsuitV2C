using UnityEngine;
using System.Collections;

public class CloudsManager : MonoBehaviour {

	// Different settings for cloud systems.
	public enum Weather  {Sunny, Cloudy, Stormy};
	public Weather weather = Weather.Sunny;

	// Handle activity of different cloud systems.
	private GameObject topLevelClouds 	= null;
	private GameObject midLevelClouds 	= null;
	private GameObject mountainClouds 	= null;

	// Settings for different cloud systems.
	private GameObject clouds 			= null;
	private GameObject cloudLayer 		= null;
	private GameObject mountainTop	 	= null;

	void Start () {

		//(Controller)GameObject.Find("RiftCam").GetComponent("KinectController");
		topLevelClouds = GameObject.Find ("TopLevel");
		midLevelClouds = GameObject.Find ("MidLevel");
		mountainClouds = GameObject.Find ("MountainClouds");

		// Default and initial weather is sunny. 
		switch(weather) {
			// Only skydome clouds.
		case Weather.Sunny :
			topLevelClouds.SetActive (false);
			midLevelClouds.SetActive (false);
			mountainClouds.SetActive (false);
			break;
			// Top and midLevel clouds.
		case Weather.Cloudy :
			topLevelClouds.SetActive (true);
			midLevelClouds.SetActive (true);
			mountainClouds.SetActive (false);
			break;
			// All cloud systems.
		case Weather.Stormy :
			topLevelClouds.SetActive (true);
			midLevelClouds.SetActive (true);
			mountainClouds.SetActive (true);
			break;
		default: weather = Weather.Sunny;
			break;
		}
	}

	void Update () {
	
	}
}
