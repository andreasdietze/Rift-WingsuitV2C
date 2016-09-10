using UnityEngine;
using System.Collections;

public class LookAtCam : MonoBehaviour {
	
	// Camera transformation
	private Transform cam; 

	// Target (player) transformation
	private Transform target;

	// Tmp cam position
	private Vector3 oldCamPos;

	// Final value for rotation on y-axis
	private float rotationVelocity 		= 0.0f;

	// The value we increase the rotation every frame
	public float rotationSpeed 			= 0.0f;

	// Rotation by user for free cam 
	private float rotateX				= 0.0f;
	private float rotateY				= 0.0f;

	// Index for cam style via keyboard
	int index = 0;

	// Distance between cam and player in world units
	public float distanceToPlayer 		= 1.0f;

	// For distanceToPlayer lerp
	private float oldMinDistToPlayer 	= 0.0f;
	private float oldMaxDistToPlayer 	= 0.0f;
	public float minDistanceToPlayer	= 0.0f;
	public float maxDistanceToPlayer 	= 0.0f;
	public float lerpSpeed				= 0.0f;
	private float lerpedDistance		= 0.0f;
	private float lerpTimer				= 0.0f;
	
	// Camera styles for action cam:
	// - followLeft: 	look from the left to the player
	// - followAbove: 	look from above to the player
	// - followRight: 	look from the right to the player
	// - followBehind: 	look from behind to the player
	// - followHead: 	look through the eyes of the player
	// - circleAroundY: look at the player and circle around him (Y)
	// - circleAroundX: look at the player and circle around him (X)
	// - freeCam:		look at the player with free orbit control
	private enum ActionCam{followLeft, followRight, followAbove, followBehind, followFront, followHead,
		circleAroundY, circleAroundX, freeCam};

	// Enum object of ActionCam
	private ActionCam actionCam = ActionCam.followLeft;
	
	// Update cam styles by unity menue interface
	public bool followLeft 		= false;
	public bool followRight 	= false;
	public bool followAbove 	= false;
	public bool followBehind 	= false;
	public bool followFront 	= false;
	public bool followHead 		= false;
	public bool circleAroundY	= false;
	public bool circleAroundX 	= false;
	public bool freeCam 		= false;

	// Instance to network managing. Verifies that client has joind a server.
	public Netz nManager;

	// Access to player script
	private Player player;

	// Oculus rotation
	private Quaternion ovrRot;

	// Activate or deactivate oculus rotation processing by network
	public bool enableOVROrientation = false;

	// GUI
	bool showText = true;
	Rect textArea = new Rect(300,0,Screen.width, Screen.height);
	
	// Use this for initialization
	void Start () {
		// Find main camera with editor properties
		cam = GameObject.FindGameObjectWithTag ("MainCamera").transform;
		
		// Find networkManager
		nManager = (Netz)GameObject.FindGameObjectWithTag("Network").GetComponent("Netz");
		//Debug.Log(nManager.useOwnMasterServer);
		
		// Save properties by unity settings
		oldMinDistToPlayer = minDistanceToPlayer;
		oldMaxDistToPlayer = maxDistanceToPlayer;
		//Debug.Log ("oldMinDist: " + oldMinDistToPlayer + " oldMaxDist: " + oldMaxDistToPlayer);
	}
	
