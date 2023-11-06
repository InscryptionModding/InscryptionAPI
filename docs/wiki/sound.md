## Adding Music Tracks to the Gramophone
---
This API supports adding new tracks to the Gramophone in Leshy's Cabin.
(A user must have the Hex disc unlocked in order to be able to listen to the new tracks.)

All you need for that is a regular audio file. The API will do all of the conversion. The file should be inside the 'plugins' folder. and the supported audio formats are MP3, WAV, OGG and AIFF.

You can register your track like this:

```csharp
GramophoneManager.AddTrack(PluginGuid, "MyFile.wav", 0.5f);
```
The first parameter should be your plugin's GUID. The second parameter should be your file's name.
The third parameter is optional, and determines the volume of your track, from 0 to 1f. 

## Converting Audio Files to Unity AudioClip Objects
This API provides a helper method for converting audio files to Unity AudioClip objects so that they can be played in-game with the AudioSource component. You can use this to replace in-game music through patches, or to play your own sound effects.

The audio file should be located inside of the 'plugins' folder. The supported audio formats are MP3, WAV, OGG and AIFF.

You can convert your audio file into an AudioClip object like this:

```csharp
AudioClip audio = SoundManager.LoadAudioClip("Example.mp3");
```