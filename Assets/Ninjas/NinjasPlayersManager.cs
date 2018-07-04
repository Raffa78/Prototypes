using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NinjasPlayersManager : Photon.PunBehaviour
{

	public GameObject playerPane;
	public GameObject playerBoard;

	struct PlayerInfo
	{
		public GameObject playerPane;
		public string name;
		public int score;
	}

	private Dictionary<int, PlayerInfo> playerDic;

	private Dictionary<string, UnityEvent> eventDictionary;

	private static NinjasPlayersManager eventManager;

	private PhotonView pv;

	public static NinjasPlayersManager instance
	{
		get
		{
			if (!eventManager)
			{
				eventManager = FindObjectOfType(typeof(NinjasPlayersManager)) as NinjasPlayersManager;

				if (!eventManager)
				{
					Debug.LogError("There needs to be one active NinjasPlayersManager script on a GameObject in your scene.");
				}
				else
				{
					//eventManager.Init();
				}
			}

			return eventManager;
		}
	}


	void Start()
	{
		pv = GetComponent<PhotonView>();

		playerDic = new Dictionary<int, PlayerInfo>();

		if (eventDictionary == null)
		{
			eventDictionary = new Dictionary<string, UnityEvent>();
		}
	}

	public override void OnJoinedRoom()
	{
		base.OnJoinedRoom();

		SpawnLocalPlayer();
		
		foreach(PhotonPlayer player in PhotonNetwork.playerList)
		{
			AddPlayerToPanel(PhotonNetwork.player.ID);
		}

		//PhotonNetwork.sendRate = 30;
		//PhotonNetwork.sendRateOnSerialize = 30; 
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		base.OnPhotonPlayerConnected(newPlayer);

		AddPlayerToPanel(newPlayer.ID);
	}

	void SpawnLocalPlayer()
	{
		Vector3 position = GameObject.Find("Spawn").transform.position;
		GameObject go = PhotonNetwork.Instantiate("NinjasPlayer", position, Quaternion.identity, 0);

		NinjasPlayer player = go.GetComponent<NinjasPlayer>();
		player.OnDeath += OnPlayerDeath;
	}
	
	public void AddPlayerToPanel(int playerID)
	{
		//TODO: add player pane
		GameObject go;
		PlayerInfo pInfo;
		
		go = GameObject.Instantiate(playerPane);
		go.transform.parent = playerBoard.transform;

		pInfo = new PlayerInfo();
		pInfo.playerPane = go;

		if (PhotonPlayer.Find(playerID) == PhotonNetwork.player)
		{
			pInfo.name = "ME";
		}
		else
		{
			pInfo.name = "OTHER";
		}

		playerDic.Add(playerID, pInfo);

		pInfo.score = 0;
		

		go.transform.Find("Name").GetComponent<Text>().text = pInfo.name;
		go.transform.Find("Score").GetComponent<Text>().text = pInfo.score.ToString();
	}

	void OnPlayerDeath(NinjasPlayer player, int killingPlayerID)
	{
		pv.RPC("PlayerWasKilled", PhotonTargets.All, PhotonNetwork.player.ID, killingPlayerID);
		
		SpawnLocalPlayer();
	}

	[PunRPC]
	public void PlayerWasKilled(int killedPlayerID, int killingPlayerID)
	{
		//Update player score
		PlayerInfo pInfo = new PlayerInfo();
		playerDic.TryGetValue(killingPlayerID, out pInfo);

		pInfo.score++;
		pInfo.playerPane.transform.Find("Score").GetComponent<Text>().text = pInfo.score.ToString();


	}

	public static void StartListening(string eventName, UnityAction listener)
	{
		UnityEvent thisEvent = null;
		if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.AddListener(listener);
		}
		else
		{
			thisEvent = new UnityEvent();
			thisEvent.AddListener(listener);
			instance.eventDictionary.Add(eventName, thisEvent);
		}
	}

	public static void StopListening(string eventName, UnityAction listener)
	{
		if (eventManager == null) return;
		UnityEvent thisEvent = null;
		if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.RemoveListener(listener);
		}
	}

	public static void TriggerEvent(string eventName)
	{
		UnityEvent thisEvent = null;
		if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.Invoke();
		}
	}
	
}
