using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {

	float camSpeed;
	float GUISize = 60f;
	bool keyMoving = false;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		Rect recDown = new Rect (0, 0, Screen.width, GUISize);
		Rect recUp = new Rect (0, Screen.height-GUISize, Screen.width, GUISize);
		Rect recLeft = new Rect (0, 0, GUISize, Screen.height);
		Rect recRight = new Rect (Screen.width-GUISize, 0, GUISize, Screen.height);

		//Keyboard Movement
		if(Input.GetKey(KeyCode.S)) {
			keyMoving = true;
			float camSpeed = 1f;
			if(transform.position.z > 0)
				transform.Translate(0, 0, -camSpeed, Space.World);
		}
		
		if (Input.GetKey(KeyCode.W)) {
			keyMoving = true;
			float camSpeed = 1f;
			if(transform.position.z < 50)
				transform.Translate(0, 0, camSpeed, Space.World);
		}
		
		if (Input.GetKey(KeyCode.A)) {
			keyMoving = true;
			float camSpeed = 1f;
			if(transform.position.x > 10)
				transform.Translate(-camSpeed, 0, 0, Space.World);
		}
		
		if (Input.GetKey(KeyCode.D)) {
			keyMoving = true;
			float camSpeed = 1f;
			if(transform.position.x < 20)
				transform.Translate(camSpeed, 0, 0, Space.World);
		}

		if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)) {
			keyMoving = false;
		}

		//Mouse Movement
		if(!keyMoving) {
			if(recDown.Contains(Input.mousePosition)) {
				float camSpeed = 0.3f;
				if(transform.position.z > 0)
					transform.Translate(0, 0, -camSpeed, Space.World);
			}
			
			if (recUp.Contains(Input.mousePosition)) {
				float camSpeed = 0.3f;
				if(transform.position.z < 50)
					transform.Translate(0, 0, camSpeed, Space.World);
			}
			
			if (recLeft.Contains(Input.mousePosition)) {
				float camSpeed = 0.3f;
				if(transform.position.x > 10)
					transform.Translate(-camSpeed, 0, 0, Space.World);
			}
			
			if (recRight.Contains(Input.mousePosition)) {
				float camSpeed = 0.3f;
				if(transform.position.x < 20)
					transform.Translate(camSpeed, 0, 0, Space.World);
			}
		}
		/*if (Input.GetAxis("Mouse ScrollWheel") <0)
		{
			if (Camera.main.fieldOfView<=100)
				Camera.main.fieldOfView +=2;
		}
		
		if (Input.GetAxis("Mouse ScrollWheel") > 0)
		{
			if (Camera.main.fieldOfView>2)
				Camera.main.fieldOfView -=2;
		}*/
	}
}