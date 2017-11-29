using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cam_move : MonoBehaviour {

	public float turnSpeed = 4.0f;		// Speed of camera turning when mouse moves in along an axis

	private Vector3 mouseOrigin;	// Position of cursor when mouse dragging starts
	private bool isRotating;	// Is the camera being rotated?
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		if(Input.GetKey("w")){
			transform.position = transform.position + Vector3.forward*0.5f;
		}
		if(Input.GetKey("s")){
			transform.position = transform.position + Vector3.back*0.5f;
		}
		if(Input.GetKey("d")){
			transform.position = transform.position + Vector3.right*0.5f;
		}
		if(Input.GetKey("a")){
			transform.position = transform.position + Vector3.left*0.5f;
		}
		if(Input.GetKey("e")){
			transform.position = transform.position + Vector3.up*0.5f;
		}
		if(Input.GetKey("q")){
			transform.position = transform.position + Vector3.down*0.5f;
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