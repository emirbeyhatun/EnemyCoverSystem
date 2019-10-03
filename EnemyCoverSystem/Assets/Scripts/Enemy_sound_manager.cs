using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_sound_manager : MonoBehaviour {

	public AudioSource talkSource;
	public AudioSource hitSource;
	public AudioSource WeaponSource;
	public AudioSource FootstepSource;

    public List<AudioClip> hitClips;
    public List<AudioClip> weaponFireClips;
    public List<AudioClip> talkClips;
    public List<AudioClip> footStepsClips;
    
    public void PlayHitClips()
    {

        int hitClipCount = hitClips.Count;

        if (hitSource)
        {
            System.Random rd = new System.Random();
            hitSource.clip = hitClips[rd.Next(0, hitClipCount)];
            hitSource.Play();
        }
    }

    public void PlayWeaponClips()
    {
        int weapClipCount = weaponFireClips.Count;

        System.Random rd = new System.Random();
        if (WeaponSource)
        {
            if (weapClipCount > 0)
            {
                WeaponSource.clip = weaponFireClips[rd.Next(0, weapClipCount)];
                WeaponSource.Play();
            }

        }
    }

    public void PlayTalkClips()
    {
        if (talkSource)
        {
            System.Random rd = new System.Random();
            if (talkClips.Count > 0)
            {
                talkSource.clip = talkClips[rd.Next(0, talkClips.Count)];
                talkSource.Play();
            }
        }
    }

    public void PlayFootSteps()
    {
        if (FootstepSource)
        {
            System.Random rd = new System.Random();
            if (footStepsClips.Count > 0)
            {
                FootstepSource.clip = footStepsClips[rd.Next(0, footStepsClips.Count)];
                FootstepSource.Play();
            }
        }

    }
}
