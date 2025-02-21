using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioSource[] audios;

    private Dictionary<string, float> audioSourcePlayTimes = new Dictionary<string, float>();

    public string[] AudioSourceLoopPlayings { get => audioSourceLoops.Select(a => a.Key).ToArray(); }
    private Dictionary<string, int> audioSourceLoops = new Dictionary<string, int>();

    private void Awake()
    {
        audios = GetComponentsInChildren<AudioSource>();
    }

    public void PlaySound(string name, float delayTime, float pitch)
    {
        if (audioSourcePlayTimes.ContainsKey(name))
        {
            if (Time.time - audioSourcePlayTimes[name] <= 0)
                return;

            audioSourcePlayTimes[name] = Time.time + delayTime;
        }
        else audioSourcePlayTimes.Add(name, Time.time + delayTime);


        PlaySound(name, pitch);
    }

    public void PlaySound(string name, bool loop, int playID)
    {
        if (loop)
        {
            if (audioSourceLoops.TryGetValue(name, out int myPlayID) && myPlayID != playID)
            {
                StopSound(name, myPlayID);
                audioSourceLoops[name] = playID;
            }
            else audioSourceLoops.Add(name, playID);

            var audio = GetSound(name);

            if (audio)
                audio.loop = true;
        }

        PlaySound(name);
    }

    public void PlaySound(string name) => PlaySound(name, Random.Range(0.85f, 1.15f));

    public void PlaySound(string name, float pitch)
    {
        var audio = GetSound(name);

        if (audio != null && audio.clip != null && audio.clip.name.Equals(name))
        {
            audio.pitch = pitch;
            audio.Play();
        }
    }

    public void StopSound(string name, int playID)
    {
        if (audioSourceLoops.TryGetValue(name, out int myPlayID) && myPlayID == playID)
        {
            var audio = GetSound(name);

            if (audio != null && audio.clip != null && audio.clip.name.Equals(name))
            {
                audio.Stop();
                audio.loop = false;
            }

            audioSourceLoops.Remove(name);
        }
    }

    private AudioSource GetSound(string name)
    {
        foreach (var audio in audios)
        {
            if (audio.clip != null && audio.clip.name.Equals(name))
            {
                return audio;
            }
        }
        return null;
    }
}
