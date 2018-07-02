using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using LibPDBinding;

public class PdStereo : MonoBehaviour {

	private int[] selectedChannels = {0,1}; //channel
	private static float[] PdInput = new float[16384]; //input buffer
	private static float[] PdOutput = new float[16384]; //output buffer
    private AudioSource Mic=null; //This load to get the microphone input
    private string Device = "";
	/// <summary>
	/// This function attach the Mixer output channel.
	/// </summary>
	public void setMixerGroup(AudioMixerGroup group){
		GetComponent<AudioSource> ().outputAudioMixerGroup = group;
	}
    /// <summary>
    /// Function to start record from Mic_Device.
    /// <param name="Device">Specifies the name of the input device for the microphone, by defualt it takes the Mic Device specified in Pd Manager.</param>
    /// </summary>
    public void AvailableMicrophone(string device){        
        Device = device;
        Mic.clip = Microphone.Start(Device, true, 3, AudioSettings.outputSampleRate);
        Mic.loop = true;
        while (!(Microphone.GetPosition(null) > 0)) { }
        Mic.Play();        
    }
    public void DisableMicrophone()
    {
        Microphone.End(Device);
        Mic.Stop();
    }
	// Use this for initialization
	void Start () {
		if (!GetComponent<AudioSource> ()) {
			gameObject.AddComponent<AudioSource> ();
		}
        Mic = GetComponent<AudioSource>();
        if (PdManager.Instance.StartWithMic) {
            PdManager.Instance.Available_Mic();
        }
    }
	//Integrate audio to mixer channel
	void OnAudioFilterRead(float[] data, int channels)
	{
		if (PdManager.Instance != null) {
			if (PdManager.Instance.pdDsp) {
				if (PdManager.Instance.MicState ()) {
                    PdInput = new float[1024];
                    for (int i = 0;i< PdInput.Length;i++)
                    {
                        PdInput[i] = data[i * channels];
                    }                    
				} else {
					PdInput = new float[0];
				}
				if (PdManager.Instance.getNumberOutputs () > 0)
					PdOutput = new float[(int)(data.Length / channels * PdManager.Instance.getNumberOutputs ())];
				else
					PdOutput = new float[0];
				PdManager.Instance.Process_Audio (data.Length, channels, PdInput, PdOutput);
				for (int i = 0; i < data.Length / channels; i++) {
					for (int j = 0; j < selectedChannels.Length; j++) {
						//Use to marge into channel mixer with others sounds
						//data [(i * channels) + j] =(data [(i * channels) + j] + PdOutput [(i * PdManager.Instance.numberOfOutputChannel) + selectedChannels [j]])/2;
						//Use for merge into channel dedicate for Pure-data
						data [(i * channels) + j] = PdOutput [(i * PdManager.Instance.getNumberOutputs ()) + selectedChannels [j]];
					}
				}
			}
		}
	}
}
