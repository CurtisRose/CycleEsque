using System.Collections.Generic;
using UnityEngine;

public class Quadtree {
	private int MAX_OBJECTS = 0;
	private int MAX_LEVELS = 5;
	private int level;
	private List<GameObject> objects;
	private Rect bounds;
	private Quadtree[] nodes;

	// Start constructor to set max objects and max levels
	public Quadtree(int pLevel, Rect pBounds, int MAX_OBJECTS, int MAX_LEVELS) {
		level = pLevel;
		objects = new List<GameObject>();
		bounds = pBounds;
		nodes = new Quadtree[4];
		this.MAX_OBJECTS = MAX_OBJECTS;
		this.MAX_LEVELS = MAX_LEVELS;
	}
	
	// Recursive constructor
	public Quadtree(int pLevel, Rect pBounds) {
		level = pLevel;
		objects = new List<GameObject>();
		bounds = pBounds;
		nodes = new Quadtree[4];
	}

	public void Clear() {
		objects.Clear();

		for (int i = 0; i < nodes.Length; i++) {
			if (nodes[i] != null) {
				nodes[i].Clear();
				nodes[i] = null;
			}
		}
	}

	private void Split() {
		float subWidth = (bounds.width / 2);
		float subHeight = (bounds.height / 2);
		float x = bounds.x;
		float y = bounds.y;

		nodes[0] = new Quadtree(level + 1, new Rect(x + subWidth, y, subWidth, subHeight));
		nodes[1] = new Quadtree(level + 1, new Rect(x, y, subWidth, subHeight));
		nodes[2] = new Quadtree(level + 1, new Rect(x, y + subHeight, subWidth, subHeight));
		nodes[3] = new Quadtree(level + 1, new Rect(x + subWidth, y + subHeight, subWidth, subHeight));
	}

	public void Insert(GameObject gameObject) {
		Vector2 pos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);

		if (!bounds.Contains(pos)) {
			return; // Object is outside the bounds of this quadtree node
		}

		if (nodes[0] != null) {
			int index = GetIndex(pos);
			if (index != -1) {
				nodes[index].Insert(gameObject);
				return;
			}
		}

		objects.Add(gameObject);

		if (objects.Count > MAX_OBJECTS && level < MAX_LEVELS) {
			if (nodes[0] == null) {
				Split();
			}

			int i = 0;
			while (i < objects.Count) {
				int index = GetIndex(new Vector2(objects[i].transform.position.x, objects[i].transform.position.z));
				if (index != -1) {
					GameObject obj = objects[i];
					objects.RemoveAt(i);
					nodes[index].Insert(obj);
				} else {
					i++;
				}
			}
		}
	}

	public bool Remove(GameObject gameObject) {
		if (!bounds.Contains(new Vector2(gameObject.transform.position.x, gameObject.transform.position.z))) {
			return false; // Object is not within the bounds of this node
		}

		// Try to remove the object from this node's objects list
		if (objects.Remove(gameObject)) {
			return true;
		}

		// If the object is not in the objects list, search in the child nodes if they exist
		if (nodes[0] != null) {
			for (int i = 0; i < nodes.Length; i++) {
				if (nodes[i].Remove(gameObject)) {
					return true;
				}
			}
		}

		// If the object was not found in this node or any children, return false
		return false;
	}

	private int GetIndex(Vector2 pos) {
		bool left = pos.x < bounds.x + bounds.width / 2;
		bool top = pos.y < bounds.y + bounds.height / 2;

		if (left) {
			if (top) {
				return 1; // Top-left
			} else {
				return 2; // Bottom-left
			}
		} else {
			if (top) {
				return 0; // Top-right
			} else {
				return 3; // Bottom-right
			}
		}
	}

	public List<GameObject> Query(Rect range, List<GameObject> found = null) {
		if (found == null) {
			found = new List<GameObject>();
		}

		if (!bounds.Overlaps(range)) {
			return found;
		}

		for (int i = 0; i < objects.Count; i++) {
			if (range.Contains(new Vector2(objects[i].transform.position.x, objects[i].transform.position.z))) {
				found.Add(objects[i]);
			}
		}

		if (nodes[0] != null) {
			for (int i = 0; i < 4; i++) {
				nodes[i].Query(range, found);
			}
		}

		return found;
	}
	public void DrawQuadtree(Quadtree node) {
		if (node != null) {
			Gizmos.color = Color.green; // Use a distinctive color to make it stand out
										// Draw the rectangular bounds of the node
			Gizmos.DrawWireCube(new Vector3(node.bounds.center.x, 0, node.bounds.center.y),
								new Vector3(node.bounds.size.x, 1, node.bounds.size.y));

			// Recursively draw children
			if (node.nodes[0] != null) {
				foreach (var subNode in node.nodes) {
					DrawQuadtree(subNode);
				}
			}
		}
	}
}
