using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathMenu : MonoBehaviour
{
	[SerializeField] private GameObject deathMenu;
	[SerializeField] float timeToLoadStartScene = 3f;

	private void Start() {
		Player.Instance.GetComponent<Health>().OnDeath += OnPlayerDeath;
	}

	private void OnPlayerDeath() {
		deathMenu.SetActive(true);
		StartCoroutine(LoadStartScene());
	}

	IEnumerator LoadStartScene() {
		yield return new WaitForSeconds(timeToLoadStartScene);
		GameManager.Instance.SavePlayerData();
		SceneManagerHelper.LoadSceneWithPlayerData("SpaceStation");
	}
}
