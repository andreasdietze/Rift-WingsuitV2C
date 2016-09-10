using UnityEngine;
using System.Collections;

public class HeadOrientation : MonoBehaviour {

	// Script access
	private NetworkManager nManager;
	private Player player;

	// Head orientation by oculus
	private Quaternion headOrientation;


	private Netz netz;
	
	private Transform head;
	private Transform cam;
	
	private LookAtCam lac;
	
	void Start () {
		// Find networkManager
		netz = (Netz)GameObject.FindGameObjectWithTag("Network").GetComponent("Netz");
		Debug.Log(netz);
		// Get player head
		head =  GameObject.Find("driverhelmet").transform;
		// Get player script
		player = (Player)GameObject.FindGameObjectWithTag ("Player").GetComponent("Player");
		Debug.Log(player);
		// Get look at cam 
		lac = (LookAtCam)GameObject.FindGameObjectWithTag("MainCamera").GetComponent("LookAtCam");
		
		//head.transform.position = (player.transform.position);// + (player.transform.rotation * (Vector3.one));
		//head.transform.position = player.transform.position + (headOrientation * Vector3.up);
		
	}
	
	// Update is called once per frame
	void Update () {

		// Check if client has joined a server.
		// This is necessary because the playerprefab is automatically generated.
		if (netz.serverJoined) {
			try {
				player = (Player)GameObject.FindGameObjectWithTag ("Player").GetComponent("Player");
				cam = GameObject.FindGameObjectWithTag ("MainCamera").transform;
				if(lac.enableOVROrientation) 
					headOrientation = player.syncEndOVRRotation;//lerpedOVRRotation;  // syncEndOVRRotation
				else 
					headOrientation = Quaternion.identity;
			} catch (UnityException e) {
				Debug.Log(e.Message);
			}
		}
		
		//transform.rotation = headOrientation * Quaternion.Euler(new Vector3(0.0f, 0.0f, -90.0f));
		//transform.rotation = headOrientation * Quaternion.Euler(new Vector3(0.0f, 0.0f, -90.0f));
		head.transform.rotation = headOrientation;
	}

	
	// Debug
	bool showText = true;
	Rect textArea = new Rect(300,30,Screen.width, Screen.height);
	
	private void OnGUI()
	{
		if(netz.serverJoined && showText)
			GUI.Label(textArea, headOrientation.ToString());
	}
}
