using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LibPDBinding;
using LibPDBinding.Managed;
using LibPDBinding.Managed.Data;

public class PdManager : MonoBehaviour {


	public int numberOfInputChannel = 0;
	public int numberOfOutputChannel = 2;
	public AudioMixerGroup[] targetMixerGroups;
	public bool startDspOnStart = false;
	private Pd PD;
	private bool _pdDsp = true;
	private List<Patch> _loadedPatches = new List<Patch>();
	private GameObject pdMixer;
	public string Mic_Device=""; //Microsoft® LifeCam HD-5000 //Logitech USB Headset

	//
	private AudioClip Mic;
	private bool Is_Device=false;
	private float[] PDMic_Input;
	private int Ni = 0;
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
		Patch patch = PD.LoadPatch(Application.dataPath +
			Path.DirectorySeparatorChar.ToString () + "StreamingAssets" +
		                 Path.DirectorySeparatorChar.ToString () + name);
		_loadedPatches.Add(patch);
		return patch.DollarZero;
	}

	public void ClosePdPatch(int dollarzero){
		foreach(Patch patch in _loadedPatches){
			if(patch.DollarZero==dollarzero){
				patch.Dispose ();
				_loadedPatches.Remove(patch);
				return;
			}
		}
	}

	public void Send(string reciver,float data){
		PD.Messaging.Send(reciver, new Float(data));	
	}

	public void Send(string reciver,string data){
		PD.Messaging.Send(reciver, new Symbol(data));	
	}

	public void Send(string reciver){
		PD.Messaging.Send(reciver, new Bang());	
	}

	public void Compute(bool state){
		if (state)
			PD.Start ();
		else
			PD.Stop ();
	}

	public float[] Process_Audio(int Length,int channels,float[] input){
		float[] output=new float[Length];
		PD.Process ((int)(Length / PD.BlockSize / channels), input, output);
		return output;
	}

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
			PD=new Pd(numberOfInputChannel, numberOfOutputChannel, AudioSettings.outputSampleRate,new List<string>() {Application.dataPath + Path.DirectorySeparatorChar.ToString () + "StreamingAssets"});
			openNewPdPatch("pdManager.pd");
			if (numberOfOutputChannel != targetMixerGroups.Length * 2) {
				Debug.LogWarning ("The number of output channel is not equal to the number of mixer group!");
				Debug.LogWarning ("Set number of output channel to " + (targetMixerGroups.Length * 2).ToString ());
				numberOfOutputChannel = targetMixerGroups.Length * 2;
			}
			createPdMixer ();
			if(startDspOnStart) PD.Start();
		} else if (!Instance.Equals((object)this)){
			Destroy (gameObject);
		}
	}
		
	void Avaible_Mic(){
		if(numberOfInputChannel<=0){
			numberOfInputChannel = Ni;
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
			Mic = Microphone.Start(Mic_Device, true, 3, AudioSettings.outputSampleRate);
			//PDMic_Input=new float[Mic.samples * Mic.channels];
			PDMic_Input=new float[1024*numberOfInputChannel];
		}
	}

	void Disable_Mic(){
		Microphone.End (Mic_Device);
		Ni = numberOfInputChannel;
		numberOfInputChannel = 0;
	}

	public float [] Get_Audio_Mic(){
		if(numberOfInputChannel > 0){
			int pos = Microphone.GetPosition (Mic_Device);
			if (pos - 1024 > 0)
				pos = pos - 1024+1;
			Mic.GetData(PDMic_Input,pos);	
		}
		return PDMic_Input;
	}

	void Start () {
		Avaible_Mic();
	}


	void OnApplicationQuit(){
		PD.Dispose ();
	}
}
