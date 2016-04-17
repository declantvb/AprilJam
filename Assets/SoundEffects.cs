using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundEffects : MonoBehaviour {

    public static SoundEffects Singleton;

    private AudioSource[] audioSources;
    private AudioSource musicSource;
    private int nextSource = 0;
    private Dictionary<string, AudioClip> soundEffects = new Dictionary<string, AudioClip>();

    public AudioClip Music;

    public SoundEffects()
    {
        Singleton = this;
    }

    // Use this for initialization
    void Start () {

        audioSources = new AudioSource[10];
        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
        }
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = Music;
        musicSource.loop = true;
        musicSource.Play();

        var clips = Resources.LoadAll<AudioClip>("Sound Effects");
        foreach (var clip in clips)
        {
            soundEffects.Add(clip.name, clip);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Play(string soundEffect)
    {
        audioSources[nextSource].PlayOneShot(soundEffects[soundEffect]);

        nextSource++;
        nextSource = nextSource % audioSources.Length;
    }
}
