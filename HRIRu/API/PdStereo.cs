using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using LibPDBinding;

public class PdStereo : MonoBehaviour {

	private int[] selectedChannels = {0,1}; //channel
	private static float[] PdInput = new float[16384]; //input buffer
	private static float[] PdOutput = new float[16384]; //output buffer
	/// <summary>
	/// This function attach the Mixer output channel.
	/// </summary>
	public void setMixerGroup(AudioMixerGroup group){
		GetComponent<AudioSource> ().outputAudioMixerGroup = group;
	}
	// Use this for initialization
	void Start () {
		if (!GetComponent<AudioSource> ()) {
			gameObject.AddComponent<AudioSource> ();
		}
	}
	//Integrate audio to mixer channel
	void OnAudioFilterRead(float[] data, int channels)
	{
		if (PdManager.Instance != null) {
			if (PdManager.Instance.pdDsp) {
				if (PdManager.Instance.getNumberInputs () > 0) {
					PdInput = PdManager.Instance.Get_Audio_Mic ();
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
