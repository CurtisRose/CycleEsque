using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerNoiseLevel {
	None,
	Low,    // walking
	Medium, // running
	High,    // jumping, attacking
	VeryHigh, // Shooting
	Deafening // Explosions
}

public class PlayerSoundController : SoundManager {
	new public static PlayerSoundController Instance;

	private PlayerNoiseLevel maxNoiseLevelThisInterval = PlayerNoiseLevel.None;
	private int framesUntilNextEmission = 0;
	private int emissionInterval = 10; // Emit noise every 60 frames
	private Vector3 latestPosition;


	private void Awake() {
		if (Instance == null) {
			Instance = this;
		} else {
			Destroy(this);
		}
	}

	private void Update() {
		if (framesUntilNextEmission <= 0) {
			if (maxNoiseLevelThisInterval != PlayerNoiseLevel.None) {
				EmitNoise(maxNoiseLevelThisInterval, transform.position);
				maxNoiseLevelThisInterval = PlayerNoiseLevel.None;
			}
			framesUntilNextEmission = emissionInterval;
		} else {
			framesUntilNextEmission--;
		}
	}

	public override void RegisterSound(PlayerNoiseLevel noiseLevel, Vector3 position, bool ForceSound = false) {
		if (ForceSound) {
			EmitNoise(noiseLevel, position);
			return;
		}
		//Debug.Log("Registered sound at " + noiseLevel.ToString());
		if (noiseLevel > maxNoiseLevelThisInterval) {
			maxNoiseLevelThisInterval = noiseLevel;
		}
		latestPosition = position;
	}

	public override void OnDrawGizmos() {
		if (!DrawGizmos)
			return;
		if (maxNoiseLevelThisInterval != PlayerNoiseLevel.None && noiseDistances.ContainsKey(maxNoiseLevelThisInterval)) {
			Gizmos.color = noiseColors[maxNoiseLevelThisInterval];
			Gizmos.DrawWireSphere(latestPosition, noiseDistances[maxNoiseLevelThisInterval]);
		}
	}
}
