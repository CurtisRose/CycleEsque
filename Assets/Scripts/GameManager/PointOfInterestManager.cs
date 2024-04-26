using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointOfInterestManager : MonoBehaviour
{
	public static PointOfInterestManager Instance;

	[SerializeField] private Quadtree poiQuadtree;

	[SerializeField] private List<MonsterSpawner> monsterSpawners;
	private List<MonsterSpawner> activeSpawners = new List<MonsterSpawner>(); // Track active spawners
	
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
		InitializeMonsterSpawners();
		UpdateMonsterSpawners();
	}

	void Update() {
		checkTimer += Time.deltaTime;
		if (checkTimer >= checkInterval) {
			checkTimer = 0;
			UpdateMonsterSpawners();
		}
	}

	public void RegisterMonsterSpawner(MonsterSpawner monsterSpawner) {
		// Add the new monster spawner to the list
		monsterSpawners.Add(monsterSpawner);
		monsterSpawner.Deactivate();
		poiQuadtree.Insert(monsterSpawner.gameObject);
	}

	private void InitializeMonsterSpawners() {
		foreach (MonsterSpawner monsterSpawner in monsterSpawners) {
			poiQuadtree.Insert(monsterSpawner.gameObject); // Assuming monsters have a GameObject component
			monsterSpawner.Deactivate(); // Initially deactivate all monsters
		}
	}

	private void UpdateMonsterSpawners() {
		HashSet<MonsterSpawner> newlyActiveSpawners = new HashSet<MonsterSpawner>();

		float extendedDistance = 2 * activationDistance;
		foreach (Player player in players) {

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
			List<GameObject> nearbyMonsterSpawner = poiQuadtree.Query(nearbyArea);
			foreach (GameObject monsterObj in nearbyMonsterSpawner) {
				MonsterSpawner monsterSpawner = monsterObj.GetComponent<MonsterSpawner>();
				if (monsterSpawner != null) {
					float distance = Vector3.Distance(monsterSpawner.transform.position, player.transform.position);
					if (distance <= activationDistance) {
						monsterSpawner.Activate();
						newlyActiveSpawners.Add(monsterSpawner);
					}
				}
			}
		}

		// Deactivate any previously active spawners that are no longer in range
		foreach (var spawner in activeSpawners) {
			if (!newlyActiveSpawners.Contains(spawner)) {
				spawner.Deactivate();
			}
		}

		// Update the active spawners list
		activeSpawners = new List<MonsterSpawner>(newlyActiveSpawners);
	}

	private void OnDrawGizmos() {
		float extendedDistance = 2 * activationDistance;

		// Draw the search area for each player
		foreach (Player player in players) {
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireCube(player.transform.position, new Vector3(extendedDistance, 0, extendedDistance));
		}

		// Draw the monsterSpawners activation distance as a sphere
		// If it's active draw it in green, otherwise in red
		foreach (MonsterSpawner spawner in monsterSpawners) {
			if (spawner.IsActive() && !spawner.isDeactivating) {
				Gizmos.color = Color.green;  // Active and not deactivating
			} else if (spawner.IsActive() && spawner.isDeactivating) {
				Gizmos.color = Color.yellow;  // Active but pending deactivation
			} else {
				Gizmos.color = Color.red;  // Fully deactivated
			}

			Gizmos.DrawWireSphere(spawner.transform.position, activationDistance);
		}

		// Draw the quadTree
		if (poiQuadtree != null) {
			poiQuadtree.DrawQuadtree(poiQuadtree);
		}
	}
}
