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
	
	private int numberOfInputChannel = 0; //The number of input channels to spatializer, if it is greater than 0, it will start to listen the Mic_Device, the recommended value is 1 to listen mono signal of the microphone.
	private int numberOfOutputChannel = 2;//The number of output channels from spatializer, the recommended value is 2, HRIR spatializer is focus to stereo systems.
	/// <summary>
	/// Mixers to the ouput signal processed by Pure data.
	/// </summary>
	public AudioMixerGroup MixerChannel;
	private GameObject MixerObject; //Object where is attached the Mixer channel, because GameObject cant has AudioListener and AudioSource
	/// <summary>
	/// State of Dsp of Pure-data instance, True if audio is computing, false in otherwise.
	/// If script init with pdDsp true, they start computing audio from Pure-data.
	/// </summary>
	public bool pdDsp = true;// Is computing audio?.
	private Pd PD; //Instance of pure data
	private Patch manager; //Patch of PdManager
	private string HRIRuPath="";// Path of the API HRIRu.
	private List<Patch> _loadedPatches = new List<Patch>(); //All patches loaded
	/// <summary>
	/// This variable indicates if the instance of pure data start with micrhophone or not.
	/// Also, it indicates the state of microphone divice, true if is recording and false in otherwise.
	/// </summary>
	public bool useMic=false;
	/// <summary>
	/// Microphone device to take a signal to spatializer, if it is null, it will take the default microphone for unity.
	/// </summary>
	public string Mic_Device="";

	//
	private AudioClip Mic; //Class to recording from microphone.
	private bool Is_Device=false; //Exists this device.
	private float[] PDMic_Input; //Buffer given from microphone.
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
	/// This functions returns the input channels for microphone, if it is greater than 0 means that pure data 
	/// is using the microphone as input.
	/// </summary>
	/// <returns> Number of input channels. </returns>
	public int getNumberInputs(){
		return numberOfInputChannel;
	}
	/// <summary>
	/// This functions returns the output channels for microphone.
	/// </summary>
	/// <returns> Number of output channels. </returns>
	public int getNumberOutputs(){
		return numberOfOutputChannel;
	}
	/// <summary>
	/// This fuctions returns the HRIRu path.
	/// </summary>
	/// <returns> String with path of HRIRu API. </returns>
	public string APIPath(){
		return HRIRuPath;
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
	/// <param name="state">True to put available audio, false to disable audio. Default is true.</param>
	public void Compute(bool state=true){
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
	public void Process_Audio(int length,int channels,float[] input,float[] output){
		PD.Process ((int)(length / PD.BlockSize / channels), input, output);
	}

	//Attach the mixer to MixerObject.
	private void createPdMixer(){
		MixerObject = new GameObject ();
		MixerObject.AddComponent<PdStereo>();
		if (MixerObject.GetComponent<AudioSource> () == null)
			MixerObject.AddComponent<AudioSource>();
		MixerObject.GetComponent<PdStereo>().setMixerGroup(MixerChannel);
	}
	//Set up the instance of Pure-data
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
			manager=PD.LoadPatch(HRIRuPath+"StreamingAssets"+Path.DirectorySeparatorChar+"pdManager.pd");
			if (MixerChannel == null)
				Debug.LogWarning ("Not found mixer channel...");
			createPdMixer ();
			if (pdDsp) {
				PD.Start ();
			}
			if(useMic)
				Avaible_Mic();
		} else if (!Instance.Equals((object)this)){
			Destroy (gameObject);
		}
	}
		
	/// <summary>
	/// Function to start record from Mic_Device.
	/// </summary>
	public void Avaible_Mic(){
		numberOfInputChannel = 1;
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
		PDMic_Input=new float[1024*numberOfInputChannel];
		useMic = true;
	}
	/// <summary>
	/// Function to stop record from Mic_Device.
	/// </summary>
	public void Disable_Mic(){
		Microphone.End (Mic_Device);
		numberOfInputChannel = 0;
		useMic = false;
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
	//Close all patchs open at the moment and release the memory of instance of Pure-datas
	private void Close(){
		foreach (Patch patch in _loadedPatches) {
			patch.Dispose ();
		}
		_loadedPatches.Clear ();
		manager.Dispose ();
		PD.Dispose ();
		pdDsp = false;
	}
		
	void OnApplicationQuit(){
		Close ();
	}
}