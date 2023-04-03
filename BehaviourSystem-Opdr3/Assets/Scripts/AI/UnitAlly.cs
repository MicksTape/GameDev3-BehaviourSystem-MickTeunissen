using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UnitAlly : MonoBehaviour {

	const float minPathUpdateTime = .2f;
	const float pathUpdateMoveThreshold = .5f;
	bool followingPath = true;
	private BTNode tree;

	[Header("Movement")]
	[SerializeField] private Transform player;
	private float currentSpeed;
	[SerializeField] private float speed = 4;
	[SerializeField] private float turnSpeed = 3;
	[SerializeField] private float turnDst = 5;
	[SerializeField] private float stoppingDst = 10;


	[Header("Protect")]
	public float keepDistanceFromPlayerRadius = 15f;
	[SerializeField] private Transform[] rocksToHideBehind;
	[SerializeField] private Transform firePoint;
	[SerializeField] private GameObject smokeBallPrefab;
	public GameObject smokeCloudPrefab;
	public AudioSource smokeBallSound;
	public float smokeRadius = 2f;
	public float smokeSpreadTime = 1f;
	public float smokeMaxSpread = 5f;

	[SerializeField] private Animator anim;

	public float slashDamage = 10f;
	private float slashInterval;
	[SerializeField] private float startTimeSlashInterval = 2f;

	[SerializeField] private Image statusImage;
	[SerializeField] private Sprite attackSprite;
	private Sprite defendSprite;
	private Transform ally;

	private bool playerInRange = false;
	public bool playerInDanger = false;
	public bool behindCover = false;

	CreatePath path;

	private void Awake() {
		ally = this.gameObject.transform;
	}

	private void Start() {
		speed = 4;
		slashInterval = startTimeSlashInterval;
		defendSprite = statusImage.sprite;

		anim = GetComponent<Animator>();
		anim.SetBool("isRun", true);

		tree =
			 new BTSelector(
				 // Help player
				 new BTSequence(
					new BTCheckPlayerInDanger(ally),
					new BTFindNearestRock(ally),
					new BTWait(2.0f), // Hide
					new BTDebug("Throw smoke!"),
					new BTThrowSmoke(ally, player, smokeBallPrefab)
				 ),
				 // Follow player
				 new BTGuardPlayer(ally, player)
			 );
	}

	private void FixedUpdate() {
		tree?.Run();
	}

	private void Update() {
		// If inside the lookRadius
		float playerDistance = Vector3.Distance(transform.position, player.position);

		if (playerDistance >= keepDistanceFromPlayerRadius) {
			playerInRange = true;
			statusImage.sprite = attackSprite;

			anim.SetBool("isIdle", false);
			anim.SetBool("isRun", true);


		} else {
			playerInRange = false;
			statusImage.sprite = defendSprite;

			anim.SetBool("isIdle", false);
			anim.SetBool("isRun", true);

		}

	}

	// If the path has been found, follow the path
	public void OnPathFound(Vector3[] waypoints, bool pathSuccessful) {
		if (pathSuccessful) {
			path = new CreatePath(waypoints, transform.position, turnDst, stoppingDst);

			StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");

		}
	}

	// Update path to target position
	private void UpdatePath(Transform _target) {
		PathRequestManager.RequestPath(new PathRequest(transform.position, _target.position, OnPathFound));
	}

	// Function that calculates the path and follows it
	IEnumerator FollowPath() {

		int pathIndex = 0;

		// Smooth lookAt
		if (path.lookPoints.Length > 1) {
			transform.LookAt(path.lookPoints[1]);
		} else {
			transform.LookAt(path.lookPoints[0]);
		}

		float speedPercent = 1;

		while (followingPath) {
			Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
			while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D)) {
				if (pathIndex == path.finishLineIndex) {
					followingPath = false;
					break;
				} else {
					pathIndex++;
				}
			}

			if (followingPath) {

				if (pathIndex >= path.slowDownIndex && stoppingDst > 0) {
					speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDst);
					if (speedPercent < 0.01f) {
						followingPath = false;
					}
				}

				Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
				transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
				transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
			}

			yield return null;

		}
	}

	//Main node
	public abstract class BTNode {
		public enum BTResult { Success, Failed, Running }
		public abstract BTResult Run();
		private bool isInitialized = false;

		public BTResult OnUpdate() {
			if (!isInitialized) {
				OnEnter();
				isInitialized = true;
			}
			BTResult result = Run();
			if (result != BTResult.Running) {
				OnExit();
				isInitialized = false;
			}
			return result;
		}

		public virtual void OnEnter() { }
		public virtual void OnExit() { }
	}

	//Action node
	public class BTWait : BTNode {
		private float waitTime;
		private float currentTime;
		public BTWait(float _waitTime) {
			waitTime = _waitTime;
		}

		public override BTResult Run() {
			currentTime += Time.deltaTime;
			if (currentTime >= waitTime) {
				currentTime = 0;
				return BTResult.Success;
			}
			return BTResult.Running;

		}
	}

	//Action node
	public class BTDebug : BTNode {
		private string debugMessage;
		public BTDebug(string _debugMessage) {
			debugMessage = _debugMessage;
		}

		public override BTResult Run() {
			Debug.LogWarning(debugMessage);
			return BTResult.Success;
		}
	}

	//Composite node
	public class BTSequence : BTNode {
		private BTNode[] children;
		private int currentIndex = 0;

		public BTSequence(params BTNode[] _children) {
			children = _children;
		}

		public override BTResult Run() {
			for (; currentIndex < children.Length; currentIndex++) {
				BTResult result = children[currentIndex].OnUpdate();
				switch (result) {
					case BTResult.Failed:
						currentIndex = 0;
						return BTResult.Failed;
					case BTResult.Running:
						return BTResult.Running;
					case BTResult.Success: break;
				}
			}
			currentIndex = 0;
			return BTResult.Success;
		}
	}

	//Composite node
	public class BTSelector : BTNode {
		private BTNode[] children;
		private int currentIndex = 0;

		public BTSelector(params BTNode[] _children) {
			children = _children;
		}

		public override BTResult Run() {
			for (; currentIndex < children.Length; currentIndex++) {
				BTResult result = children[currentIndex].OnUpdate();
				switch (result) {
					case BTResult.Failed: break;
					case BTResult.Running:
						return BTResult.Running;
					case BTResult.Success:
						currentIndex = 0;
						return BTResult.Success;
				}
			}
			currentIndex = 0;
			return BTResult.Failed;
		}
	}

	//Condition node
	public class BTCheckPlayerInDanger : BTNode {

		private Transform ally;
		private UnitAlly allyScript;

		public BTCheckPlayerInDanger(Transform _ally) {
			ally = _ally;
			allyScript = ally.GetComponent<UnitAlly>();
		}

		public override BTResult Run() {
			// If player gets hit by a blade from the enemy
			if (allyScript.playerInDanger) {
				//Debug.LogWarning("Player in danger!");
				allyScript.playerInDanger = false;
				return BTResult.Success;
			} else {
				//Debug.LogWarning("Player not in danger!");
				return BTResult.Failed;
			}
		}
	}

	//Action node
	public class BTFindNearestRock : BTNode {

		private Transform ally;
		private UnitAlly allyScript;

		public BTFindNearestRock(Transform _ally) {
			ally = _ally;
			allyScript = ally.GetComponent<UnitAlly>();
		}

		public override BTResult Run() {
			// Check if ally is behind cover, else find cover
			if (!allyScript.behindCover) {
				Transform nearestRock = GetNearestRock();
				allyScript.UpdatePath(nearestRock);
				allyScript.speed = 12;

				// Distance to the nearest rock
				float rockDistance = Vector3.Distance(nearestRock.position, ally.position);

				// If the guard is close enough to the weapon, take it
				if (rockDistance <= allyScript.stoppingDst / 4) {
					allyScript.behindCover = true;
					allyScript.speed = 4;
				}
				return BTResult.Running;
			} else {
				allyScript.behindCover = false;
				return BTResult.Success;
			}
		}

		// Get the nearest rock
		private Transform GetNearestRock() {
			Transform nearestRock = null;
			float nearestDistance = Mathf.Infinity;

			foreach (Transform rock in allyScript.rocksToHideBehind) {
				float distance = Vector3.Distance(rock.transform.position, ally.position);
				if (distance <= nearestDistance) {
					nearestRock = rock.transform;
					nearestDistance = distance;
				}
			}

			return nearestRock;
		}
	}

	//Action node
	public class BTThrowSmoke : BTNode {

		private Transform ally;
		private Transform player;
		private UnitAlly allyScript;
		private GameObject smokeBallPrefab;

		public BTThrowSmoke(Transform _ally, Transform _player, GameObject _smokeBallPrefab) {
			ally = _ally;
			player = _player;
			allyScript = ally.GetComponent<UnitAlly>();
			smokeBallPrefab = _smokeBallPrefab;
		}

		public override BTResult Run() {
			ally.LookAt(player);
			var smokebomb = Instantiate(smokeBallPrefab, player.position, Quaternion.identity);
			smokebomb.transform.position = player.position;
			smokebomb.transform.rotation = player.rotation;
			return BTResult.Success;
		}
	}

	//Action node
	public class BTGuardPlayer : BTNode {

		private Transform ally;
		private Transform player;
		private UnitAlly allyScript;

		public BTGuardPlayer(Transform _ally, Transform _player) {
			ally = _ally;
			player = _player;
			allyScript = _ally.GetComponent<UnitAlly>();
		}

		public override BTResult Run() {
			allyScript.UpdatePath(player);
			allyScript.speed = 4;
			return BTResult.Success;
		}
	}

	private void OnDrawGizmos() {
		if (path != null) {
			path.DrawWithGizmos();
		}
	}

	// Draws Gizmos around gameObject in the inspector for the look and attack radius
	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, keepDistanceFromPlayerRadius);
	}

	// This method maps the range of numbers into another range
	public float Map(float x, float in_min, float in_max, float out_min, float out_max) {
		return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
	}
}