	// Update is called once per frame
	void Update () {

		// Keep from 0 - 359 (0 == 360)
		if (rotationVelocity % 359 == 0.0f)
			rotationVelocity = 0.0f;

		// Increase rotation value by rotationSpeed;
		rotationVelocity += rotationSpeed;

		// Check if client has joined a server.
		// This is necessary because the playerprefab is automatically generated.
		if (nManager.serverJoined) {
			try {
				player = (Player)GameObject.FindGameObjectWithTag ("Player").GetComponent("Player");
				
				if(enableOVROrientation)
					ovrRot = player.syncEndOVRRotation;//lerpedOVRRotation; syncEndOVRRotation
				else
					ovrRot = Quaternion.identity;
				//Debug.Log("ovrRot: " + ovrRot);
				target = GameObject.FindGameObjectWithTag ("Player").transform;
			} catch (UnityException e) {
				Debug.Log(e.Message);
			}
		}

		float foo1 = 0.0f, foo2 = 0.0f;

		Vector3 from 	= Vector3.zero;
		Vector3 to 		= Vector3.zero;
		// If a player has been generated we now can set up any of the provided camera styles.
		if(target){
			switch(actionCam){
			case ActionCam.followLeft: 
				cam.transform.position = target.transform.position +
					(target.transform.rotation * (Vector3.up * 1.5f)) +
					(target.transform.rotation * (Vector3.left * distanceToPlayer));
				from = target.transform.position + (target.transform.rotation * (Vector3.up * 1.5f));
				to 	 = cam.transform.position;
				cam.transform.rotation = Quaternion.LookRotation(from - to); 
				break;				
			case ActionCam.followRight: 
				cam.transform.position = target.transform.position +
					(target.transform.rotation * (Vector3.up * 1.5f)) +
					(target.transform.rotation * (Vector3.right * distanceToPlayer));
				from = target.transform.position + (target.transform.rotation * (Vector3.up * 1.5f));
				to 	 = cam.transform.position;
				cam.transform.rotation = Quaternion.LookRotation(from - to); 
				break;				
			case ActionCam.followAbove:
				cam.transform.position = target.transform.position +
					(target.transform.rotation * (Vector3.up * distanceToPlayer));
				cam.LookAt(target.transform);
				break;
			case ActionCam.followBehind: 
				cam.transform.position = target.transform.position +
					(target.transform.rotation * (Vector3.up * 1.5f)) +
					(target.transform.rotation * (Vector3.up * -distanceToPlayer)) +
					(target.transform.rotation * (Vector3.forward * -distanceToPlayer));
				from = target.transform.position + (target.transform.rotation * (Vector3.up * 1.5f));
				to 	 = cam.transform.position;
				cam.transform.rotation = Quaternion.LookRotation(from - to); 
				break;
			case ActionCam.followFront: 
				cam.transform.position = target.transform.position +
					(target.transform.rotation * (Vector3.up * 1.5f)) +
					(target.transform.rotation * (Vector3.up * distanceToPlayer / 10)) +
					(target.transform.rotation * (Vector3.forward * distanceToPlayer));
				from = target.transform.position + (target.transform.rotation * (Vector3.up * 1.5f));
				to 	 = cam.transform.position;
				cam.transform.rotation = Quaternion.LookRotation(from - to); 
				break;
			case ActionCam.followHead:
				cam.transform.rotation = enableOVROrientation ? cam.transform.rotation = ovrRot :  
																cam.transform.rotation = target.transform.rotation;
				cam.transform.position = target.transform.position + (target.transform.rotation * (Vector3.up * 1.8f));
				break;
			case ActionCam.circleAroundY:

				// Add delta time each frame to lerpTimer
				lerpTimer += Time.deltaTime;

				// Reset the lerp if max value has been reached
				if(lerpedDistance >= oldMaxDistToPlayer - 0.2f){ // small offset if lerp doesnt reach 25.0f
					// Reset lerp parameter
					minDistanceToPlayer = oldMinDistToPlayer;
					maxDistanceToPlayer = oldMaxDistToPlayer;

					// Reset lerped distance
					lerpedDistance = oldMinDistToPlayer;

					// Reset the lerp timer
					lerpTimer = 0.0f;

					// And finally set another cam style
					index = 7;
				}

				// Distance to player can be lerped now
				lerpedDistance = Mathf.Lerp(minDistanceToPlayer, maxDistanceToPlayer, lerpTimer * lerpSpeed);
				//Debug.Log("lerpDist: " + lerpedDistance);
				//Debug.Log ("time: " + lerpTimer);

				// Get target position and circle around it on y
				cam.transform.position = target.transform.position + (target.transform.rotation * (Vector3.up * 1.5f)) +
					new Vector3(Mathf.Sin(rotationVelocity / 180 * Mathf.PI) * lerpedDistance,
					            0.0f,
					            Mathf.Cos(rotationVelocity / 180 * Mathf.PI) * lerpedDistance);

				from = target.transform.position + (target.transform.rotation * (Vector3.up * 1.5f));
				to 	 = cam.transform.position;
				cam.transform.rotation = Quaternion.LookRotation(from - to); 
				break;
			case ActionCam.circleAroundX:

				// Add delta time each frame to lerpTimer
				lerpTimer += Time.deltaTime;
				
				// Reset the lerp if max value has been reached
				if(lerpedDistance >= oldMaxDistToPlayer - 0.2f){ // small offset if lerp doesnt reach 25.0f
					// Reset lerp parameter
					minDistanceToPlayer = oldMinDistToPlayer;
					maxDistanceToPlayer = oldMaxDistToPlayer;
					
					// Reset lerped distance
					lerpedDistance = oldMinDistToPlayer;
					
					// Reset the lerp timer
					lerpTimer = 0.0f;
					
					// And finally set another cam style
					index = 6;
				}
				
				// Distance to player can be lerped now
				lerpedDistance = Mathf.Lerp(minDistanceToPlayer, maxDistanceToPlayer, lerpTimer * lerpSpeed);

				// Get target position and circle around it on x
				cam.transform.position = target.transform.position + (target.transform.rotation * (Vector3.up * 1.5f)) +
					new Vector3(0.0f,
					            Mathf.Sin(rotationVelocity / 180 * Mathf.PI) * lerpedDistance,
					            Mathf.Cos(rotationVelocity / 180 * Mathf.PI) * lerpedDistance);

				from = target.transform.position + (target.transform.rotation * (Vector3.up * 1.5f));
				to 	 = cam.transform.position;
				cam.transform.rotation = Quaternion.LookRotation(from - to); 
				break;
			case ActionCam.freeCam: 

				cam.transform.position = target.transform.position + (target.transform.rotation * (Vector3.up * 1.5f)) +
					new Vector3(0.0f,
					            Mathf.Sin(rotateY / 180 * Mathf.PI) * distanceToPlayer,
					            Mathf.Cos(rotateY / 180 * Mathf.PI) * distanceToPlayer) + 
					new Vector3(Mathf.Sin(rotateX / 180 * Mathf.PI) * distanceToPlayer,
					            0.0f,
					            Mathf.Cos(rotateX / 180 * Mathf.PI) * distanceToPlayer);
				
				from = target.transform.position + (target.transform.rotation * (Vector3.up * 1.5f));
				to 	 = cam.transform.position;
				cam.transform.rotation = Quaternion.LookRotation(from - to); 


				/*cam.transform.position = RotatePointAroundPivot(cam.transform.position,
					                       						target.transform.position,
					                       						Quaternion.Euler(0, foo1++ * Time.deltaTime, 0));
				from = target.transform.position + (target.transform.rotation * (Vector3.up * 1.5f));
				to 	 = cam.transform.position;
				cam.transform.rotation = Quaternion.LookRotation(from - to);  */

				Debug.Log ("FreeCam");
				break;
			}
		}

		// Update Controls
		UpdateActionCamByMenue();
		UpdateActionCamByKeyboard();
		UpdateDistanceToPlayer ();
		UpdateFreeCam ();
	}
	
