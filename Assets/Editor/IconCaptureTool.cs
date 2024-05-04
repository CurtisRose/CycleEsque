using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class IconCaptureTool : EditorWindow {
	private Camera iconCamera;
	private string path = "Assets/Icons/";
	public List<GameObject> objectsToCapture = new List<GameObject>();

	[MenuItem("Tools/Icon Capture Tool")]
	public static void ShowWindow() {
		GetWindow(typeof(IconCaptureTool));
	}

	void OnGUI() {
		iconCamera = (Camera)EditorGUILayout.ObjectField("Camera", iconCamera, typeof(Camera), true);
		SerializedObject serializedObject = new SerializedObject(this);
		SerializedProperty prop = serializedObject.FindProperty("objectsToCapture");
		EditorGUILayout.PropertyField(prop, true);
		serializedObject.ApplyModifiedProperties();

		path = EditorGUILayout.TextField("Path", path);

		if (GUILayout.Button("Capture Icons")) {
			CaptureIcons();
		}
	}

	private void CaptureIcons() {
		foreach (GameObject obj in objectsToCapture) {
			obj.SetActive(true);
			TakeScreenshot(obj.name);
			obj.SetActive(false);
		}
	}

	private void TakeScreenshot(string name) {
		RenderTexture rt = new RenderTexture(500, 500, 24);
		iconCamera.targetTexture = rt;
		Texture2D screenShot = new Texture2D(500, 500, TextureFormat.RGBA32, false);
		iconCamera.Render();
		RenderTexture.active = rt;
		screenShot.ReadPixels(new Rect(0, 0, 500, 500), 0, 0);
		screenShot.Apply();

		// Replace red background with transparency
		Color backgroundColor = Color.black;  // Define the background color to replace
		Color transparent = new Color(0, 0, 0, 0);  // Transparent color
		for (int x = 0; x < screenShot.width; x++) {
			for (int y = 0; y < screenShot.height; y++) {
				if (screenShot.GetPixel(x, y) == backgroundColor) {
					screenShot.SetPixel(x, y, transparent);
				}
			}
		}
		screenShot.Apply();  // Apply pixel changes to the texture

		byte[] bytes = screenShot.EncodeToPNG();
		string filename = Path.Combine(path, name + ".png");
		File.WriteAllBytes(filename, bytes);

		iconCamera.targetTexture = null;
		RenderTexture.active = null;
		DestroyImmediate(rt);
	}
}
