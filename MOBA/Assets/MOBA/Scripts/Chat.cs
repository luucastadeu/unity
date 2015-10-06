using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Chat : MonoBehaviour {
	private string MensagemParaEnviar = "";
	private Vector2 PosicaoDoScroll;
	List<string> Historico = new List<string>();
	public Rect chatRect;
	public GUISkin skin;
	public bool typing = true;

	// Use this for initialization
	void Start () {
		chatRect = new Rect((Screen.width / 2) - 180, 235, 360, 150);
		PosicaoDoScroll = Vector2.zero;
	}

	void OnGUI() {
		GUI.skin = skin;

		if (NetworkPeerType.Disconnected != Network.peerType) {
			GUILayout.BeginArea(chatRect);
			PosicaoDoScroll = GUILayout.BeginScrollView (PosicaoDoScroll, GUILayout.Width (360), GUILayout.Height (95));
			
			GUILayout.FlexibleSpace();
			for(int i = 0; i < Historico.Count; i++){
				GUILayout.Label(Historico[i]);
			}
			
			GUILayout.EndScrollView();
			
			GUILayout.Space(5);
			
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return && MensagemParaEnviar != "" && GUI.GetNameOfFocusedControl() == "Message") {
				GetComponent<NetworkView>().RPC("EnviaMsg", RPCMode.All, Network.player.ToString(), MensagemParaEnviar);
				MensagemParaEnviar = "";
				GUI.FocusControl("");
				Event.current.Use();
				if(transform.GetComponent<NetworkManager>().playing)
					typing = false;
			}
			
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return && MensagemParaEnviar == "" && GUI.GetNameOfFocusedControl() == "Message") {
				GUI.FocusControl("");
				Event.current.Use();
				if(transform.GetComponent<NetworkManager>().playing)
					typing = false;
			}
			
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() != "Message") {
				if(transform.GetComponent<NetworkManager>().playing) {
					typing = true;
				}
				GUI.FocusControl("Message");
				Event.current.Use();
			}
			
			if(typing) {
				GUI.SetNextControlName("Message");
				MensagemParaEnviar = GUILayout.TextField(MensagemParaEnviar);
			}
			
			GUILayout.EndArea();
		}
	}

	[RPC]
	void EnviaMsg (string playerID, string msg) {
		Historico.Add(NetworkManager.players.getPlayerNameByID(playerID) + ": " + msg);
		PosicaoDoScroll.y = 10000;
	}

	[RPC]
	void EnviaSelecao (string playerID, string msg) {
		Historico.Add("<" + NetworkManager.players.getPlayerNameByID(playerID) + msg + ">");
		PosicaoDoScroll.y = 10000;
	}
}
