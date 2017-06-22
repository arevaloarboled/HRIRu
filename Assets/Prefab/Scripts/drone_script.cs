using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drone_script : MonoBehaviour {
	private HRIR hrir_control;
	// Use this for initialization
	void Start () {
		hrir_control=this.GetComponent<HRIR>();
		hrir_control.Play_Loop("Prefab/Sounds/DRONE_sound.wav");
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
	}
}
