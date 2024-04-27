using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtractionHandler : MonoBehaviour
{
	[SerializeField] private float timeToExtract;

	IEnumerator LoadStartScene() {
		yield return new WaitForSeconds(timeToExtract);
		GameManager.Instance.SavePlayerData();
		SceneManagerHelper.LoadSceneWithPlayerData("SpaceStation");
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Player") {
			// Extract the player
			StartCoroutine(LoadStartScene());
		}
	}
}
