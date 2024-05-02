using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Health
{
	PlayerGearManager gearManager;

	[SerializeField] List<AudioClip> painSounds;
	SoundRandomizer painRandomClips;
	[SerializeField] AudioSource audioSource;

	protected override void Start() {
		base.Start();
		gearManager = GetComponent<PlayerGearManager>();
		painRandomClips = new SoundRandomizer(painSounds);
		audioSource = GetComponent<AudioSource>();
	}

	protected override float CalculateDamage(float amount) {
		// For now we'll just do based on armor. Later it should be based on which body part or something like that.
		float armorValue = gearManager.GetArmorValue();
		// each point of armor reduces damage by 10%?
		amount -= amount * (armorValue * 0.1f);

		MakeSound(painRandomClips.GetRandomClip());

		return base.CalculateDamage(amount);
	}

	public void MakeSound(AudioClip audioClip) {
		audioSource.pitch = Random.Range(0.95f, 1.05f); // Adjust pitch slightly
		audioSource.volume = Random.Range(0.8f, 1.0f); // Adjust volume slightly
		audioSource.PlayOneShot(audioClip);
	}
}
