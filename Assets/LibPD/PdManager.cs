using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LibPDBinding;

public class PdManager : MonoBehaviour {


	public int numberOfInputChannel = 0;
	public int numberOfOutputChannel = 2;
	public AudioMixerGroup[] targetMixerGroups;
	public bool startDspOnStart = false;
	private bool _pdDsp = true;
	private List<int> _loadedPatches = new List<int>();
	private GameObject pdMixer;
	public string Mic_Device=""; //Microsoft® LifeCam HD-5000 //Logitech USB Headset
	public int sampleRT_Mic; //48000

	//
	private AudioClip Mic;
	private bool Is_Device=false;
	private float[] PDMic_Input;
	//

	private static PdManager _instance;

	public static PdManager Instance
	{
		get 
		{
			return _instance;
		}
	}


	public bool PdDSP{
		get{
			return _pdDsp;
		}
		set{
			_pdDsp = value;
			LibPD.ComputeAudio (_pdDsp);
		}
	}

	public float[] get_PDMic_Input(){
		return PDMic_Input;
	}

	public int openNewPdPatch(string name){
		int dollarzero = LibPD.OpenPatch (Application.dataPath +
			Path.DirectorySeparatorChar.ToString () + "StreamingAssets" +
		                 Path.DirectorySeparatorChar.ToString () + name);
		_loadedPatches.Add(dollarzero);
		return dollarzero;
	}

	public void ClosePdPatch(int dollarzero){
		_loadedPatches.Remove(dollarzero);
		LibPD.ClosePatch (dollarzero);
	}

	public static float FindAzimuth(GameObject player, GameObject other){
		float azimuth = Mathf.Atan2 ((other.transform.position.z - player.transform.position.z)
			, (other.transform.position.x - player.transform.position.x)) * Mathf.Rad2Deg;
		azimuth += (player.transform.rotation.eulerAngles.y+990);
		azimuth = 360 - (azimuth%360);
		return azimuth;
	}

	public static float FindAzimuth(GameObject player, Transform other){
		float azimuth = Mathf.Atan2 ((other.position.z - player.transform.position.z)
			, (other.position.x - player.transform.position.x)) * Mathf.Rad2Deg;
		azimuth += (player.transform.rotation.eulerAngles.y+990);
		azimuth = 360 - (azimuth%360);
		return azimuth;
	}

	public static float FindDistance(GameObject player, GameObject other){
		return Mathf.Abs(Vector3.Distance (player.transform.position, other.transform.position));
	}

	public static float FindDistance(GameObject player, Transform other){
		return Mathf.Abs(Vector3.Distance (player.transform.position, other.position));
	}
		
	/*
	private void createDac(){
		object[] dacArgs = new object[3 + numberOfOutputChannel];
		dacArgs [0] = "0";
		dacArgs [1] = "0";
		dacArgs [2] = "dac~";
		for (int i = 0; i < numberOfOutputChannel; i++) {
			//create catch~ for each inlet
			LibPD.SendMessage ("pdManager.setManager", "obj", new object[]{"catch~","out"+(i+1).ToString()});
			//dac Arguments
			dacArgs [i + 3] = (i + 1).ToString ();
		}
		LibPD.SendMessage ("pdManager.setManager", "obj", dacArgs);

		for (int i = 0; i < numberOfOutputChannel; i++) {
			LibPD.SendMessage ("pdManager.setManager", "connect", new object[]{i.ToString(),"0",numberOfOutputChannel.ToString(),i.ToString()});
		}
	}

	private void createAdc(){
		object[] adcArgs = new object[3 + numberOfInputChannel];
		adcArgs [0] = "0";
		adcArgs [1] = "0";
		adcArgs [2] = "adc~";
		for (int i = 0; i < numberOfOutputChannel; i++) {
			//create throw for each outlet
			LibPD.SendMessage ("pdManager.setManager", "obj", new object[]{"throw~","in"+(i+1).ToString()});
			adcArgs [i + 3] = (i + 1).ToString ();
		}
		LibPD.SendMessage ("pdManager.setManager", "obj", adcArgs);
		for (int i = 0; i < numberOfOutputChannel; i++) {
			LibPD.SendMessage ("pdManager.setManager", "connect", 
				new object[]{(numberOfInputChannel+numberOfOutputChannel+1).ToString(),i.ToString(),
					(i+numberOfOutputChannel+1).ToString(),"0"});
		}
	}
*/
	private void createPdMixer(){
		pdMixer = new GameObject ("PdMixer");
		for(int i=0;i<numberOfOutputChannel/2;++i){
			GameObject newGroup = new GameObject (targetMixerGroups [i].name);
			PdStereo pdStereo = newGroup.AddComponent<PdStereo> ();
			newGroup.AddComponent<AudioSource> ();
			pdStereo.selectedChannels [0] = i * 2;
			pdStereo.selectedChannels [1] = (i * 2) + 1;
			pdStereo.setMixerGroup (targetMixerGroups [i]);
			if (i == 0)
				pdStereo.pullDataFromPd = true;
			newGroup.transform.parent = pdMixer.transform;
		}
		DontDestroyOnLoad (pdMixer);
	}

