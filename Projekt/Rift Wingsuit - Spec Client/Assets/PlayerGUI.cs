using UnityEngine;
using System.Collections;

public class PlayerGUI : MonoBehaviour {
	GUIStyle font;
	NetworkManager nManager;
	Player player;

	// Use this for initialization
	void Start () {
		font = new GUIStyle ();
		font.fontSize = 32;
		font.normal.textColor = Color.white;  
		nManager = (NetworkManager)GameObject.FindGameObjectWithTag("Network").GetComponent("NetworkManager");
	}
	
	void OnGUI(){
		if(nManager.serverJoined)
			player = (Player)GameObject.FindGameObjectWithTag ("Player").GetComponent("Player");
			GUI.Label(new Rect(10, 10, 150, 150), "Score : " +  player.score, font);
	}
}
