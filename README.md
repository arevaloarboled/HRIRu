<center> <img src="https://goo.gl/gPNg5w" width="100"/> </center>
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
>If you are using Plugins in your project, mind that HRIRu have the own Plugins directory, so, you can move the files of this directory in your own Plugins directory.

# Usage

The API has 2 scripts, a Manager as a control of Pure Data instance and HRIRu, for a sound source object.
<!--
<center>
<img src="https://goo.gl/TzezhQ" width="500">
</center>
<center>
Context of HRIRu.
</center>
-->
## PdManager

Following the idea of UnityLibpd, PdManager is the main interface of Pure Data, it requires to be added in the scene to an object without destroyer method (e.g., a camera object).

<center>
<img src="https://goo.gl/Yux9qU" width="200">
</center>

### Properties
|Propiertie                          |Function                 
|-------------------------------|-----------------------------|
|Target Mixer Groups            | In the Element0 put the audio mixer group that you want to get output of spatialized audio, this channel would be only used for this purpose. |
|Pd Dsp            |If you check this, the PdManager start to process the signal audio when this object are isntanciate, in otherwise, you need to start the DSP with `compute` function.|