using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
	public Dictionary<PlayerNoiseLevel, float> noiseDistances = new Dictionary<PlayerNoiseLevel, float>()
	{
		{ PlayerNoiseLevel.None, 0f },
		{ PlayerNoiseLevel.Low, 20f },    // walking
        { PlayerNoiseLevel.Medium, 40f }, // running
        { PlayerNoiseLevel.High, 50f },   // jumping, attacking
        { PlayerNoiseLevel.VeryHigh, 75f }, // shooting
        { PlayerNoiseLevel.Deafening, 150 } // explosions
    };

	public static SoundManager Instance;

	[SerializeField] protected bool DrawGizmos;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
		} else {
			Destroy(this);
		}
	}

	protected void EmitNoise(PlayerNoiseLevel noiseLevel, Vector3 position) {
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

	public virtual void RegisterSound(PlayerNoiseLevel noiseLevel, Vector3 position, bool ForceSound = false) {
		EmitNoise(noiseLevel, position);
	}

	protected Dictionary<PlayerNoiseLevel, Color> noiseColors = new Dictionary<PlayerNoiseLevel, Color>() {
		{ PlayerNoiseLevel.None, Color.clear }, // No gizmo for None
        { PlayerNoiseLevel.Low, Color.red },
		{ PlayerNoiseLevel.Medium, Color.yellow },
		{ PlayerNoiseLevel.High, Color.green },
		{ PlayerNoiseLevel.VeryHigh, Color.blue },
		{ PlayerNoiseLevel.Deafening, Color.magenta } // Magenta for Deafening, as an example
    };

	public virtual void OnDrawGizmos() {
		if (!DrawGizmos)
			return;

		// Draw a sphere for each noise level
		foreach (var noiseLevel in noiseDistances) {
			if (noiseLevel.Key != PlayerNoiseLevel.None) { // Skip drawing for 'None'
				Gizmos.color = noiseColors[noiseLevel.Key];
				Gizmos.DrawWireSphere(transform.position, noiseLevel.Value);
			}
		}
	}
}
