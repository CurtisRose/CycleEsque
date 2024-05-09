using System.Collections;
using UnityEngine;

public class CameraFOVController : MonoBehaviour {

	public static CameraFOVController Instance;
	Camera camera;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
		} else {
			Destroy(this);
		}
		camera = GetComponent<Camera>();
	}

	public void SetFOV(float fov, float duration) {
		
	}

	/*IEnumerator SetFOV(float fov, float duration) {

	}*/
}