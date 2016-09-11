using UnityEngine;
using System.Collections;

// Players class job is to receive data from network
public class Player : MonoBehaviour
{
	public float speed = 10f;
	
	private float lastSynchronizationTime = 0f;
	private float syncDelay = 0f;
	private float syncTime = 0f;
	
	// Start- and endposition for lerp
	private Vector3 syncStartPosition = Vector3.zero;
	private Vector3 syncEndPosition = Vector3.zero;
	
	// Start- and endrotation for lerp
	private Quaternion syncEndRotation = Quaternion.identity;
	private Quaternion syncStartRotation = Quaternion.identity;
	
	// OVR cam fin orientation
	public Quaternion syncEndOVRRotation = Quaternion.identity;
	private Quaternion syncStartOVRRotation = Quaternion.identity;
	public Quaternion lerpedOVRRotation = Quaternion.identity;
	
	// Kinect values
	private float syncEndDeltaY = 0f;
	private float syncStartDeltaY = 0f;

	private float syncEndDeltaZ = 0f;
	private float syncStartDeltaZ = 0f;
	
	// Player status
	public int score = 0;
	public int finScore =0 ;
	
	// Helper
	public ArmSync armSync = null;
	
	// Only receive and process data
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info){
		Vector3 syncPosition = Vector3.zero;
		Vector3 syncVelocity = Vector3.zero;
		Quaternion syncRotation = Quaternion.identity;
		Quaternion syncOVRRotation = Quaternion.identity;
		float syncDeltaY = 0f;
		float syncDeltaZ = 0f;
		int syncScore = 0;
	
		if (stream.isWriting){ // Send data
			/*syncPosition = GetComponent<Rigidbody>().position;
			stream.Serialize(ref syncPosition);

			syncVelocity = GetComponent<Rigidbody>().velocity;
			stream.Serialize(ref syncVelocity);
			
			syncRotation = GetComponent<Rigidbody>().rotation;
			stream.Serialize(ref syncRotation);*/
		}
		else {// Receive data
			stream.Serialize(ref syncPosition);
			stream.Serialize(ref syncVelocity);
			stream.Serialize(ref syncRotation);
			// OVR cam view has only to be received
			stream.Serialize(ref syncOVRRotation);
			// Also Kinect values
			stream.Serialize(ref syncDeltaY);
			stream.Serialize(ref syncDeltaZ);
			stream.Serialize(ref syncScore);
			
			syncTime = 0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;
			
			syncEndPosition = syncPosition + syncVelocity * syncDelay;
			syncStartPosition = GetComponent<Rigidbody>().position;
			
			syncEndRotation = syncRotation;
			syncStartRotation = GetComponent<Rigidbody>().rotation;
			
			// at the moment player rotates by rift -> this rot is for head only -> works
			syncEndOVRRotation = syncOVRRotation; 
			syncStartOVRRotation = GetComponent<Rigidbody>().rotation; // * Quaternion.Euler(90f, 0f, 0f);
			
			syncEndDeltaY = syncDeltaY;
			syncStartDeltaY = armSync.rotY;
			
			syncEndDeltaZ = syncDeltaZ;
			syncStartDeltaZ = armSync.rotZ;			
			
			score = syncScore;
			//Debug.Log(syncScore);
		}
	}
	
	void Awake(){
		lastSynchronizationTime = Time.time;
	}
	
	void Update(){
		if (GetComponent<NetworkView>().isMine){
			;//InputColorChange();
		}
		else{
			SyncedMovement();
		}
	}
	
	// Sync the movement due to 4 fps network
	private void SyncedMovement(){
		syncTime += Time.deltaTime;
		GetComponent<Rigidbody>().position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
		GetComponent<Rigidbody>().rotation =  Quaternion.Lerp(syncStartRotation, syncEndRotation, syncTime / syncDelay);
		lerpedOVRRotation = Quaternion.Slerp(syncStartOVRRotation, syncEndOVRRotation, syncTime / syncDelay);
		armSync.rotY = Mathf.Lerp(syncStartDeltaY, syncEndDeltaY, syncTime / syncDelay);
		armSync.rotZ = Mathf.Lerp(syncStartDeltaZ, syncEndDeltaZ, syncTime / syncDelay);
	}
	
	
	// RPC-Beispiel bitte drinne lassen
	/* private void InputColorChange()
    {
        if (Input.GetKeyDown(KeyCode.R))
            ChangeColorTo(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
    }

    [RPC] void ChangeColorTo(Vector3 color)
    {
        GetComponent<Renderer>().material.color = new Color(color.x, color.y, color.z, 1f);

        if (GetComponent<NetworkView>().isMine)
            GetComponent<NetworkView>().RPC("ChangeColorTo", RPCMode.OthersBuffered, color);
    }*/
}
