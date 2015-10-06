using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NetworkManager : MonoBehaviour {
	private const string typeName = "Modo padrÃ£o";
	public string gameName;
	public string gamePort;
	private string connectIP;
	private string connectPort = "";
	public string playerName;

	private HostData[] hostList;

	public bool isHome = true;
	public bool atLobby = false;
	public int playersReady = 0;
	public bool allReady = false; //esse atualiza no update
	public bool imReady = false;
	public bool playing = false;

	public static ListPlayersList players = new ListPlayersList();

	public GUISkin skin;

	public GameObject[] playerPrefabs;
	public GameObject[] spawnPoints;


	enum Heroes {cubo, esfera}
	private int selGridInt = 0;
	private string[] selStrings = new string[] {"Cubo", "Esfera"};
	private bool choosingHero = true;
	Heroes currentHero; //muda ao clicar em escolher e faz switch no SpawnPlayer()

	
	void Start () {
		if(Application.loadedLevelName == "Home")
			DontDestroyOnLoad(transform.gameObject);
	}

	// Update is called once per frame
	void Update () {
		if(isHome) {
			MasterServer.RequestHostList(typeName);
		}

		if(Network.peerType == NetworkPeerType.Server) {
			if(playersReady == Network.connections.Length)
				allReady = true;
			else
				allReady = false;
		}
	}

	//Inicializa o servidor e encaminha para o Lobby
	private void StartServer() {
		Network.InitializeServer(4, int.Parse(gamePort), Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName, gameName);
	}

	void OnServerInitialized(NetworkPlayer player) {
		atLobby = true;
		isHome = false;
		GetComponent<NetworkView>().RPC("AddPlayerOnList", RPCMode.AllBuffered, player.ToString(), playerName, true);
	}

	void OnConnectedToServer() {
		GameObject.Find("Canvases").transform.FindChild("Tela Inicial").gameObject.SetActive(false);
		GameObject.Find("Canvases").transform.FindChild("Lobby (Client)").gameObject.SetActive(true);
		GetComponent<NetworkView>().RPC("AddPlayerOnList", RPCMode.AllBuffered, Network.player.ToString(), playerName, false);
		atLobby = true;
		isHome = false;
	}

	//Spawna os jogadores
	private void SpawnPlayer() {
		spawnPoints = GameObject.FindGameObjectsWithTag("PlayerSpawnPoints");
		int spawnPointIndex = int.Parse(Network.player.ToString());
		isHome = true;
		atLobby = false;
		playing = true;
		transform.GetComponent<Chat>().typing = false;
		Network.Instantiate(playerPrefabs[selGridInt], spawnPoints[spawnPointIndex].transform.position, spawnPoints[spawnPointIndex].transform.rotation, 0);
		/*switch(currentHero) {
			case Heroes.cubo:
				Network.Instantiate(playerPrefabs[selGridInt], spawnPoints[spawnPointIndex].transform.position, spawnPoints[spawnPointIndex].transform.rotation, 0);
				Debug.Log("Iniciou com Cubo (0)");
				break;
			case Heroes.esfera:
				Network.Instantiate(playerPrefabs[selGridInt], spawnPoints[spawnPointIndex].transform.position, spawnPoints[spawnPointIndex].transform.rotation, 0);
				Debug.Log("Iniciou com Esfera (1)");
				break;
		}*/
	}

	void OnDisconnectedFromServer(NetworkDisconnection info) {
		Destroy(transform.gameObject);
		Application.LoadLevel(0);
	}

	void OnPlayerDisconnected(NetworkPlayer player) {
		print("Jogador " + player.ipAddress + ":" + player.port + " desconectado");
	}

	void OnLevelWasLoaded(int level) {
		if (level == 1) {
			SpawnPlayer();
			this.GetComponent<Chat>().chatRect = new Rect((Screen.width / 2) - 180, (Screen.height - 150), 360, 150);
		}
		if (level == 0) {
			atLobby = false;
			isHome = true;
			playing = false;
			this.GetComponent<Chat>().chatRect = new Rect((Screen.width / 2) - 180, 235, 360, 150);
			GameObject.Find("Canvases").transform.FindChild("Tela Inicial").gameObject.SetActive(true);
			GameObject.Find("Canvases").transform.FindChild("Fixo").gameObject.SetActive(true);
		}
	}
	
	//GUI
	void OnGUI()
	{
		GUI.skin = skin;
		
		#region Tela inicial
		if(Network.peerType == NetworkPeerType.Disconnected) {
			hostList = MasterServer.PollHostList ();
			if (hostList != null && isHome) {
				for (int i = 0; i < hostList.Length; i++) {
					if(playerName == "") {
						GUI.enabled = false;
						if (GUI.Button(new Rect(((Screen.width / 2) + 35), 140 + (40 * i), 160, 30), hostList[i].gameName)) { //arrumar
						}
						GUI.enabled = true;
					}
					else {
						if (GUI.Button(new Rect(((Screen.width / 2) + 35), 140 + (40 * i), 160, 30), hostList[i].gameName)) {
							JoinServer(hostList[i]);
							GameObject.Find("Canvases").transform.FindChild("Lobby (Client)").gameObject.SetActive(true);
						}
					}
				}
			}
		}
		#endregion

		#region Status labels
		//Tela Inicial
		if (Network.peerType == NetworkPeerType.Disconnected) {
			GUILayout.Label("Desconectado");
		}

		//Conectando
		if (Network.peerType == NetworkPeerType.Connecting) {
			GUILayout.Label("Conectando...");
		}

		//Client in-game
		if (Network.peerType == NetworkPeerType.Client && isHome) {
			GUILayout.Label("Tipo: Client");
			GUILayout.Label("Jogadores: " + (Network.connections.Length + 1));
			GUILayout.Label("Ping: " + Network.GetAveragePing(Network.connections[0]));
			GUILayout.Label("Seu IP: " + Network.player.externalIP + ":" + Network.player.port);
		}

		//Server in-game
		if (Network.peerType == NetworkPeerType.Server && isHome) {
			GUILayout.Label("Tipo: Server");
			GUILayout.Label("Jogadores: " + (Network.connections.Length + 1));
			GUILayout.Label("Seu IP: " + Network.player.externalIP + ":" + Network.player.port);
		}

		//Client no lobby
		if (Network.peerType == NetworkPeerType.Client && atLobby) {
			GUILayout.Label("Tipo: Client");
			GUILayout.Label("Jogadores conectados: " + (Network.connections.Length + 1));
			GUILayout.Label("Jogadores prontos: " + (playersReady + 1));
			GUILayout.Label("Ping: " + Network.GetAveragePing(Network.connections[0]));
			GUILayout.Label("Seu IP: " + Network.player.externalIP + ":" + Network.player.port);
		}

		//Server no lobby
		if (Network.peerType == NetworkPeerType.Server && atLobby) {
			GUILayout.Label("Tipo: Server");
			GUILayout.Label("Jogadores conectados: " + (Network.connections.Length + 1));
			GUILayout.Label("Jogadores prontos: " + (playersReady + 1));
			GUILayout.Label("Seu IP: " + Network.player.externalIP + ":" + Network.player.port);
		}

		//Dados do jogador
		if (atLobby) {
			Players temp = players.runList(Network.player.ToString());
			GUILayout.Label("Seu ID: " + temp.ID);
			GUILayout.Label("Seu nome: " + temp.playerName);
			GUILayout.Label("Seu status: " + temp.isReady.ToString());
			GUILayout.Label("Jogadores:");
			players.writePlayersInfo();

			GUILayout.BeginArea(new Rect((Screen.width / 2) - 180, 145, 360, 50));
			GUILayout.BeginHorizontal();

			if (choosingHero) {
				selGridInt = GUILayout.SelectionGrid(selGridInt, selStrings, 2);

				if (GUILayout.Button("Escolher")) {
					currentHero = (Heroes)selGridInt;
					this.GetComponent<Chat>().GetComponent<NetworkView>().RPC("EnviaSelecao", RPCMode.All, Network.player.ToString(), " escolheu " + selStrings[selGridInt]);
					choosingHero = false;
				}
			}

			if (!choosingHero) {
				GUI.enabled = false;
				selGridInt = GUILayout.SelectionGrid(selGridInt, selStrings, 2);
				GUI.enabled = true;

				if (GUILayout.Button("Retornar")) {
					this.GetComponent<Chat>().GetComponent<NetworkView>().RPC("EnviaSelecao", RPCMode.All, Network.player.ToString(), " cancelou sua escolha");
					choosingHero = true;
				}
			}

			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
		#endregion
	}

	#region RPCs
	//RPC do Client
	[RPC]
	void SendReadyToServer() {
		GetComponent<NetworkView>().RPC("ReceiveInfoFromClient", RPCMode.Server, true);
	}

	[RPC]
	void AddPlayerOnList(string player, string playerName, bool ready) {
		players.AddPlayer(player, playerName, ready);
	}

	[RPC]
	void ChangeReady(string player, bool ready){
		players.changeReady(player, ready);
	}

	//RPC do Server
	[RPC]
	void ReceiveInfoFromClient(bool heIsReady) {
		if (heIsReady)
			playersReady++;
		else
			playersReady--;
	}

	[RPC]
	void StartMatchToAll() {
		Application.LoadLevel(1);
	}
	#endregion

	//Conexao direta por IP
	void ConnectToServer() {
		Network.Connect(connectIP, int.Parse(connectPort)); //aqui da pra colocar senha como string
	}

	void OnFailedToConnect(NetworkConnectionError error) {
		if (error == NetworkConnectionError.ConnectionFailed)
			Debug.Log("A conexao falhou");
	}

	//Conexao pela lista de servidores
	void OnMasterServerEvent(MasterServerEvent msEvent) {
		if (msEvent == MasterServerEvent.HostListReceived)
			hostList = MasterServer.PollHostList ();
	}

	private void JoinServer(HostData hostData) {
		Network.Connect (hostData);
	}

	#region Buttons
	//Tela Inicial
	public void Quit() {
		Application.Quit();
	}

	public void PlayerName(GameObject input) {
		playerName = input.GetComponent<InputField>().text;
		if(playerName == "") {
			GameObject.Find("Botao_Nova Sala").GetComponent<Button>().interactable = false;
			GameObject.Find("Botao_IP Direto").GetComponent<Button>().interactable = false;
		}
		else {
			GameObject.Find("Botao_Nova Sala").GetComponent<Button>().interactable = true;
			GameObject.Find("Botao_IP Direto").GetComponent<Button>().interactable = true;
		}
	}

	public void LeaveHome() {
		isHome = false;
	}

	public void ReturnHome() {
		isHome = true;
	}

	//Tela de CriaÃ§ao de Sala
	public void StartServerButton(GameObject canvas) {
		StartServer();
		canvas.SetActive(false);
		GameObject.Find("Canvases").transform.FindChild("Lobby (Server)").gameObject.SetActive(true);
	}

	public void GameName(GameObject input) {
		gameName = input.GetComponent<InputField>().text;
		if(gameName == "" || gamePort == "") {
			GameObject.Find("Botao_Criar").GetComponent<Button>().interactable = false;
		}
		else {
			GameObject.Find("Botao_Criar").GetComponent<Button>().interactable = true;
		}
	}

	public void GamePort(GameObject input) {
		gamePort = input.GetComponent<InputField>().text;
		if(gamePort == "" || gameName == "") {
			GameObject.Find("Botao_Criar").GetComponent<Button>().interactable = false;
		}
		else {
			GameObject.Find("Botao_Criar").GetComponent<Button>().interactable = true;
		}
	}

	//Lobby
	public void LeaveRoomButton(GameObject canvas) {
		Network.Disconnect();
		canvas.SetActive(false);
	}

	public void SetReady(GameObject button) {
		GetComponent<NetworkView>().RPC("ChangeReady", RPCMode.AllBuffered, Network.player.ToString(), true);
		button.SetActive(false);
	}

	public void StartMatch() {
		GetComponent<NetworkView>().RPC("StartMatchToAll", RPCMode.All);
	}

	//Tela de Conexao por IP
	public void ConnectIP(GameObject input) {
		connectIP = input.GetComponent<InputField>().text;
		Debug.Log(connectPort);
		if(connectIP == "" || connectPort == "") {
			GameObject.Find("Botao_Conectar").GetComponent<Button>().interactable = false;
		}
		else {
			GameObject.Find("Botao_Conectar").GetComponent<Button>().interactable = true;
		}
	}

	public void ConnectPort(GameObject input) {
		connectPort = input.GetComponent<InputField>().text;
		if(connectPort == "" || connectIP == "") {
			GameObject.Find("Botao_Conectar").GetComponent<Button>().interactable = false;
		}
		else {
			GameObject.Find("Botao_Conectar").GetComponent<Button>().interactable = true;
		}
	}

	public void ConnectButton() {
		ConnectToServer();
	}
	#endregion
}