	void Awake()
	{
		if (_instance == null) {
			_instance = this;
			DontDestroyOnLoad (gameObject);
			LibPD.ReInit();
			LibPD.OpenAudio(numberOfInputChannel, numberOfOutputChannel, AudioSettings.outputSampleRate);
			//
			LibPD.ClearSearchPath();
			//
			LibPD.AddToSearchPath (Application.dataPath+ Path.DirectorySeparatorChar.ToString ()+"StreamingAssets");
			openNewPdPatch ("pdManager.pd");
			if (numberOfOutputChannel != targetMixerGroups.Length * 2) {
				Debug.LogWarning ("The number of output channel is not equal to the number of mixer group!");
				Debug.LogWarning ("Set number of output channel to " + (targetMixerGroups.Length * 2).ToString ());
				numberOfOutputChannel = targetMixerGroups.Length * 2;
			}
			createPdMixer ();
			if(startDspOnStart) LibPD.ComputeAudio (true);
		} else if (!Instance.Equals((object)this)){
			Destroy (gameObject);
		}
	}
		


	void Start () {
		


		//---------------these lines will crash, don't use!!!-------------------
		//if (numberOfOutputChannel != 0) createDac ();
		//if (numberOfInputChannel != 0) 	createAdc ();

		//sample optime 48000
		if(sampleRT_Mic==null || sampleRT_Mic==0){
			sampleRT_Mic = 48000; 
		}
		if (numberOfInputChannel > 0) {
			foreach (string device in Microphone.devices) {
				if(Mic_Device==device){
					Is_Device = true;
					break;
				}
			}
			if (!Is_Device)
				Mic = null;
			Mic = Microphone.Start(Mic_Device, true, 3, sampleRT_Mic);
			//PDMic_Input=new float[Mic.samples * Mic.channels];
			PDMic_Input=new float[1024*numberOfInputChannel];
		}

	}

	void Update(){
		if(numberOfInputChannel > 0){
			int pos = Microphone.GetPosition (Mic_Device);
			if (pos - 1024 > 0)
				pos = pos - 1024+1;
			Mic.GetData(PDMic_Input,pos);	
		}
	}		


	void OnApplicationQuit(){
		//LibPD.SendMessage ("pdManager.makeAbstraction", "clear");
		//Debug.Log("holi, voy para afuera: "+ _loadedPatches.Count.ToString());
		LibPD.ComputeAudio (false);
		foreach (int patch in _loadedPatches){
			LibPD.ClosePatch (patch);
			//LibPD.Unsubscribe (patch.ToString () + "-isPlaying");
		}
		//LibPD.ClearSearchPath();
		LibPD.Release();
	}
}
