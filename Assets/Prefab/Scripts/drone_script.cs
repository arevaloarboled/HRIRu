using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drone_script : MonoBehaviour {
	private HRIRu hrir_control;
	private AudioSource audioSource;

	public float scale=0f;
	public float tp=0.5f;
	private float t=0f;
	public int tipe_move=0;
	public bool use_mic=false;
	public string sound = "";
	private bool spatializer = false;
	// Use this for initialization
	void Start () {
		hrir_control=this.GetComponent<HRIRu>();
		audioSource = this.GetComponent<AudioSource> ();
		/*hrir_control.Available();
		if (use_mic)
			hrir_control.Mic (true);
		else
			hrir_control.Play_Loop("Prefab/Sounds/"+sound);*/
		if(scale<=0f)
			scale = 5f;
		if(tp<=0f)
			tp = 0.5f;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Space)){
			if (spatializer) {
				Debug.Log ("Changing to unity spatializer...");
				hrir_control.Disable ();
				audioSource.mute = !audioSource.mute;
			} else {
				Debug.Log ("Changing to HRIRu spatializer...");
				audioSource.mute = !audioSource.mute;
				hrir_control.Available ();
				hrir_control.Play_Loop("Prefab/Sounds/"+sound);
				hrir_control.Volume (5f);
			}
		}
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
