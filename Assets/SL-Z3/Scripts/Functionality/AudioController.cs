using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioController : MonoBehaviour
{
  [SerializeField] private AudioSource bg_adudio;
  [SerializeField] internal AudioSource audioPlayer_wl;
  [SerializeField] internal AudioSource audioPlayer_button;
  [SerializeField] internal AudioSource audioSpin_button;
  [SerializeField] private AudioClip[] clips;

  private void Start()
  {
    if (bg_adudio) bg_adudio.Play();
    audioPlayer_button.clip = clips[clips.Length - 1];
    audioSpin_button.clip = clips[clips.Length - 2];
  }

  private readonly Dictionary<AudioSource, bool> preFocusMuteState = new Dictionary<AudioSource, bool>();
  private bool isForceMuted = false;

  private AudioSource[] AllSources()
  {
    return new AudioSource[] { bg_adudio, audioPlayer_wl, audioPlayer_button, audioSpin_button };
  }

  //Focus-driven mute — called from BOTH UIManager.OnFocusChanged (JS bridge) and OnApplicationFocus.
  //Guarded so a duplicate call for the same direction can't clobber the stored "restore to" state.
  internal void SetMuteAll(bool forceMute)
  {
    if (forceMute == isForceMuted) return;
    isForceMuted = forceMute;

    foreach (AudioSource source in AllSources())
    {
      if (source == null) continue;
      if (forceMute)
      {
        preFocusMuteState[source] = source.mute;
        source.mute = true;
      }
      else
      {
        source.mute = preFocusMuteState.TryGetValue(source, out bool prevMuted) ? prevMuted : source.mute;
      }
    }

    if (!forceMute) preFocusMuteState.Clear();
  }

  internal void PlayWLAudio(string type)
  {
    audioPlayer_wl.loop = false;
    int index = 0;
    switch (type)
    {
      case "spin":
        index = 0;
        audioPlayer_wl.loop = true;
        break;
      case "win":
        index = 1;
        break;
      case "megaWin":
        index = 2;
        break;
      case "Flip":
        index = 3;
        break;
    }
    StopWLAaudio();
    audioPlayer_wl.clip = clips[index];
    audioPlayer_wl.Play();

  }

  internal void PlayButtonAudio()
  {
    audioPlayer_button.Play();
  }

  internal void PlaySpinButtonAudio()
  {
    audioSpin_button.Play();
  }

  internal void StopWLAaudio()
  {
    audioPlayer_wl.Stop();
    audioPlayer_wl.loop = false;
  }

  internal void StopBgAudio()
  {
    bg_adudio.Stop();
  }

  //User-toggle-driven — the sound/music buttons. An explicit user interaction proves the game has
  //real interactive focus, so a stale forced-mute must never block it.
  internal void ToggleMute(bool toggle, string type = "all")
  {
    SetMuteAll(false);

    switch (type)
    {
      case "bg":
        bg_adudio.mute = toggle;
        break;
      case "button":
        audioPlayer_button.mute = toggle;
        audioSpin_button.mute = toggle;
        break;
      case "wl":
        audioPlayer_wl.mute = toggle;
        break;
      case "all":
        audioPlayer_wl.mute = toggle;
        bg_adudio.mute = toggle;
        audioPlayer_button.mute = toggle;
        audioSpin_button.mute = toggle;
        break;
    }
  }

}
