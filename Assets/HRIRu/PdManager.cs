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
	
	private int numberOfInputChannel = 1; //The number of input channels to spatializer, if it is greater than 0, it will start to listen the Mic_Device, the recommended value is 1 to listen mono signal of the microphone.
	private int numberOfOutputChannel = 2;//The number of output channels from spatializer, the recommended value is 2, HRIR spatializer is focus to stereo systems.
	/// <summary>
	/// Mixers to the ouput signal processed by Pure data.
	/// </summary>
	public AudioMixerGroup MixerChannel;
	private GameObject PdMixer; //Object where is attached the Mixer channel, because GameObject cant has AudioListener and AudioSource
	/// <summary>
	/// State of Dsp of Pure-data instance, True if audio is computing, false in otherwise.
	/// If script init with pdDsp true, they start computing audio from Pure-data.
	/// </summary>
	public bool pdDsp = true;// Is computing audio?.
	private Pd PD; //Instance of pure data
	private Patch manager; //Patch of PdManager	
	private List<Patch> _loadedPatches = new List<Patch>(); //All patches loaded
	/// <summary>
	/// This variable indicates if the instance of pure data start with micrhophone or not.	
	/// </summary>
	public bool StartWithMic=false;
    private bool ActMic = false;// It indicates the state of microphone divice, true if is recording and false in otherwise.
    /// <summary>
    /// Microphone device to take a signal to spatializer, if it is null, it will take the default microphone for unity.
    /// </summary>
    public string MicDevice="";

	//	
	private bool Is_Device=false; //Exists this device.	
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
	/// This function returns the absolute path for the file request on the HRIRu path in StreamingAssets.
	/// </summary>
    /// <param name="file">Name of file. </param>
	/// <returns> Absolute path of the file. </returns>
	public string APIPath(string file)
    {
        return Path.Combine(Application.streamingAssetsPath,"HRIRu/"+file);
    }
    /// <summary>
    /// This function returns the input channels for microphone, if it is greater than 0 means that pure data 
    /// is using the microphone as input.
    /// </summary>
    /// <returns> Number of input channels. </returns>
    public int getNumberInputs(){
		return numberOfInputChannel;
	}
	/// <summary>
	/// This function returns the output channels for microphone.
	/// </summary>
	/// <returns> Number of output channels. </returns>
	public int getNumberOutputs(){
		return numberOfOutputChannel;
	}
    /// <summary>
	/// This function returns the state of the Mic., True if is active and false in otherwise.
	/// </summary>
	/// <returns> Returns the state of the Mic. </returns>
    public bool MicState()
    {
        return ActMic;
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
		PdMixer = new GameObject ("PdMixer");
		PdMixer.AddComponent<PdStereo>();
		if (PdMixer.GetComponent<AudioSource> () == null)
			PdMixer.AddComponent<AudioSource>();
		PdMixer.GetComponent<PdStereo>().setMixerGroup(MixerChannel);
		DontDestroyOnLoad (PdMixer);
	}
	//Set up the instance of Pure-data
	void Awake()
	{
		if (_instance == null) {
			_instance = this;
			DontDestroyOnLoad (gameObject);            
            PD =new Pd(numberOfInputChannel, numberOfOutputChannel, AudioSettings.outputSampleRate);
			//Uses this to get prints of Pure Data
			PD.Messaging.Print += delegate(object sender, PrintEventArgs e) {
				Debug.Log(e.Symbol.Value);
			};
			manager=PD.LoadPatch(APIPath("pdManager.pd"));
			if (MixerChannel == null)
				Debug.LogWarning ("Not found mixer channel...");
			createPdMixer ();
			if (pdDsp) {
				PD.Start ();
			}			
		} else if (!Instance.Equals((object)this)){
			Destroy (gameObject);
		}
	}

    /// <summary>
    /// Function to start record from Mic_Device.
    /// <param name="Device">Specifies the name of the input device for the microphone, by defualt it takes the Mic Device specified in Pd Manager.</param>
    /// </summary>
    public void Available_Mic(string Device=""){
        if (Device != "")
            MicDevice = Device;
		Is_Device = false;
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("Not Microphone devices found");
            return;
        }
		foreach (string device in Microphone.devices) {
			if(MicDevice==device){
				Is_Device = true;
				break;
			}
		}
		if (!Is_Device)
            PdMixer.GetComponent<PdStereo>().AvailableMicrophone(null);
        else
            PdMixer.GetComponent<PdStereo>().AvailableMicrophone(MicDevice);        
		ActMic = true;
	}
	/// <summary>
	/// Function to stop record from Mic_Device.
	/// </summary>
	public void Disable_Mic(){
        PdMixer.GetComponent<PdStereo>().DisableMicrophone();
        ActMic = false;
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