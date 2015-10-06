using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player : MonoBehaviour {

	private float lastSynchronizationTime = 0f;
	private float syncDelay = 0f;
	private float syncTime = 0f;
	private Vector3 syncStartPosition = Vector3.zero;
	private Vector3 syncEndPosition = Vector3.zero;
	private NavMeshAgent agent;
	private bool isPause = false;
	private Rect menuWindowRect = new Rect ((Screen.width / 2 - 60), (Screen.height / 2 - 25), 120, 50);
	
	public float maxHealth = 30;
	public float curHealth;
	private Slider healthBar;
	private Quaternion canvasRotation;

	public GUISkin skin;

	public Transform target;
	public Transform activeTarget;
	public bool mouseOverSomething = false;
	public bool isFollowing = false;

	private float attackInterval = 1.4f;
	private float nextAttack = 0;
	public float meleeDamage = 2;

	private float regenInterval = 2.2f;
	private float nextRegen = 0;
	public float regenRate = 1;
	public bool onAction = false;

	private Vector3 screenPos;

	void Exit() {
		Network.Disconnect();
		isPause = false;
	}

	void StopServer() {
		Network.Disconnect();
		isPause = false;
	}

	void OnDisconnectedFromServer() {
		Destroy(gameObject);
	}
	
	private void PauseMenu(int windowID) {
		if (Network.isClient) {
			if (GUI.Button(new Rect(10, 20, 100, 20), "Desconectar")) {
				Exit();
			}
		}
		
		if (Network.isServer) {
			if (GUI.Button(new Rect(10, 20, 100, 20), "Fechar")) {
				StopServer();
			}
		}
		GUI.DragWindow ();
	}
	
	private void OnGUI () {
		GUI.skin = skin;

		if (isPause) {
			menuWindowRect = GUI.ModalWindow(0, menuWindowRect, PauseMenu, "Menu");
		}

		GUILayout.BeginArea(new Rect(0, 0, Screen.width, (Screen.height / 3)));
		if(GetComponent<NetworkView>().isMine && target != null)
			GUILayout.Label("Alvo: " + target.gameObject.name);
		if(GetComponent<NetworkView>().isMine && activeTarget != null)
			GUILayout.Label("Alvo ativo: " + activeTarget.gameObject.name);
		GUILayout.EndArea();

		GUILayout.BeginArea(new Rect(0, (Screen.height - 60), Screen.width, 50));
		if(GetComponent<NetworkView>().isMine && (target == null || target == transform)) {
			GUILayout.Label("Nome: " + gameObject.transform.FindChild("HealthBar").FindChild("Name").GetComponent<Text>().text);
			GUILayout.Label("Vida: " + curHealth + "/" + maxHealth);
		}
		else if (GetComponent<NetworkView>().isMine) {
			GUILayout.Label("Nome: " + target.FindChild("HealthBar").FindChild("Name").GetComponent<Text>().text);
			if(target.gameObject.layer == 8)
				GUILayout.Label("Vida: " + target.GetComponent<EnemyAI>().curHealth + "/" + target.GetComponent<EnemyAI>().maxHealth);
			else
				GUILayout.Label("Vida: " + target.GetComponent<Player>().curHealth + "/" + target.GetComponent<Player>().maxHealth);
		}
		GUILayout.EndArea();

		GUI.Label(new Rect(screenPos.x, (screenPos.y + 20), 100, 25), "hue");
	}

	private void Awake() // ativa a camera e audio do objeto controlado apos ser instanciado
	{
		if (GetComponent<NetworkView>().isMine)
		{
			GetComponent<AgentWalk>().enabled = true;
			GetComponent<AudioListener>().enabled = true;
		}
	}

	// Use this for initialization
	private void Start () {
		agent = GetComponent<NavMeshAgent>();
		canvasRotation = this.gameObject.GetComponentInChildren<Canvas>().transform.rotation;
		if (GetComponent<NetworkView>().isMine) {
			healthBar = this.gameObject.GetComponentInChildren<Slider>();
			GetComponent<NetworkView>().RPC("SetHealth", RPCMode.AllBuffered, Network.player.ToString());
		}
	}

	// Update is called once per frame
	private void Update () {
		if (GetComponent<NetworkView>().isMine) {
			PausedCamera();
			if(Input.GetMouseButtonDown(1) && !mouseOverSomething) {
				isFollowing = false;
				activeTarget = null;
			}

			if(Input.GetMouseButtonDown(0) && !mouseOverSomething) {
				//desativa o indicador de seleçao
				if (target != null && target.gameObject.GetComponentInChildren<Projector>().enabled == true)
					target.gameObject.GetComponentInChildren<Projector>().enabled = false;

				target = null;
			}

			if(isFollowing) {
				transform.GetComponent<AgentWalk>().agent.stoppingDistance = 1.5f;
				transform.GetComponent<AgentWalk>().agent.SetDestination(activeTarget.transform.position);
			}
			else {
				transform.GetComponent<AgentWalk>().agent.stoppingDistance = 0f;
			}
		}
		else {
			//SyncedMovement();
		}

		if(activeTarget != null) {
			if((agent.transform.position - activeTarget.transform.position).magnitude <= 1.5f && activeTarget.GetComponent<EnemyAI>().curHealth > 0) {
				Attack();
			}
			if(activeTarget.GetComponent<EnemyAI>().curHealth <= 0) {
				if(target == activeTarget)
					target = null;

				activeTarget = null;
				isFollowing = false;
			}
		}

		if(curHealth < maxHealth && !onAction) {
			Regen();
		}

		screenPos = Camera.main.ViewportToScreenPoint(transform.position);
	}

	void Regen() {
		if(Time.time > nextRegen) {
			if(GetComponent<NetworkView>().isMine) {
				nextRegen = Time.time + regenInterval;
				GetComponent<NetworkView>().RPC("SendRegen", RPCMode.AllBuffered);
			}
		}
	}

	void Attack() {
		onAction = true;
		if(Time.time > nextAttack) {
			nextAttack = Time.time + attackInterval;
			Debug.Log("Atacou a creep");
			activeTarget.SendMessage("ApplyDamage", meleeDamage);
		}
	}

	private void LateUpdate () {
		GetComponent<NetworkView>().RPC("FixHealthBar", RPCMode.All);
	}

	void OnMouseEnter() {
		GameObject.FindGameObjectWithTag("Player").SendMessage("mouseOver", true);

		if(!GetComponent<NetworkView>().isMine) {
			GetComponent<Renderer>().material.shader = Shader.Find("Standard");
		}
	}

	void OnMouseExit() {
		GameObject.FindGameObjectWithTag("Player").SendMessage("mouseOver", false);

		if(!GetComponent<NetworkView>().isMine) {
			GetComponent<Renderer>().material.shader = Shader.Find("Diffuse");
		}
	}

	void OnMouseDown() {
		Transform thisTarget = transform;
		GameObject.FindGameObjectWithTag("Player").SendMessage("getTarget", thisTarget);
	}

	void OnMouseOver() {
		if(Input.GetMouseButtonDown(1)) {
			Transform thisTarget = transform;

			if(!GetComponent<NetworkView>().isMine)
				GameObject.FindGameObjectWithTag("Player").SendMessage("chaseTarget", thisTarget);
		}
	}

	void PausedCamera() {
		if (Input.GetKeyDown(KeyCode.Escape) && (Network.isClient || Network.isServer)) {
			isPause = !isPause;
			if (isPause) {
				//GetComponentInChildren<GrayscaleEffect>().enabled = true;
				//GetComponentInChildren<Blur>().enabled = true;
				GetComponent<AgentWalk>().enabled = false;
			}
			else {
				//GetComponentInChildren<GrayscaleEffect>().enabled = false;
				//GetComponentInChildren<Blur>().enabled = false;
				GetComponent<AgentWalk>().enabled = true;
			}
		}

		if (isPause) {
			Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 45, 5*Time.deltaTime);
		}
		else {
			Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 60, 5*Time.deltaTime);
		}
	}

	void Followed(bool following) {
		if(following)
			onAction = true;
		if(!following)
			onAction = false;
	}

	void ApplyDamage(float damage) {
		if(GetComponent<NetworkView>().isMine) {
			GetComponent<NetworkView>().RPC("SendDamage", RPCMode.AllBuffered, damage);
		}
	}

	void getTarget(Transform thisTarget) {
		//deseleciona o alvo atual caso selecione novo alvo diretamente
		if(target != null && target != thisTarget)
			target.gameObject.GetComponentInChildren<Projector>().enabled = false;

		target = thisTarget;

		//ativa o indicador de seleçao
		if (thisTarget.gameObject.GetComponentInChildren<Projector>().enabled == false)
			thisTarget.gameObject.GetComponentInChildren<Projector>().enabled = true;
	}

	void chaseTarget(Transform thisTarget) {
		activeTarget = thisTarget;
		isFollowing = true; //o comando de seguir fica no Update
	}

	void mouseOver(bool mouseOver) {
		mouseOverSomething = mouseOver;
	}

	private void SyncedMovement() {
		syncTime += Time.deltaTime;
		GetComponent<Rigidbody>().position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		Vector3 syncPosition = Vector3.zero;
		Vector3 syncVelocity = Vector3.zero;
		if (stream.isWriting) {
			syncPosition = GetComponent<Rigidbody>().position;
			stream.Serialize(ref syncPosition);

			syncVelocity = GetComponent<Rigidbody>().velocity;
			stream.Serialize(ref syncVelocity);

			Quaternion rot = transform.rotation;
			stream.Serialize(ref rot);
		}
		else {
			stream.Serialize(ref syncPosition);
			stream.Serialize(ref syncVelocity);

			syncTime = 0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;

			syncEndPosition = syncPosition + syncVelocity * syncDelay;
			syncStartPosition = GetComponent<Rigidbody>().position;
		}
	}

	[RPC]
	void SendDamage(float damage) {
		if(this.curHealth > 0)
			this.curHealth -= damage;
		if(this.curHealth <= 0) {
			this.healthBar.fillRect.GetComponent<Image>().enabled = false;
			Debug.Log("Morreu");
			this.GetComponent<Renderer>().material.color = new Color(0, 0, 0);
		}
		this.healthBar.value = this.curHealth;
		this.gameObject.transform.FindChild("HealthBar").FindChild("Health").GetComponent<Text>().text = this.curHealth.ToString() + " / " + this.maxHealth.ToString();
	}

	[RPC]
	void SendRegen() {
		if(this.curHealth > 0) {
			this.curHealth += this.regenRate;
			this.healthBar.value = this.curHealth;
			this.gameObject.transform.FindChild("HealthBar").FindChild("Health").GetComponent<Text>().text = this.curHealth.ToString() + " / " + this.maxHealth.ToString();
		}
	}

	[RPC]
	void SetHealth(string playerID) {
		this.healthBar.maxValue = this.maxHealth;
		this.curHealth = this.maxHealth;
		this.healthBar.value = this.maxHealth;
		this.gameObject.transform.FindChild("HealthBar").FindChild("Health").GetComponent<Text>().text = this.curHealth.ToString() + " / " + this.maxHealth.ToString();
		this.gameObject.transform.FindChild("HealthBar").FindChild("Name").GetComponent<Text>().text = NetworkManager.players.getPlayerNameByID(playerID);
	}

	[RPC]
	void FixHealthBar() {
		this.gameObject.GetComponentInChildren<Canvas>().transform.rotation = canvasRotation;
	}
}