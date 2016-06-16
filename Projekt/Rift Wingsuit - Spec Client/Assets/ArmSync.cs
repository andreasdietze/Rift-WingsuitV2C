using UnityEngine;
using System.Collections;

public class ArmSync : MonoBehaviour {
	// Script access
	private NetworkManager nManager;
	private Player player;

	// Arm transformation
	private float rot = 0.0f;
	private Transform leftShoulder;
	private Transform rightShoulder;
	
	void Start () {
		// Find networkManager
		nManager = (NetworkManager)GameObject.FindGameObjectWithTag("Network").GetComponent("NetworkManager");
		// Funzt leider nicht -> Arme seperat aber nicht suit
		leftShoulder = GameObject.Find ("WGT-shoulder_L").transform;
		rightShoulder = GameObject.Find ("MCH-shoulder_rh_ns_ch_R").transform;
	}
	
	// Update is called once per frame
	void Update () {
		rot++;
		// Check if client has joined a server.
		// This is necessary because the playerprefab is automatically generated.
		if (nManager.serverJoined) {
			try {
				//player = (Player)GameObject.FindGameObjectWithTag ("Player").GetComponent("Player");
				//armPosition = player.syncEndOVRRotation;//lerpedOVRRotation;
			} catch (UnityException e) {
				Debug.Log(e.Message);
			}
		}
		//leftShoulder.rotation = Quaternion.Euler (new Vector3 (rot, 0.0f, 0.0f));
		//rightShoulder.rotation = Quaternion.Euler (new Vector3 (0.0f, 0.0f, rot));
	}
	
	bool showText = true;
	Rect textArea = new Rect(300,60,Screen.width, Screen.height);
	
	private void OnGUI()
	{
		//if(nManager.serverJoined && showText)
			//GUI.Label(textArea, headOrientation.ToString());
	}

}
