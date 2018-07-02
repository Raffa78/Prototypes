using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NinjasPlayersManager : MonoBehaviour
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

	void OnJoinedRoom()
	{

		SpawnLocalPlayer();

		//PhotonNetwork.sendRate = 30;
		//PhotonNetwork.sendRateOnSerialize = 30; 
	}

	void SpawnLocalPlayer()
	{
		Vector3 position = GameObject.Find("Spawn").transform.position;
		GameObject go = PhotonNetwork.Instantiate("NinjasPlayer", position, Quaternion.identity, 0);

		pv.RPC("PlayerSpawned", PhotonTargets.All, PhotonNetwork.player.ID);

		NinjasPlayer player = go.GetComponent<NinjasPlayer>();
		player.OnDeath += OnPlayerDeath;
	}

	[PunRPC]
	public void PlayerSpawned(int playerID)
	{
		//TODO: add player pane
		GameObject go = GameObject.Instantiate(playerPane);
		go.transform.parent = playerBoard.transform;

		PlayerInfo playerInfo = new PlayerInfo();
		playerInfo.playerPane = go;
		//playerInfo.name = PhotonPlayer.Find(playerID).NickName;
		if(PhotonPlayer.Find(playerID) == PhotonNetwork.player)
		{
			playerInfo.name = "ME";
		}
		else
		{
			playerInfo.name = "OTHER";
		}

		playerInfo.score = 0;
		playerDic.Add(playerID, playerInfo);

		go.transform.Find("Name").GetComponent<Text>().text = playerInfo.name;
		go.transform.Find("Score").GetComponent<Text>().text = playerInfo.score.ToString();
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
