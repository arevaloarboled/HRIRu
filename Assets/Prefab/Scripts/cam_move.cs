using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cam_move : MonoBehaviour {

	public float turnSpeed = 4.0f;		// Speed of camera turning when mouse moves in along an axis
	public float moveSpeed= 1f;
	public GameObject prefab;
	private Vector3 mouseOrigin;	// Position of cursor when mouse dragging starts
	private bool isRotating;	// Is the camera being rotated?
	public float delta_time=1f;
	private float count_time=0f;
	public bool is_pulling=false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Time.time-count_time>delta_time && is_pulling){
			GameObject clone = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
			drone_script properties = clone.GetComponent<drone_script> ();
			properties.tipe_move = (int)Random.Range (0f,3f);
			count_time = Time.time;
		}
		if(Input.GetKeyDown(KeyCode.P)){
			is_pulling = true;
		}
		if(Input.GetKeyDown(KeyCode.O)){
			is_pulling = false;
		}
		if(Input.GetKeyDown(KeyCode.I)){
			GameObject clone = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
			drone_script properties = clone.GetComponent<drone_script> ();
			properties.tipe_move = (int)Random.Range (0f,3f);
		}
		if(Input.GetKey("w")){
			transform.position = transform.position + transform.forward*moveSpeed;
		}
		if(Input.GetKey("s")){
			transform.position = transform.position - transform.forward*moveSpeed;
		}
		if(Input.GetKey("d")){
			transform.position = transform.position + transform.right*moveSpeed;
		}
		if(Input.GetKey("a")){
			transform.position = transform.position - transform.right*moveSpeed;
		}
		if(Input.GetKey("e")){
			transform.position = transform.position + Vector3.up*moveSpeed;
		}
		if(Input.GetKey("q")){
			transform.position = transform.position + Vector3.down*moveSpeed;
		}

		// Get the left mouse button
		if(Input.GetMouseButtonDown(0))
		{
			// Get mouse origin
			mouseOrigin = Input.mousePosition;
			isRotating = true;
		}

		// Disable movements on button release
		if (!Input.GetMouseButton(0)) isRotating=false;

		// Rotate camera along X and Y axis
		if (isRotating)
		{
			Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);
			transform.RotateAround(transform.position, transform.right, -pos.y * turnSpeed);
			transform.RotateAround(transform.position, Vector3.up, pos.x * turnSpeed);
		}
	}
}
