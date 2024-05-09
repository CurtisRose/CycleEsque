using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lazer : MonoBehaviour
{
	[SerializeField] float length = 5.0f; // Length of the laser gizmo
	[SerializeField] Color color = Color.red; // Color of the laser
	[SerializeField] Vector3 direction = Vector3.forward; // Direction of the laser

	void OnDrawGizmos() {
		Gizmos.color = color; // Set the color for the Gizmos
		Vector3 startPosition = transform.position; // Start position of the laser
		Vector3 endPosition = startPosition + transform.TransformDirection(direction.normalized) * length; // Calculate end position based on direction and length

		Gizmos.DrawLine(startPosition, endPosition); // Draw the laser line
	}
}
