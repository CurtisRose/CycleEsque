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

public class PlayerSoundController : MonoBehaviour
{
	public static PlayerSoundController Instance;

	public Dictionary<PlayerNoiseLevel, float> noiseDistances = new Dictionary<PlayerNoiseLevel, float>()
	{
		{ PlayerNoiseLevel.None, 0f },
		{ PlayerNoiseLevel.Low, 20f },    // walking
        { PlayerNoiseLevel.Medium, 40f }, // running
        { PlayerNoiseLevel.High, 50f },   // jumping, attacking
        { PlayerNoiseLevel.VeryHigh, 75f }, // shooting
        { PlayerNoiseLevel.Deafening, 150 } // explosions
    };

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

	public void RegisterSound(PlayerNoiseLevel noiseLevel, Vector3 position, bool ForceSound = false) {
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

	private void EmitNoise(PlayerNoiseLevel noiseLevel, Vector3 position) {
		float noiseDistance;
		//Debug.Log("Noise at " + noiseLevel.ToString() + " Emitted");
		if (noiseDistances.TryGetValue(noiseLevel, out noiseDistance)) {
			Collider[] hitColliders = Physics.OverlapSphere(position, noiseDistance, LayerMask.GetMask("Monster"));
			if (hitColliders.Length > 0) {
				foreach (Collider hitCollider in hitColliders) {
					MonsterController monster = hitCollider.GetComponent<MonsterController>();
					if (monster != null) {
						monster.HearNoise(position, noiseLevel);
					}
				}
			}
		}
	}

	Dictionary<PlayerNoiseLevel, Color> noiseColors = new Dictionary<PlayerNoiseLevel, Color>() {
		{ PlayerNoiseLevel.None, Color.clear }, // No gizmo for None
        { PlayerNoiseLevel.Low, Color.red },
		{ PlayerNoiseLevel.Medium, Color.yellow },
		{ PlayerNoiseLevel.High, Color.green },
		{ PlayerNoiseLevel.VeryHigh, Color.blue },
		{ PlayerNoiseLevel.Deafening, Color.magenta } // Magenta for Deafening, as an example
    };

	void OnDrawGizmos() {
		// Define colors for each noise level

		// Draw a sphere for each noise level
		foreach (var noiseLevel in noiseDistances) {
			if (noiseLevel.Key != PlayerNoiseLevel.None) { // Skip drawing for 'None'
				Gizmos.color = noiseColors[noiseLevel.Key];
				Gizmos.DrawWireSphere(transform.position, noiseLevel.Value);
			}
		}
	}
}
