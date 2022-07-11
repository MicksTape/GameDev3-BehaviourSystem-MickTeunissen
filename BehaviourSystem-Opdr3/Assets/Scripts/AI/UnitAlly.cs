using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Panda;
using UnityEngine.UI;

public class UnitAlly : MonoBehaviour {

	const float minPathUpdateTime = .2f;
	const float pathUpdateMoveThreshold = .5f;
	bool followingPath = true;

	[Header("Movement")]
	[SerializeField] private Transform player;
	private float currentSpeed;
	[SerializeField] private float speed = 8;
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

	[Task]
	private bool playerInRange = false;

	[Task]
	public bool playerInDanger = false;

	[Task]
	private bool behindCover = false;

	CreatePath path;

	private void Start() {
		currentSpeed = speed;
		slashInterval = startTimeSlashInterval;
		defendSprite = statusImage.sprite;

		anim = GetComponent<Animator>();
		anim.SetBool("isRun", true);
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

	[Task]
	private void MoveToPlayer() {
		UpdatePath(player);
		Task.current.Succeed();
	}

	[Task]
	private void MoveToNearestRock() {
		// Find nearest weaponrack transform and move enemy to that position
		UpdatePath(getClosestRock());
		behindCover = true;
		Task.current.Succeed();
	}

	[Task]
	private void ThrowSmoke() {
		Debug.Log("Throwing smoke grenade");
		transform.LookAt(player);

		var smokebomb = Instantiate(smokeBallPrefab, transform.position, Quaternion.identity);
		smokebomb.transform.position = transform.position;
		smokebomb.transform.rotation = transform.rotation;

		playerInDanger = false;
		behindCover = false;
		
		Task.current.Succeed();
	}


	private Transform getClosestRock() {
		// Create instances
		float closestDistance = Mathf.Infinity;
		Transform trans = null;

		// Loop through all weaponrack transforms
		foreach (Transform rTrans in rocksToHideBehind) {
			float currentDistance;
			currentDistance = Vector3.Distance(transform.position, rTrans.transform.position);

			if (currentDistance < closestDistance) {
				closestDistance = currentDistance;
				trans = rTrans.transform;
			}
		}
		return trans;
	}



	[Task]
	private void SlashKatana() {
		if (slashInterval <= 0) {

			slashInterval = startTimeSlashInterval;
		} else {
			slashInterval -= Time.deltaTime;
		}
		Task.current.Succeed();
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
