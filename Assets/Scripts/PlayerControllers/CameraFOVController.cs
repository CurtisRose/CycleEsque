using System.Collections;
using UnityEngine;

public class CameraFOVController : MonoBehaviour {

	public static CameraFOVController Instance;
	Camera camera;

	private Coroutine fovCoroutine;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
		} else {
			Destroy(this);
		}
		camera = GetComponent<Camera>();
	}

	public void SetFOV(float fov, float duration) {
		if (fovCoroutine != null) {
			StopCoroutine(fovCoroutine);  // Stop the currently running coroutine
		}
		fovCoroutine = StartCoroutine(ChangeFOV(fov, duration));  // Start a new coroutine and store the reference
	}

	IEnumerator ChangeFOV(float targetFOV, float duration) {
		float timeElapsed = 0;
		float startFOV = camera.fieldOfView;  // Start from the current FOV

		while (timeElapsed < duration) {
			// Interpolate the field of view based on the elapsed time
			camera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, timeElapsed / duration);

			// Increment the elapsed time by the time passed since the last frame
			timeElapsed += Time.deltaTime;

			// Yield until the next frame
			yield return null;
		}

		// Ensure the final field of view is set to the target value
		camera.fieldOfView = targetFOV;
		fovCoroutine = null;  // Reset the coroutine reference once complete
	}
}