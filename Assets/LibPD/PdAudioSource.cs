using UnityEngine;
using System.Collections;
using LibPDBinding;

[AddComponentMenu("Audio/PdAudioSource")]
public class PdAudioSource : MonoBehaviour {

	public string pdPatchName;
	public bool playOnAwake = false;
	public LibPDFloat delCheckPlayingState; 
	//public LibPDBang delSelfDestroy;
	public int dollarzero = -999;
	private bool _isPlaying = false;
	private bool _isNew = true;

	public bool isNew{
		get{
			return _isNew;
		}
		set{
			_isNew = value;
		}
	}

	public bool isPlaying{
		get{
			return _isPlaying;
		}
		set{
			_isPlaying = value;
		}
	}

	public void SendFloat (string recv, float v){
		LibPD.SendFloat (dollarzero.ToString ()+"-"+recv, v);
	}

	public void SendBang(string recv){
		LibPD.SendBang (dollarzero.ToString ()+"-"+recv);
	}

	public void SendMessage(string recv, string msg, params object[] args){
		LibPD.SendMessage(dollarzero.ToString ()+"-"+recv,msg,args);
	}

	public void SendList(string recv, params object[] args){
		LibPD.SendList (dollarzero.ToString ()+"-"+recv, args);
	}

	public void SetSpatialBlend (float blend){
		LibPD.SendFloat (dollarzero.ToString () + "-SpatialBlend",blend);
	}

	public void Distance (float distance){
		LibPD.SendFloat (dollarzero.ToString () + "-Distance",distance);
	}

	public void Play(){
		LibPD.SendBang (dollarzero.ToString()+"-Play");
	}

	public void Switch(float onOff){
		LibPD.SendFloat (dollarzero.ToString () + "-Switch", onOff);
	}

	public void SetDSP(float onOff){
		LibPD.SendFloat (dollarzero.ToString () + "-DSP", onOff);
	}

	public void Stop(){
		LibPD.SendBang (dollarzero.ToString()+"-Stop");
	}

	public void PlayLastShot(){
		LibPD.SendBang (dollarzero.ToString()+"-PlayLastShot");
	}

	public void SetAmplitude(float amp){
		LibPD.SendFloat (dollarzero.ToString () + "-Amplitude", amp);
	}

	public void SetPan(float pan){
		LibPD.SendFloat (dollarzero.ToString () + "-Pan", pan);
	}

	public void SetSpeed(float speed){
		LibPD.SendFloat (dollarzero.ToString () + "-Speed", speed);
	}

	public void PlayDelay(float time){
		Invoke ("Play", time);
	}

	public void SetTotalDuration(float time){
		LibPD.SendFloat (dollarzero.ToString() + "-TotalDuration", time);
	}

	public void ClearSequence(){
		LibPD.SendBang (dollarzero.ToString()+"-SequenceClear");
	}

	public void AddSequencerMessage(object[] arguments){
		LibPD.SendList (dollarzero.ToString () + "-SequenceAdd", arguments);
	}

	public void StartSequencer(){
		LibPD.SendBang (dollarzero.ToString () + "-SequenceStart");
	}

	public void CheckPlayingState(string recv, float value){
		if (recv.CompareTo (dollarzero.ToString () + "-isPlaying") == 1) {
			_isPlaying = value==1?true:false;

		}
	}



	void Start(){
		dollarzero = PdManager.Instance.openNewPdPatch (pdPatchName);
		LibPD.Subscribe (dollarzero.ToString () + "-isPlaying");
		delCheckPlayingState = new LibPDFloat(CheckPlayingState);
		LibPD.Float += delCheckPlayingState;
		if (playOnAwake)
			Play ();
	}


	void OnDestroy() {
		LibPD.Float -= delCheckPlayingState;
		LibPD.Unsubscribe (dollarzero.ToString () + "-isPlaying");
		PdManager.Instance.ClosePdPatch (dollarzero);
	}


}
