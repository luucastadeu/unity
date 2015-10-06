using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Players {
	public string ID { get; set; }
	public string playerName { get; set; }
	public bool isReady { get; set; }

	public Players(string ID, string playerName, bool isReady) {
		this.ID = ID;
		this.playerName = playerName;
		this.isReady = isReady;
	}
}