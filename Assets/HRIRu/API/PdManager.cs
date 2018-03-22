using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LibPDBinding;
using LibPDBinding.Managed;
using LibPDBinding.Managed.Data;
using LibPDBinding.Managed.Events;

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
	/// <summary>
	/// Start computing audio.
	/// </summary>
	public bool startDspOnStart = false;
	private Pd PD; //Instance of pure data
	/// <summary>
	/// Path of the API HRIRu.
	/// </summary>
	public string HRIRuPath="";
	/// <summary>
	/// Is computing audio?.
	/// </summary>
	public bool pdDsp = true;
	/// <summary>
	/// Is recording from Mic_Device?.
	/// </summary>
	public bool isRecording=false;
	private List<Patch> _loadedPatches = new List<Patch>(); //All patches loaded
	private GameObject pdMixer; //Mixer object
	/// <summary>
	/// Microphone device to take a signal to spatializer, if it is null, it will take the default microphone for unity.
	/// </summary>
	public string Mic_Device=""; //Microsoft® LifeCam HD-5000 //Logitech USB Headset

	//
	private AudioClip Mic; //Class to recording from microphone.
	private bool Is_Device=false; //Exists this device.
	private float[] PDMic_Input; //Buffer given from microphone.
	private int Ni = 0; //Temporal variable to save number of inputs.
	//

	private static PdManager _instance;

	public static PdManager Instance
	{
		get 
		{
			return _instance;
		}
	}

	/// <summary>
	/// Function to load patch in pure data
	/// </summary>
	/// <param name="name">Is a full path of the .pd patch to load. </param>
	/// <returns> References ID of patch in pure data. </returns>
	public int OpenNewPdPatch(string name){
		Patch patch = PD.LoadPatch(name);
		_loadedPatches.Add(patch);
		return patch.DollarZero;
	}
	/// <summary>
	/// Function to close and remove patch of pure data.
	/// </summary>
	/// <param name="dollarzero">ID references of patch given at the time of loading. </param>
	public void ClosePdPatch(int dollarzero){
		foreach(Patch patch in _loadedPatches){
			if(patch.DollarZero==dollarzero){
				patch.Dispose ();
				_loadedPatches.Remove(patch);
				return;
			}
		}
	}
	/// <summary>
	/// Function to send a float to a receiver in some patch loaded in pure data.
	/// </summary>
	/// <param name="receiver">Name of receiver in the patch (remind the dollarzero of the patch).</param>
	/// <param name="data">Float data to send.</param>
	public void Send(string receiver,float data){
		PD.Messaging.Send(receiver, new Float(data));	
	}
	/// <summary>
	/// Function to send a symbol to a receiver in some patch loaded in pure data.
	/// </summary>
	/// <param name="receiver">Name of receiver in the patch (remind the dollarzero of the patch).</param>
	/// <param name="data">Symbol data to send.</param>
	public void Send(string receiver,string data){
		PD.Messaging.Send(receiver, new Symbol(data));	
	}
	/// <summary>
	/// Function to send a Bang to a receiver in some patch loaded in pure data.
	/// </summary>
	/// <param name="receiver">Name of receiver in the patch (remind the dollarzero of the patch).</param>
	public void Send(string receiver){
		PD.Messaging.Send(receiver, new Bang());	
	}
	/// <summary>
	/// Function to change state of process audio.
	/// </summary>
	/// <param name="state">True to put available audio, false to disable audio.</param>
	public void Compute(bool state){
		if (state) {
			PD.Start ();
			pdDsp = true;
		} else {
			PD.Stop ();
			pdDsp = false;
		}
	}
	/// <summary>
	/// Function to do the process audio in pure data, pass the input buffers and output buffers.
	/// </summary>
	/// <param name="length">Size of buffer in unity.</param>
	/// <param name="channels">Number of channels ouput of unity.</param>
	/// <param name="input">Buffer of input pass to pure data.</param>
	/// <returns> Buffer of audio processed from pure data. </returns>
	public float[] Process_Audio(int length,int channels,float[] input){
		float[] output=new float[length];
		PD.Process ((int)(length / PD.BlockSize / channels), input, output);
		return output;
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
			HRIRuPath=Path.GetDirectoryName(System.IO.Directory.GetFiles(Application.dataPath, "HRIRu.cs", SearchOption.AllDirectories)[0])+Path.DirectorySeparatorChar+".."+Path.DirectorySeparatorChar;
			PD=new Pd(numberOfInputChannel, numberOfOutputChannel, AudioSettings.outputSampleRate,new List<string>() {HRIRuPath+"StreamingAssets"});
			//Uses this to get prints of Pure Data
			/*PD.Messaging.Print += delegate(object sender, PrintEventArgs e) {
				Debug.Log(e.Symbol.Value);
			};*/
			OpenNewPdPatch(HRIRuPath+"StreamingAssets"+Path.DirectorySeparatorChar+"pdManager.pd");
			if (numberOfOutputChannel != targetMixerGroups.Length * 2) {
				Debug.LogWarning ("The number of output channel is not equal to the number of mixer group!");
				Debug.LogWarning ("Set number of output channel to " + (targetMixerGroups.Length * 2).ToString ());
				numberOfOutputChannel = targetMixerGroups.Length * 2;
			}
			createPdMixer ();
			if (startDspOnStart) {
				PD.Start ();
				pdDsp = true;
			}
		} else if (!Instance.Equals((object)this)){
			Destroy (gameObject);
		}
	}
		
	/// <summary>
	/// Function to start record from Mic_Device.
	/// </summary>
	public void Avaible_Mic(){
		if(numberOfInputChannel<=0){
			numberOfInputChannel = Ni;
		}
		if (numberOfInputChannel > 0) {
			Is_Device = false;
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
			isRecording = true;
		}
	}
	/// <summary>
	/// Function to stop record from Mic_Device.
	/// </summary>
	public void Disable_Mic(){
		Microphone.End (Mic_Device);
		Ni = numberOfInputChannel;
		numberOfInputChannel = 0;
		isRecording = false;
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

	void Start () {
		Avaible_Mic();
	}


	void OnApplicationQuit(){
		PD.Dispose ();
	}
}
