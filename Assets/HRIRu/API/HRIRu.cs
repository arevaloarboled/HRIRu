using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibPDBinding;
using System.IO;
using System;
using LibPDBinding.Managed;

public class HRIRu : MonoBehaviour {
	private string pdPatchName="hrir.pd"; //Patch that have HRIR spatializer
	/// <summary>
	/// The scale of distance respectively between sound source and listener in Cm.
	/// </summary>
	public float scale=1f; //Scale of Cm
	private int dollarzero=-999; //Reference of patch
	public GameObject listener=null; //Listener object
	private bool _isPlaying=false; //Spatializer is working or not

	public bool isPlaying{
		get{ 
			return _isPlaying;
		}
	}
	/// <summary>
	/// Function to change the gain of this sound sources.  
	/// </summary>
	/// <param name="f">Default is 1, Min. value 0, Max. value 10</param>
	public void Volume(float f){
		if (f < 0f)
			f = 0f;
		if (f > 10f)
			f = 10f;
		PdManager.Instance.Send(dollarzero.ToString ()+"-Vol",f);
	}
	/// <summary>
	/// Function to load route of files .WAV in patch HRIR.
	/// </summary>
	/// <param name="song">Is the path song from the assets folder</param>
	/// <returns> Returns true if song is load successfully </returns>
	public bool Load_Audio(string song){
		if (!_isPlaying)
			return false;
		string[] splt = song.Split ('.');
		if(!File.Exists(Application.dataPath+Path.DirectorySeparatorChar+song) || splt[splt.Length-1]!="wav" )
		{
			Debug.LogWarning ("Error, Can't load file ["+song+"], it's not wav or file not exist");
			return false;
		}
		PdManager.Instance.Send(dollarzero.ToString ()+"-Load",Application.dataPath+Path.DirectorySeparatorChar+song);
		return true;
	}
	/// <summary>
	/// Function to reproduce song just once in HRIR
	/// </summary>
	/// <param name="song">Is the path song from the assets folder</param>
	public void Play(string song){
		if(!Load_Audio(song)){
			return;
		}
		PdManager.Instance.Send(dollarzero.ToString ()+"-Play");
	}
	/// <summary>
	/// Function to reproduce song in bucle 
	/// </summary>
	/// <param name="song">Is the path song from the assets folder</param>
	public void Play_Loop(string song){
		if(!Load_Audio(song)){
			return;
		}
		PdManager.Instance.Send(dollarzero.ToString ()+"-Play_Loop");
	}
	/// <summary>
	/// Function to stop song plays
	/// </summary>
	public void Stop(){
		if(_isPlaying)
			PdManager.Instance.Send(dollarzero.ToString ()+"-Stop");
	}
	/// <summary>
	/// Function to set available or unavailable the default microphone 
	/// </summary>
	/// <param name="available">Variable bool available, default is true</param>
	public void Mic (bool available=true){
		if (available) PdManager.Instance.Send (dollarzero.ToString () + "-Mic", 1f); 
		else PdManager.Instance.Send (dollarzero.ToString () + "-Mic", 0f);	
	}
	/// <summary>
	/// Function to update azimuth in HRIR
	/// </summary>
	/// <param name="f">Is a angle of azimuth</param>
	private void Update_Azimuth (float f){
		PdManager.Instance.Send (dollarzero.ToString () + "-A", f);	
	}
	/// <summary>
	/// Function to update elevation in HRIR
	/// </summary>
	/// <param name="f">Is a angle of elevation</param>
	private void Update_Elevation (float f){
		PdManager.Instance.Send (dollarzero.ToString ()+"-E", f);
	}
	/// <summary>
	/// Function to update distance in HRIR
	/// </summary>
	/// <param name="f">Is a distance between sound sources and listener.</param>
	private void Update_Distance (float f){
		PdManager.Instance.Send(dollarzero.ToString ()+"-D", f);
	}

	/// <summary>
	/// Dispose sound spatializer for sound sources object
	/// </summary>
	public void Available(){
		if (scale <= 0f)	scale=1f;        
        dollarzero = PdManager.Instance.OpenNewPdPatch (PdManager.Instance.APIPath()+pdPatchName);        
        Volume (1f);
		if(listener==null){
			//Seek audio listeners in scene
			AudioListener[] listeners = UnityEngine.Object.FindObjectsOfType<AudioListener>();
			if (listeners.Length == 0) {
				//The sound doesn't make sense without no one to hear it
				Debug.LogWarning ("No Listner founds in this scene.");
				Destroy (this);
			} else {
				//Set a listener
				listener = listeners[0].gameObject;
				//listener_id = listener.GetInstanceID ();
			}
		}
		_isPlaying = true;
	}

	/// <summary>
	/// Disable sound spatializer for sound sources object
	/// </summary>
	public void Disable(){
		PdManager.Instance.ClosePdPatch(dollarzero);
		listener = null;
		_isPlaying = false;
	}

	void OnDestroy() {
		if(PdManager.Instance.pdDsp)
			Disable ();
	}

	// Update is called once per frame
	void LateUpdate () {
		if(_isPlaying){
			//Calculate distance between listener and sound source	
			Update_Distance (Mathf.Abs (Vector3.Distance (transform.position,listener.transform.position))*scale);				
			//Calculate diretion vector between listener and sound source	
			Vector3 dir=(transform.position-listener.transform.position).normalized;
			//Calculate angle of elevation between listener and sound source
			Vector3 dirE=Vector3.ProjectOnPlane (dir, listener.transform.right);
			float elevation = - Vector3.SignedAngle (listener.transform.forward, dirE, listener.transform.right);
			if (elevation<-90f) {
				elevation = -90-(elevation % 90);
			}
			if (elevation > 90f) {
				elevation = 90 - (elevation % 90);
			}
			Update_Elevation (elevation);
			//Calculate angle of azimuth between listener and sound source
			Vector3 dirA=Vector3.ProjectOnPlane (dir, listener.transform.up);
			float azimuth = - Vector3.SignedAngle (listener.transform.forward, dirA, listener.transform.up);
			if (azimuth < 0f) {
				azimuth = 360f + azimuth;
			}
			Update_Azimuth (azimuth);
			//Debug.Log ("E: "+elevation.ToString()+"\tA: "+azimuth.ToString());
		}
	}
}
