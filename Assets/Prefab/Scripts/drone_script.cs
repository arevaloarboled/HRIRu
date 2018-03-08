using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drone_script : MonoBehaviour {
	private HRIR hrir_control;

	public float scale=0f;
	public float tp=0.5f;
	private float t=0f;
	public int tipe_move=0;
	public bool use_mic=false;
	public string sound = "";
	// Use this for initialization
	void Start () {
		hrir_control=this.GetComponent<HRIR>();
		hrir_control.Available ();
		if (use_mic)
			hrir_control.Mic (true);
		else
			hrir_control.Play_Loop("/Prefab/Sounds/"+sound);
		if(scale<=0f)
			scale = 5f;
		if(tp<=0f)
			tp = 0.5f;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 tmp=new Vector3(0,0,0);
		switch(tipe_move){
			case 0:
				tmp = new Vector3 (Mathf.Cos (t), Mathf.Sin (t), Mathf.Cos (2 * t));
				break;
			case 1:
				tmp = new Vector3 (Mathf.Cos (5 * t), Mathf.Cos (t) + Mathf.Cos (5 * t), Mathf.Sin (3 * t));	
				break;
			default:
				tmp = new Vector3 (Mathf.Sin (2 * t), Mathf.Cos (2 * t), Mathf.Cos (3 * t));	
				break;
			
		}
		transform.position = tmp * scale;
		t = t + tp;
	}
}
