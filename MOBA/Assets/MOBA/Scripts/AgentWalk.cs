using UnityEngine;
using System.Collections;

public class AgentWalk : MonoBehaviour {

	public NavMeshAgent agent;
	public GameObject mark;

	// Use this for initialization
	void Start () {
		agent = gameObject.GetComponent<NavMeshAgent>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(1) && GetComponent<NetworkView>().isMine)
		{
			SetDestination();
		}
	}

	public void SetDestination() {
		Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		
		if (Physics.Raycast(screenRay, out hit))
		{
			if(transform.GetComponent<Player>().mouseOverSomething == false) {
				agent.SetDestination(hit.point);
				GameObject clone;
				clone = Instantiate (mark, hit.point, Quaternion.identity) as GameObject;
				Destroy (clone, 0.5f);
			}
		}
	}
}
