<p align="center"> <img src="https://goo.gl/gPNg5w" width="100"/> </p>
<p align="center">
    <a href="https://unity3d.com/es/unity" target="_blank">
        <img src="https://img.shields.io/badge/Unity-2017.4.1-blue.svg" alt="Unity 2017.4.1">
    </a>
    <a href="https://puredata.info/" target="_blank">
        <img src="https://img.shields.io/badge/Pure_Data-0.48--0-green.svg" alt="Pure Data 0.48">
    </a>
    <a href="https://github.com/libpd/libpd/" target="_blank">
        <img src="https://img.shields.io/badge/Libpd-C%23-brightgreen.svg" alt="Libpd C#">
    </a>
</p>

HRIRu is an API for spatialize sound sources in Unity engine, using [hrir~](http://onkyo.u-aizu.ac.jp/index.php/software/hrir/) , a sound spatializer developed in Pure Data capable of spatialization between distance $20 \leqslant d \leqslant 160$ cm, azimuth $0^o \leqslant \theta \leqslant 360^o$, and elevation $-40^o \leqslant \phi \leqslant 90^o$.


This implementacion is based on [UnityLibpd](https://github.com/Wilsonwaterfish/UnityLibpd) by Wing Sang Wong, using the C# API of Libpd to embed Pure Data in Unity.

# Getting start

This API is tested in Unity 2017.4.1f1, to get it, just copy the files in an Editor directory within your unity Assets folder.

For the moment, this API only works for MacOS.
>If you are using Plugins in your project, mind that HRIRu have the own Plugins directory, so, you can move the files of this directory in your own Plugins directory.

# Usage

The API has 2 scripts, a Manager as a control of Pure Data instance and HRIRu, for a sound source object.

## PdManager

Following the idea of [UnityLibpd](https://github.com/Wilsonwaterfish/UnityLibpd), PdManager is the main interface of Pure Data, it requires to be added in the scene to an object without destroyer method (e.g., a camera object).

<p align="center">
<img src="https://goo.gl/Yux9qU" width="200">
</p>

### Properties
|Propiertie                          |Description
|-------------------------------|-----------------------------|
|Target Mixer Groups            | In the Element0 put the audio mixer group that you want to get output of spatialized audio, this channel would be only used for this purpose. |
|Pd Dsp            |This propiertie say the state of DPS for instance of Pure Data. If you check this, the PdManager start to process the signal audio when this object is instantiated, in otherwise, you need to start the DSP with `compute` function.|
|Use Mic            |If you check this, the PdManager start to get audio signal from Microphone specified in `Mic Device`, in otherwise, you can start to use the microphone with `Avaible_Mic` function.|
|Mic Device            |This especified the device to get microphone signal, in otherwise, it use the defualt microphone for Unity.|

### Methods

|Function | Parameters | Description
|-----------|--------------------|-----------------------------|
|`Compute`  | `bool state`: default is true | This function change state of DSP in Pure Data, if the parameter is true it put available compute audio from Pure Data, in otherwise, you can put false.|
|`Avaible_Mic`  | Not parameters | This function set up the microphone specified in `Mic Device` as an input signal for Pure Data. Note: the sample rate by default in unity is 48000 Hz.|
|`Disable_Mic`  | Not parameters | This function quiet a microphone as an input signal for Pure Data.|
>All of these methods return void.

In the code, you can access these methods and properties as follows:

```csharp
PdManager.Instance.compute(true);
```

## HRIRu

This script would be added to sounds source objects located near to a scene listener object, this will be processed as an audio player with hrir~ providing options to stop, play once, loop a WAV file, and using a microphone input. Location coordinates of sound sources relative to the location of a listener are automatically computed.

<p align="center">
<img src="https://goo.gl/7LRNcN" width="200">
</p>

### Properties

|Propiertie                          |Description
|-------------------------------|-----------------------------|
|Scale           | Is a scale of the distance from the listener object to sound source, according to the units used to define the geometry of objects. By default is 1.|
|Listener        | This propierty specified the listener in the scene, by default is the Audio Listener in the scene.|
|isPlaying       | This property says the state of the sound spatializer, if is true, it computes for all `Update` times the coordinates from the listener object and processes the audio in Pure Data, false in otherwise.|

### Methods

|Function | Parameters | Description
|-----------|---------------|----------|------------------------|
|`Volume`  | `float f` | This function changes the volume for input of the spatializer, By default is set to 1, the maximum value is 10.|
|`Play`  | `string song` | This function loads a WAV file and plays it just once, the `song` parameter specified the path of the WAV file.|
|`Play_Loop`  | `string song` | This function loads a WAV file and plays it in a loop, the `song` parameter specified the path of the WAV file.|
|`Stop`  | No parameters | This function stops all songs are playing at the moment.|
|`Mic`  | `bool available`; Default is true| If available is true, the sound source use the input signal of Pure Data (the microphone) and processes it with the spatializer, false in otherwise.|
|`Available`  | No parameters| This functions set up the spatializer for a sound source. By default, the spatializer is not instantiated, so, it is necessary to use for start the spatializer.|
|`Disable`  | No parameters| This function disables the spatializer and removes the memory load from it.|

>The path of WAV song files start in the Assets directory, so, if you want to play a song.wav in a Sounds/ directory into Assets folder, you call HRIRu.Play("Sounds/song.wav").

In the code, to access these methods and properties in a sound source object, do it as follows:

```csharp
HRIRu hrir_control=this.GetComponent<HRIRu>();
hrir_control.Available();
hrir_control.Play("Sounds/song.wav");
```