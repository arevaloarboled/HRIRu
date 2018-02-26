using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using LibPDBinding;

public class PdStereo : MonoBehaviour {

	public int[] selectedChannels = {0,1};
	public bool pullDataFromPd = false;
	public static float[] PdInput = new float[16384];
	public static float[] PdOutput = new float[16384];
	public void setMixerGroup(AudioMixerGroup group){
		GetComponent<AudioSource> ().outputAudioMixerGroup = group;
	}
	// Use this for initialization
	void Start () {
		if (!GetComponent<AudioSource> ()) {
			gameObject.AddComponent<AudioSource> ();
		}
	}
	/*void Update(){
		PdInput = new float[(int)(44100 * PdManager.Instance.numberOfInputChannel)];
		Mic.clip.GetData (PdInput,0);
		LibPD.ProcessRaw (PdInput,PdOutput);
	}*/
	
	void OnAudioFilterRead(float[] data, int channels)
	{
		//input data is not used, please create a separte class for passing audio into pd

		if (pullDataFromPd) {
			//AudioSource aud = GetComponent<AudioSource>();
			if (PdManager.Instance.numberOfInputChannel != 0) {
				//PdInput=PdManager.Instance.get_PDMic_Input();
				PdInput=PdManager.Instance.Get_Audio_Mic();
			} else {
				PdInput = new float[0];
			}
			if (PdManager.Instance.numberOfOutputChannel != 0)
				PdOutput = new float[(int)(data.Length / channels * PdManager.Instance.numberOfOutputChannel)];
			else
				PdOutput = new float[0];
			HRIR[] hrir_list = UnityEngine.Object.FindObjectsOfType<HRIR>();
			float[] mixed=new float[0];
			foreach(HRIR sound_source in hrir_list){
				mixed = sound_source.Process_Audio (data,channels,PdInput);

			}
		}

		if (PdManager.Instance != null) {
			for (int i = 0; i < data.Length / channels; i++) {
				for (int j = 0; j < selectedChannels.Length; j++) {
					data [(i * channels) + j] = PdOutput [(i * PdManager.Instance.numberOfOutputChannel) + selectedChannels [j]];
				}
			}
		}
	}
}
