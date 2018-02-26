using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibPDBinding;
using LibPDBinding.Managed;
using System.IO;
using System;

public class HRIR : MonoBehaviour {
	private string pdPatchName="hrir.pd";
	public float scale=1f; //Scale of Cm
	public Patch patch = -999;
	private Pd PD=null;
	public GameObject listener=null; //Listener object

	/// <summary>
	/// Function to load route of files .WAV in patch HRIR
	/// </summary>
	/// <param name="song">Is the path song from the assets folder</param>
	/// <returns> Returns true if path is load successfully </returns>
	public bool Load_Audio(string song,params object[] args){
		string[] splt = song.Split ('.');
		if(!File.Exists(Application.dataPath+song) || splt[splt.Length-1]!="wav" )
		{
			Debug.LogWarning ("Error, Can't load file, it's not wav or file not exist");
			return false;
		}
		int answer=LibPD.SendSymbol(patch.DollarZero.ToString ()+"-Load",Application.dataPath+song);
		if (answer == 0)
			return true;
		else
			return false;
	}
	/// <summary>
	/// Function to reproduce song just once in HRIR
	/// </summary>
	/// <param name="song">Is the path song from the assets folder</param>
	public void Play(string song,params object[] args){
		if(!Load_Audio(song)){
			return;
		}
		LibPD.SendBang(patch.DollarZero.ToString ()+"-Play");
	}
	/// <summary>
	/// Function to reproduce song in bucle 
	/// </summary>
	/// <param name="song">Is the path song from the assets folder</param>
	public void Play_Loop(string song,params object[] args){
		if(!Load_Audio(song)){
			return;
		}
		LibPD.SendBang(patch.DollarZero.ToString ()+"-Play_Loop");
	}
	/// <summary>
	/// Function to stop song plays
	/// </summary>
	public void Stop(){
		LibPD.SendBang(patch.DollarZero.ToString ()+"-Stop");
	}
	/// <summary>
	/// Function to set available or unavailable the default microphone 
	/// </summary>
	/// <param name="available">Variable bool available</param>
	public void Mic (bool available){
		if (available) LibPD.SendFloat (patch.DollarZero.ToString () + "-Mic", 1f); 
		else LibPD.SendFloat (patch.DollarZero.ToString () + "-Mic", 0f);	
	}
	/// <summary>
	/// Function to update azimuth in HRIR
	/// </summary>
	/// <param name="f">Is a angle of azimuth</param>
	private void Update_Azimuth (float f){
		LibPD.SendFloat (patch.DollarZero.ToString () + "-A", f);	
	}
	/// <summary>
	/// Function to update elevation in HRIR
	/// </summary>
	/// <param name="f">Is a angle of elevation</param>
	private void Update_Elevation (float f){
		LibPD.SendFloat (patch.DollarZero.ToString ()+"-E", f);
	}
	/// <summary>
	/// Function to update distance in HRIR
	/// </summary>
	/// <param name="f">Is a angle of distance</param>
	private void Update_Distance (float f){
		LibPD.SendFloat (patch.DollarZero.ToString ()+"-D", f*scale);
	}

	public float[] Process_Audio(float[] data,int channels,float[] input){
		float[] output;
		PD.Start ();
		PD.Process ((int)(data.Length / LibPD.BlockSize / channels), input, output);
		PD.Stop();
		return output;
	}
		
	void Start(){
		PD = new Pd (PdManager.Instance.numberOfInputChannel, PdManager.Instance.numberOfOutputChannel, AudioSettings.outputSampleRate, Application.dataPath + Path.DirectorySeparatorChar.ToString () + "StreamingAssets");
		patch = PD.LoadPatch (pdPatchName);
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
	}


	void OnDestroy() {
		patch.Dispose();
		PD.Dispose();
	}

	// Update is called once per frame
	void Update () {
		//Calculate distance between listener and sound source	
		Update_Distance (Mathf.Abs (Vector3.Distance (listener.transform.position, transform.position)));				
		//Calculate diretion vector between listener and sound source	
		Vector3 dir=(transform.position-listener.transform.position).normalized;
		//Calculate angle of elevation between listener and sound source	
		float elevation=Vector3.Angle(listener.transform.forward,new Vector3(listener.transform.forward.x,dir.y,listener.transform.forward.z));
		if(dir.y<listener.transform.forward.y){
			elevation = -elevation;
		}
		Update_Elevation (elevation);
		//Calculate angle of azimuth between listener and sound source
		float azimuth=Vector3.Angle(listener.transform.forward,new Vector3(dir.x,listener.transform.forward.y,dir.z));
		if(listener.transform.forward.x*dir.z-(listener.transform.forward.z*dir.x)<0){ //use determinant to know the direction of azimuth
			azimuth = 360 - azimuth;
		}
		Update_Azimuth (azimuth);
		//Debug.Log ("azimuth "+azimuth+" elevation "+elevation+" Direction "+dir+" Listener "+listener.transform.forward);
	}
}
