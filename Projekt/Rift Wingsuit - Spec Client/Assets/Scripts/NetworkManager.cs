using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour
{
	// Lobby settings
	private const string typeName = "RiftWingsuit";
	private const string gameName = "Wingsuit-Lobby";
	
	// Host data for global unity master server
	// Note: global -> global unity master server
	private bool isRefreshingHostList = false;
	private HostData[] hostList;
	
	// Player prefab and spawn settings (by cam)
	public GameObject playerPrefab;
	public Transform camPos;
	
	// User for seperate master server
	// Note: need to set your own ip (local or net)
	public bool useOwnMasterServer = true;
	private string omsip = "192.168.0.194";   //"192.168.0.194";  GameController.instance.ip;
	private int omsport = 25000;
	
	// Server joined flag for instantiating player prefab
	public bool serverJoined = false;
	
	// Provide client network gui (todo: extend GUI !!!!)
	void OnGUI(){
		// Network connection to global or own master server
		if (!Network.isClient && !Network.isServer){
			if (GUI.Button(new Rect(100, 100, 250, 100), "Refresh Hosts")){
				if(useOwnMasterServer)
					Network.Connect(omsip, omsport);
				else
					RefreshHostList();
			}
			
			// Only needed for multiple game servers via global master server
			if (hostList != null && !useOwnMasterServer){
				for (int i = 0; i < hostList.Length; i++){
					if (GUI.Button(new Rect(400, 100 + (110 * i), 300, 100), hostList[i].gameName))
						JoinServer(hostList[i]);
				}
			}
		}
	}
	
	// Register host at global
	private void StartServer(){
		Network.InitializeServer(2, // Players
		                         25000, // Port
		                         !Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName, gameName);
	}
	
	// Update server list from global 
	void Update(){
		if (isRefreshingHostList && MasterServer.PollHostList().Length > 0){
			isRefreshingHostList = false;
			hostList = MasterServer.PollHostList();
		}
	}
	
	// Refresh global
	private void RefreshHostList(){
		if (!isRefreshingHostList){
			isRefreshingHostList = true;
			MasterServer.RequestHostList(typeName);
		}
	}
	
	// Global event, get host list
	void OnMasterServerEvent(MasterServerEvent msEvent){
		if (msEvent == MasterServerEvent.HostListReceived)
			hostList = MasterServer.PollHostList();
	}
	
	// Join global
	private void JoinServer(HostData hostData){
		Network.Connect(hostData);
	}
	
	// Set server joined flag in any case
	void OnConnectedToServer(){
		if(useOwnMasterServer)
			Debug.Log("Server Joined | OMS");
		else
			Debug.Log("Server Joined | Global");
		
		serverJoined = true;
		//SpawnPlayer();
	}
	
	// Not used now/anymore
	private void SpawnPlayer(){
		Network.Instantiate(playerPrefab, camPos.position, Quaternion.identity, 0);   // Quaternion.identity
		Debug.Log("Player has initialized");
	}
}