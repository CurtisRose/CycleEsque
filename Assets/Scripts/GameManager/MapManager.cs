using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapManager : MonoBehaviour
{
	public static MapManager Instance;

	[SerializeField] private Quadtree poiQuadtree;

	[SerializeField] private List<PointOfInterest> pointsOfInterest;
	private List<PointOfInterest> activePointsOfInterest = new List<PointOfInterest>();


	[SerializeField] private List<Player> players;
	[SerializeField] private float activationDistance = 50.0f; // Distance to activate monsters
	// extendedDistance is the diagonal distance of a square with side length 1, since later we are checking with a circle
	// This is used to measure distance from the player to POI
	[SerializeField] private float checkInterval = 1.0f; // How often to check for player proximity
	private float checkTimer = 0;

	[SerializeField] private float MAPSIZEX = 1000;
	[SerializeField] private float MAPSIZEY = 1000;
	[SerializeField] private int MAX_OBJECTS = 0;
	[SerializeField] private int MAX_LEVELS = 5;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
		} else {
			Destroy(this);
		}
		poiQuadtree = new Quadtree(0, new Rect(transform.position.x, transform.position.z, MAPSIZEX, MAPSIZEY), MAX_OBJECTS, MAX_LEVELS); // Set bounds appropriately for your game area
	}

	void Start() {
		InitializePointsOfInterest();
		UpdatePointsOfInterest();
	}

	void Update() {
		checkTimer += Time.deltaTime;
		if (checkTimer >= checkInterval) {
			checkTimer = 0;
			UpdatePointsOfInterest();
		}
	}

	private void InitializePointsOfInterest() {
		foreach (PointOfInterest poi in pointsOfInterest) {
			poiQuadtree.Insert(poi.gameObject);
			poi.Deactivate();
		}
	}

	private void UpdatePointsOfInterest() {
		HashSet<PointOfInterest> newlyActivePOIs = new HashSet<PointOfInterest>();

		float extendedDistance = 2 * activationDistance;
		foreach (Player player in players) {
			// TODO: should probably subscribe to player death event and remove from list
			if (player == null) continue; // Skip if player is null (e.g. player has been destroyed

			// Convert player position to be relative to the Quadtree's origin
			float relativeX = player.transform.position.x - transform.position.x;
			float relativeZ = player.transform.position.z - transform.position.z;

			// Define the search area around each player
			Rect nearbyArea = new Rect(
				player.transform.position.x - extendedDistance / 2,
				player.transform.position.z - extendedDistance / 2,
				extendedDistance,
				extendedDistance
			);

			// Retrieve all monsters within the nearby area
			List<GameObject> nearbyPOI = poiQuadtree.Query(nearbyArea);
			foreach (GameObject poiObj in nearbyPOI) {
				PointOfInterest poi = poiObj.GetComponent<PointOfInterest>();
				if (poi != null) {
					float distance = Vector3.Distance(poi.transform.position, player.transform.position);
					if (distance <= activationDistance) {
						poi.Activate();
						newlyActivePOIs.Add(poi);
					}
				}
			}
		}

		// Deactivate any previously active spawners that are no longer in range
		foreach (PointOfInterest poi in activePointsOfInterest) {
			if (!newlyActivePOIs.Contains(poi)) {
				poi.Deactivate();
			}
		}

		// Update the active spawners list
		activePointsOfInterest = new List<PointOfInterest>(newlyActivePOIs);
	}

	private void OnDrawGizmos() {
		float extendedDistance = 2 * activationDistance;

		// Draw the search area for each player
		if (players != null) {
			foreach (Player player in players) {
				if (player == null) continue; // Skip if player is null (e.g. player has been destroyed)
				Gizmos.color = Color.magenta;
				Gizmos.DrawWireCube(player.transform.position, new Vector3(extendedDistance, 0, extendedDistance));
			}
		}

		// Draw the monsterSpawners activation distance as a sphere
		// If it's active draw it in green, otherwise in red
		foreach (PointOfInterest poi in pointsOfInterest) {
			if (poi.IsActive() && !poi.isDeactivating) {
				Gizmos.color = Color.green;  // Active and not deactivating
			} else if (poi.IsActive() && poi.isDeactivating) {
				Gizmos.color = Color.yellow;  // Active but pending deactivation
			} else {
				Gizmos.color = Color.red;  // Fully deactivated
			}

			Gizmos.DrawWireSphere(poi.transform.position, activationDistance);
		}

		// Draw the quadTree
		if (poiQuadtree != null) {
			poiQuadtree.DrawQuadtree(poiQuadtree);
		}
	}
}
