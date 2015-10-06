using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
	public Transform spawn;
	public Transform chasing;
	public GameObject[] players;
	private NavMeshAgent agent;
	public Color playerColor = new Color(84, 234, 92, 255);

	private float attackInterval = 1.4f;
	private float nextAttack = 0;
	public float meleeDamage = 2;

	public float maxHealth = 10;
	public float curHealth;
	private Slider healthBar;
	public Quaternion canvasRotation;

	void Start ()
	{
		canvasRotation = new Quaternion(0.5f, 0, 0, 0.86f);
		if (GetComponent<NetworkView>().isMine) {
			GetComponent<NetworkView>().RPC("SetHealth", RPCMode.AllBuffered);
		}

		agent = GetComponent<NavMeshAgent>();
	}

	void Update ()
	{
		players = GameObject.FindGameObjectsWithTag("Player");
		foreach(GameObject target in players)
		{
			if(Vector3.Distance(target.transform.position, agent.transform.position) < 7)
			{
				if(chasing == null && target.transform.GetComponent<Player>().curHealth > 0)
				{
					chasing = target.GetComponent<Transform>();
					chasing.SendMessage("Followed", true);
					//chasing.renderer.material.color = new Color(0.84f, 0.234f, 0.92f);
				}

				if(chasing != null && chasing.GetComponent<Player>().curHealth > 0) {
					if(target.gameObject == chasing.gameObject) {
						agent.stoppingDistance = 1.5f;
						agent.SetDestination(chasing.transform.position);
					}
				}
			}
			else if (chasing != null && Vector3.Distance(chasing.transform.position, agent.transform.position) > 7)
			{
				//chasing.renderer.material.color = new Color(0.3294f, 0.9176f, 0.3607f);
				chasing.SendMessage("Followed", false);
				ReturnToSpawn();
			}
		}

		if(chasing != null) {
			if((agent.transform.position - chasing.transform.position).magnitude <= 1.5f) {
				Attack();
			}

			if(chasing.GetComponent<Player>().curHealth <= 0)
				ReturnToSpawn();
		}
	}

	private void LateUpdate () {
		GetComponent<NetworkView>().RPC("FixHealthBar", RPCMode.All);
	}

	void ReturnToSpawn() {
		chasing = null;
		agent.stoppingDistance = 0f;
		agent.SetDestination(spawn.position);
		if(agent.velocity.magnitude == 0 && (agent.transform.position - spawn.transform.position).magnitude < 0.2f)
		{
			StartCoroutine(ResetAI());
		}
	}

	void Attack() {
		if(Time.time > nextAttack) {
			nextAttack = Time.time + attackInterval;
			Debug.Log("Atacando");
			chasing.SendMessage("ApplyDamage", meleeDamage);
		}
		if(chasing.GetComponent<Player>().curHealth <= 0) {
			agent.stoppingDistance = 0;
			agent.SetDestination(spawn.position);
		}
	}

	void ApplyDamage(float damage) {
		if(GetComponent<NetworkView>().isMine) {
			GetComponent<NetworkView>().RPC("SendDamage", RPCMode.AllBuffered, damage);
		}
	}

	void OnMouseEnter() {
		GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().mouseOverSomething = true;
		GetComponent<Renderer>().material.shader = Shader.Find("Standard");
	}
	
	void OnMouseExit() {
		GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().mouseOverSomething = false;
		GetComponent<Renderer>().material.shader = Shader.Find("Diffuse");
	}

	void OnMouseDown() {
		Transform thisTarget = transform;
		GameObject.FindGameObjectWithTag("Player").SendMessage("getTarget", thisTarget);
	}

	void OnMouseOver() {
		if(Input.GetMouseButtonDown(1)) {
			Transform thisTarget = transform;
			GameObject.FindGameObjectWithTag("Player").SendMessage("chaseTarget", thisTarget);
		}
	}

	void Deselect() {
		transform.gameObject.GetComponentInChildren<Projector>().enabled = false;
	}

	IEnumerator ResetAI() {
		transform.rotation = Quaternion.Slerp(transform.rotation, spawn.transform.rotation, Time.time * 0.005f);
		yield return new WaitForSeconds(1.5f);
	}

	[RPC]
	void SetHealth() {
		healthBar = this.gameObject.GetComponentInChildren<Slider>();
		this.healthBar.maxValue = this.maxHealth;
		this.curHealth = this.maxHealth;
		this.healthBar.value = this.maxHealth;
		this.gameObject.transform.FindChild("HealthBar").FindChild("Health").GetComponent<Text>().text = this.curHealth.ToString() + " / " + this.maxHealth.ToString();
	}
	
	[RPC]
	void FixHealthBar() {
		this.gameObject.GetComponentInChildren<Canvas>().transform.rotation = canvasRotation;
	}

	[RPC]
	void SendDamage(float damage) {
		if(this.curHealth > 0)
			this.curHealth -= damage;

		this.healthBar.value = this.curHealth;
		this.gameObject.transform.FindChild("HealthBar").FindChild("Health").GetComponent<Text>().text = this.curHealth.ToString() + " / " + this.maxHealth.ToString();

		if(this.curHealth <= 0) {
			Debug.Log("A Creep morreu");
			chasing.SendMessage("Followed", false);
			Destroy(this.gameObject);
		}
	}
}