using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LibPDBinding;

public class PdManager : MonoBehaviour {

	/// <summary>
	/// The number of input channels to spatializer, if it is greater than 0, it will start to listen the Mic_Device, the recommended value is 1 to listen mono signal of the microphone.
	/// </summary>
	public int numberOfInputChannel = 0;
	/// <summary>
	/// The number of output channels from spatializer, the recommended value is 2, HRIR spatializer is focus to stereo systems.
	/// </summary>
	public int numberOfOutputChannel = 2;
	/// <summary>
	/// Mixers to put the processed audio.
	/// </summary>
	public AudioMixerGroup[] targetMixerGroups;
	private GameObject pdMixer;//Mixer object
	/// <summary>
	/// Microphone device to take a signal to spatializer, if it is null, it will take the default microphone for unity.
	/// </summary>
	public string Mic_Device=""; //Microsoft® LifeCam HD-5000 //Logitech USB Headset
	private HRIR[] sound_sources; //All sound source in scene

	//
	private AudioClip Mic; //Class to recording from microphone.
	private bool Is_Device=false; //Exists this device.
	private float[] PDMic_Input; //Buffer given from microphone.
	private int Temp_InputChannels = 0;  //Temporal variable to save number of inputs.
	//

	private static PdManager _instance;

	public static PdManager Instance
	{
		get 
		{
			return _instance;
		}
	}
	//Create a mixer.
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
	/// <summary>
	/// Function to start record from Mic_Device.
	/// </summary>
	void Avaible_Mic(){
		Is_Device = false;
		if(numberOfInputChannel<=0){
			numberOfInputChannel = Temp_InputChannels;
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
			PDMic_Input=new float[AudioSettings.outputSampleRate*numberOfInputChannel];
		}
	}

	/// <summary>
	/// Function to stop record from Mic_Device.
	/// </summary>
	public void Disable_Mic(){
		Microphone.End (Mic_Device);
		Temp_InputChannels = numberOfInputChannel;
		numberOfInputChannel = 0;
	}
	/// <summary>
	/// Function to get a buffer from Mic_Device.
	/// </summary>
	/// <returns> Buffer of audio from Mic_Device. </returns>
	public float [] Get_Audio_Mic(){
		if(numberOfInputChannel > 0){
			int pos = Microphone.GetPosition (Mic_Device);
			if (pos - 1024 > 0)
				pos = pos - 1024+1;
			Mic.GetData(PDMic_Input,pos);	
		}
		return PDMic_Input;
	}
	/// <summary>
	/// Function to return the sound sources objects in scene.
	/// </summary>
	/// <returns> Array with all spatializers in scene. </returns>
	public HRIR[] Get_SoundSources(){
		return sound_sources;
	}

	void Start () {
		//sample optimum 48000
		Avaible_Mic();
	}

	void Update(){	
		sound_sources = UnityEngine.Object.FindObjectsOfType<HRIR> ();//Refresh sound sources objects, that posible to the new object has create	
	}		
		
}