	// Set actionCam by unity menue. Bit ugly implementation but
	// needed for realtime cam style manipulation in the unity menue.
	private void UpdateActionCamByMenue(){
		if(followLeft)
			actionCam = ActionCam.followLeft;
		else if(followRight)
			actionCam = ActionCam.followRight;
		else if(followAbove)
			actionCam = ActionCam.followAbove;
		else if(followBehind)
			actionCam = ActionCam.followBehind;
		else if(followFront)
			actionCam = ActionCam.followFront;
		else if(followHead)
			actionCam = ActionCam.followHead;
		else if(circleAroundY)
			actionCam = ActionCam.circleAroundY;
		else if(circleAroundX)
			actionCam = ActionCam.circleAroundX;
		else if(freeCam)
			actionCam = ActionCam.freeCam;
		else // Default actionCam
			actionCam = ActionCam.followBehind;
		
	}
	
	// Set actionCam by device input
	private void UpdateActionCamByKeyboard(){
		if (Input.GetKeyDown (KeyCode.C))
			index++;

		if (index == 9)
			index = 0;

		//Debug.Log ("Actual actionCam index: " + index);

		switch (index) {
			case 0: actionCam = ActionCam.followLeft; break;
			case 1: actionCam = ActionCam.followRight; break;
			case 2: actionCam = ActionCam.followAbove; break;
			case 3: actionCam = ActionCam.followBehind; break;
			case 4: actionCam = ActionCam.followFront; break;
			case 5: actionCam = ActionCam.followHead; break;
			case 6: actionCam = ActionCam.circleAroundY; break;
			case 7: actionCam = ActionCam.circleAroundX; break;
			case 8: actionCam = ActionCam.freeCam; break;
		}
	}

	// Update distance to player (zoom)
	private void UpdateDistanceToPlayer(){
		if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Q))
		   distanceToPlayer += 0.1f;

		if(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.E))
		   distanceToPlayer -= 0.1f;
	}

	// Update rotation for free cam
	private void UpdateFreeCam(){
		if (rotateX >= 360) rotateX = 0.0f;
		if (rotateY >= 360) rotateY = 0.0f;
		if (Input.GetKey (KeyCode.W)) rotateY += 1.0f;		
		if (Input.GetKey (KeyCode.S)) rotateY -= 1.0f;
		if(Input.GetKey(KeyCode.A)) rotateX -= 1.0f;
		if(Input.GetKey(KeyCode.D)) rotateX += 1.0f;
	}

	public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion angle) {
		return angle * ( point - pivot) + pivot;
	}

    private void OnGUI() {
		//if(nManager.serverJoined && showText)
			//GUI.Label(textArea, ovrRot.ToString());
    }
}
