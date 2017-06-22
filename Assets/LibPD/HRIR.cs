using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibPDBinding;
using System.IO;

public class HRIR : MonoBehaviour {
	private string pdPatchName="hrir.pd";
	//public bool playOnAwake = false;
	public LibPDFloat delCheckPlayingState; 
	public float scale=1f; //Scale of Cm
	//public LibPDBang delSelfDestroy;
	public int dollarzero = -999;
	private bool _isPlaying = false;
	private bool _isNew = true;
	private GameObject listener=null; //Listener object

	public bool isNew{
		get{
			return _isNew;
		}
		set{
			_isNew = value;
		}
	}

	public bool isPlaying{
		get{
			return _isPlaying;
		}
		set{
			_isPlaying = value;
		}
	}

	/*
	Function to load route of files .WAV in patch HRIR
	Parameters: msg is the path song from the assets folder
	Returns true if path is load successfully
	*/
	public bool Load_Audio(string msg,params object[] args){
		string[] splt = msg.Split ('.');
		if(!File.Exists(Application.dataPath+"/"+msg) || splt[splt.Length-1]!="wav" )
		{
			Debug.LogWarning ("Error, Can't load file, it's not wav or file not exist");
			return false;
		}
		LibPD.SendMessage(dollarzero.ToString ()+"-Load","set symbol "+Application.dataPath+"/"+msg,args);
		return true;
	}
	/*
	Function to reproduce song just once in HRIR
	Parameters: msg is the path song from the assets folder
	*/
	public void Play(string msg,params object[] args){
		if(!Load_Audio(msg)){
			return;
		}
		LibPD.SendBang(dollarzero.ToString ()+"-Play");
	}
	/*
	Function to reproduce song in bucle 
	Parameters: msg is the path song from the assets folder
	*/
	public void Play_Loop(string msg,params object[] args){
		if(!Load_Audio(msg)){
			return;
		}
		LibPD.SendBang(dollarzero.ToString ()+"-Play_Loop");
	}
	/*
	Function to stop song plays
	*/
	public void Stop(){
		LibPD.SendBang(dollarzero.ToString ()+"-Stop");
	}
	/*
	Function to update azimuth in HRIR
	Parameters: f is a angle of azimuth
	*/
	public void Update_Azimuth (float f){
		LibPD.SendFloat (dollarzero.ToString () + "-A", f);	
	}
	/*
	Function to update elevation in HRIR
	Parameters: f is a angle of elevation
	*/
	public void Update_Elevation (float f){
		LibPD.SendFloat (dollarzero.ToString ()+"-E", f);
	}
	/*
	Function to update distance in HRIR
	Parameters: f is a angle of azimuth
	*/
	public void Update_Distance (float f){
		LibPD.SendFloat (dollarzero.ToString ()+"-D", f*scale);
	}

	public void CheckPlayingState(string recv, float value){
		if (recv.CompareTo (dollarzero.ToString () + "-isPlaying") == 1) {
			_isPlaying = value==1?true:false;
		}
	}
		
	void Start(){
		dollarzero = PdManager.Instance.openNewPdPatch (pdPatchName);
		LibPD.Subscribe (dollarzero.ToString () + "-isPlaying");
		delCheckPlayingState = new LibPDFloat(CheckPlayingState);
		LibPD.Float += delCheckPlayingState;
		if(listener==null){
			//Seek audio listeners in scene
			AudioListener[] listeners = UnityEngine.Object.FindObjectsOfType<AudioListener>();
			if (listeners.Length == 0) {
				//The sound doesn't make sense without no one to hear it
				Debug.LogWarning ("No Listner founds in this scene!");
				Destroy (this);
			} else {
				//Set a listener
				listener = listeners[0].gameObject;
			}
		}
		/*if (playOnAwake)
			Play ();*/
	}


	void OnDestroy() {
		LibPD.Float -= delCheckPlayingState;
		LibPD.Unsubscribe (dollarzero.ToString () + "-isPlaying");
		PdManager.Instance.ClosePdPatch(dollarzero);
	}

	// Update is called once per frame
	void Update () {
		//Calculate distance between listener and sound source	
		Update_Distance (Mathf.Abs (Vector3.Distance (listener.transform.position, transform.position)));				
		//Calculate diretion vector between listener and sound source	
		Vector3 dir=(transform.position-listener.transform.position).normalized;
		//Calculate angle of elevation between listener and sound source	
		float elevation=Vector3.Angle(dir,new Vector3(dir.x,listener.transform.forward.y,dir.z));
		if(dir.y<0){
			elevation = -elevation;
		}
		Update_Elevation (elevation);
		//Calculate angle of azimuth between listener and sound source
		float azimuth=Vector3.Angle(dir,new Vector3(listener.transform.forward.x,dir.y,listener.transform.forward.z));
		if(dir.x>0){
			azimuth = 360 - azimuth;
		}
		Update_Azimuth (azimuth);
	}
}
