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
	private GameObject pdMixer;
	public string Mic_Device=""; //Microsoft® LifeCam HD-5000 //Logitech USB Headset
	public int sampleRT_Mic; //48000

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

	public float[] get_PDMic_Input(){
		return PDMic_Input;
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
			if (numberOfOutputChannel != targetMixerGroups.Length * 2) {
				Debug.LogWarning ("The number of output channel is not equal to the number of mixer group!");
				Debug.LogWarning ("Set number of output channel to " + (targetMixerGroups.Length * 2).ToString ());
				numberOfOutputChannel = targetMixerGroups.Length * 2;
			}
			createPdMixer ();
		} else if (!Instance.Equals((object)this)){
			Destroy (gameObject);
		}
	}
		
	void Avaible_Mic(){
		if(numberOfInputChannel<=0){
			numberOfInputChannel = Ni;
		}
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
			PDMic_Input=new float[AudioSettings.outputSampleRate*numberOfInputChannel];
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
		//sample optimum 48000
		Avaible_Mic();
	}

	void Update(){
		
	}		
		
}
