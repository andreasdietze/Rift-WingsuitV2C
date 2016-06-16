using UnityEngine;
using System.Collections;

public class ViewIDAllocator : MonoBehaviour {

	/* 
	 * http://docs.unity3d.com/ScriptReference/Network.AllocateViewID.html
	 * 
	 * Auch hier wird ein Objekt mit einer falschen View-ID erstellt (falsche ID + 1...)
	 * Lösung: Das Objekt muss direkt vom Prefab des Servers stammen. 
	 * Mit manuell erstellte Komponenten kann es zu Fehlern kommen.
	 * Das Script wird von daher nicht mehr benötigt, dient aber weiterhin als Bsp. 
	 * für Objekterzeugung über eine viewID und eine RPC Aufruf (daher alte ID + 1)
	 */

	public Transform cubePrefab;
	public NetworkView nView;

	void Start() {
		nView = GetComponent<NetworkView> ();
	}

	void OnGUI() {
		/*if (GUI.Button (new Rect (100, 300, 250, 100), "Spawn player")) {
			NetworkViewID viewID = Network.AllocateViewID();
			nView.RPC("SpawnBox", RPCMode.AllBuffered, viewID, transform.position);
			Debug.Log(viewID);
		}*/

	}
	[RPC]
	void SpawnBox(NetworkViewID viewID, Vector3 location) {
		Transform clone;
		clone = Instantiate(cubePrefab, location, Quaternion.identity) as Transform as Transform;
		NetworkView nView;
		nView = clone.GetComponent<NetworkView>();
		nView.viewID = viewID;
	}
}
