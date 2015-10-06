using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ListPlayersList : List<Players> {

	public Players runList(string playerID) {
		Players temp = null;
		foreach(Players tmp in this) {
			if(tmp.ID == playerID) {
				temp = tmp;
			}
		}
		return temp;
	}
	
	public void changeReady(string playerID, bool ready) {
		Players temp = this.runList(playerID);
		if(temp != null) {
			this.Remove(temp);
			temp.isReady = ready;
			this.Add(temp);
		}
	}
	
	public bool isAllReady() {
		foreach(Players temp in this)
			if(!temp.isReady)
				return false;
		return true;
	}
	
	public string[] playersName() {
		string[] nomes = new string[this.Count];
		for(int i = 0;i <= this.Count - 1;i++) {
			nomes[i] = this[i].playerName;
		}
		return nomes;
	}
	
	public string[] playersReady() {
		string[] prontos = new string[this.Count];
		for(int i = 0;i <= this.Count - 1;i++) {
			prontos[i] = this[i].isReady.ToString();
		}
		return prontos;
	}
	
	public void writePlayersInfo() {
		for(int i = 0;i <= this.Count - 1;i++) {
			GUILayout.Label(this[i].playerName + ": " + this.playersReady()[i]);
		}
	}
	
	public string getPlayerNameByID(string ID) {
		string name = "";
		for(int i = 0;i <= this.Count - 1;i++) {
			if(this[i].ID == ID) {
				name = this[i].playerName;
			}
		}
		return name;
	}

	public void AddPlayer (string playerID, string playerName, bool isReady) {
		base.Add(new Players(playerID, playerName, isReady));
	}
}
