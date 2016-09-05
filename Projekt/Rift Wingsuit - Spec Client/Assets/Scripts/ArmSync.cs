using UnityEngine;
using System.Collections;

public class ArmSync : MonoBehaviour
{
    // Script access
    private Netz nManager;
    private Player player;

    // Arm transformation
    public float rotY = 0.0f;
    public float rotZ = 0.0f;
    private Transform leftShoulder;
    private Transform rightShoulder;
    private Transform playerMesh;

    void Start()
    {
        // Find networkManager
        nManager = (Netz)GameObject.FindGameObjectWithTag("Network").GetComponent("Netz");
        leftShoulder = GameObject.Find("ShoulderL").transform;
        rightShoulder = GameObject.Find("ShoulderR").transform;
        player = (Player)GameObject.FindGameObjectWithTag("Player").GetComponent("Player");
    }

    // Update is called once per frame
    void Update()
    {
        // Check if client has joined a server.
        // This is necessary because the playerprefab is automatically generated.
        if (nManager.serverJoined) //serverJoined)
        {
            try
            {
                //player.transform.rotation = player.transform.rotation * 
                  //  Quaternion.Euler(new Vector3(0.0f, -rotY, 0.0f));
                leftShoulder.rotation = player.transform.rotation * 
                    Quaternion.Euler(new Vector3(0.0f, 0.0f, 90f + (rotZ * 50)));
                rightShoulder.rotation = player.transform.rotation * 
                    Quaternion.Euler(new Vector3(0.0f, 0.0f, -90.0f - (rotZ * 50)));
            }
            catch (UnityException e)
            {
                Debug.Log(e.Message);
            }
        }
    }

    bool showText = true;
    Rect textArea = new Rect(300, 60, Screen.width, Screen.height);

    private void OnGUI()
    {
        //if(nManager.serverJoined && showText)
          //GUI.Label(textArea, headOrientation.ToString());
		//GUI.Label(textArea, rotY.ToString());
		//GUI.Label(textArea, rotZ.ToString());
		//Debug.Log(rotY.ToString());
		//Debug.Log(rotZ.ToString());
		//Debug.Log(player.transform.rotation);
    }

}
