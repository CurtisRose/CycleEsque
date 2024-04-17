using UnityEngine;
using System.Collections.Generic;

public class SoundRandomizer
{
    private List<AudioClip> audioClips;  // List to store the AudioClips
    float pitchVariation;
    float volumeVariation;

    // Constructor to initialize the SoundRandomizer with a list of AudioClips
    public SoundRandomizer(List<AudioClip> clips)
    {
        audioClips = new List<AudioClip>(clips);  // Make a copy of the list provided
    }

    // Method to get a random AudioClip
    public AudioClip GetRandomClip()
    {
        if (audioClips.Count == 0)
        {
            Debug.LogError("No audio clips are available in the SoundRandomizer.");
            return null;
        }
        int index = Random.Range(0, audioClips.Count);  // Get a random index
        return audioClips[index];  // Return the AudioClip at the random index
    }
}