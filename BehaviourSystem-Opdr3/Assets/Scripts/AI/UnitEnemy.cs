using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Panda;
using UnityEngine.UI;

public class UnitEnemy : MonoBehaviour {

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
	[SerializeField] private Transform[] checkpoints;
	[SerializeField] private int checkpointCounter = 0;
	private float waitTime;
	[SerializeField] private float checkpointWaitTime = 3.0f;

	[Header("Health")]
	[SerializeField] private Image healthBar;
	[SerializeField] private float currentHealth;
	private float currentHealthValue;
	[SerializeField] private float maxHealth = 200f;
	[SerializeField] private float lerpSpeed = 10f;


	public float CurrentHealth {
		get { return currentHealth; }
		set { currentHealth = Mathf.Clamp(value, 0, maxHealth); }
	}

	[Header("Attack")]
	[SerializeField] private GameObject katana;
	[SerializeField] private Transform[] weaponRacks;

	[SerializeField] private Animator anim;
	[SerializeField] private AudioSource takeHitSound;
	public float slashDamage = 10f;
	private float slashInterval;
	[SerializeField] private float startTimeSlashInterval = 2f;
	public float lookRadius = 25f;
	private float originalLookRadius;
	[SerializeField] private float attackRadius = 20f;

	[SerializeField] private Image statusImage;
	[SerializeField] private Sprite attackSprite;
	private Sprite defendSprite;
	private bool isDead = false;

	[Task]
	private bool isStunned = false;

	[Task]
	private bool playerInRange = false;

	[Task]
	private bool hasKatana = false;

	[Task]
	private bool attackPlayer = false;



	

	CreatePath path;

	private void Start() {
		currentSpeed = speed;
		waitTime = checkpointWaitTime;
		currentHealth = maxHealth;
		slashInterval = startTimeSlashInterval;
		defendSprite = statusImage.sprite;

		anim = GetComponent<Animator>();
		anim.SetBool("isRunning", true);

		// Disable katana
		katana.SetActive(false);

		originalLookRadius = lookRadius;
		//closestWeaponRack = null;
	}

	private void Update() {
		// If inside the lookRadius
		float playerDistance = Vector3.Distance(player.position, transform.position);

		if (playerDistance <= lookRadius) {
			playerInRange = true;
			statusImage.sprite = attackSprite;

			anim.SetBool("isIdle", false);
			anim.SetBool("isRunning", true);
			anim.SetBool("isAttacking", false);
			anim.SetBool("isDeath", false);

			if (playerDistance <= attackRadius) {
				// Attack
				attackPlayer = true;
				anim.SetBool("isIdle", false);
				anim.SetBool("isRunning", false);
				anim.SetBool("isAttacking", true);
				anim.SetBool("isDeath", false);

			} else {
				attackPlayer = false;
				anim.SetBool("isIdle", false);
				anim.SetBool("isRunning", true);
				anim.SetBool("isAttacking", false);
				anim.SetBool("isDeath", false);
			}
		} else {
			playerInRange = false;
			statusImage.sprite = defendSprite;

			anim.SetBool("isIdle", false);
			anim.SetBool("isRunning", true);
			anim.SetBool("isAttacking", false);
			anim.SetBool("isDeath", false);
		}

		// Die
		if (currentHealth <= 0 && !isDead) {
			isDead = true;

			anim.SetBool("isIdle", false);
			anim.SetBool("isRunning", false);
			anim.SetBool("isAttacking", false);
			anim.SetBool("isDeath", true);

			Destroy(gameObject, .5f);

		}

		// Mapping the health bar
		currentHealthValue = Map(CurrentHealth, 0, maxHealth, 0, 1);
		healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, currentHealthValue, Time.deltaTime * lerpSpeed);
	}

	// If the path has been found it will follow the path
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
	private void MoveToCheckpoint() {
		// Play animations
		anim.SetBool("isIdle", false);
		anim.SetBool("isRunning", true);
		anim.SetBool("isAttacking", false);
		anim.SetBool("isDeath", false);

		// Move to checkpoint
		UpdatePath(checkpoints[checkpointCounter]);
		
		// Distance to the checkpoint
		float patrolDistance = Vector3.Distance(checkpoints[checkpointCounter].position, transform.position);

		// If distance is between unit and checkpoint destination is smaller or equal to destination, the unit has reached its destination
		if (patrolDistance <= stoppingDst) {
			Task.current.Succeed();
		}
	}

	[Task]
	private void FindNextCheckpoint() {
		// Play animations
		anim.SetBool("isIdle", true);
		anim.SetBool("isRunning", false);
		anim.SetBool("isAttacking", false);
		anim.SetBool("isDeath", false);

		if (checkpointCounter < checkpoints.Length - 1) {
			checkpointCounter++;
		} else {
			checkpointCounter = 0;
		}
		Task.current.Succeed();
	}

	[Task]
	private void MoveToNearestWeaponRack() {
		lookRadius = 100;

		// Find nearest weaponrack transform and move enemy to that position
		UpdatePath(getClosestWeaponRack());
		Task.current.Succeed();
	}

	[Task]
	private void GrabWeapon() {
		if (hasKatana == false) {
			hasKatana = true;
			katana.SetActive(true);
			lookRadius = originalLookRadius;
			Task.current.Succeed();
		}
	}

	private Transform getClosestWeaponRack() {
		// Create instances
		float closestDistance = Mathf.Infinity;
		Transform trans = null;

		// Loop through all weaponrack transforms
		foreach (Transform wrTrans in weaponRacks) {
			float currentDistance;
			currentDistance = Vector3.Distance(transform.position, wrTrans.transform.position);

			if (currentDistance < closestDistance) {
				closestDistance = currentDistance;
				trans = wrTrans.transform;
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

	[Task]
	private void Stun() {
		// Play animations
		anim.SetBool("isIdle", true);
		anim.SetBool("isRunning", false);
		anim.SetBool("isAttacking", false);
		anim.SetBool("isDeath", false);

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

	// Take damage when hit
	private void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Shuriken")) {
			takeHitSound.Play();
			CurrentHealth -= player.GetComponent<Shooting>().damage;
		} else if (other.CompareTag("Smoke")) {
			StartCoroutine(Stunning());
		}
	}

	// Stun the enemy for 5 seconds
	private IEnumerator Stunning() {
		Debug.Log("STUNNED!");
		isStunned = true;
		yield return new WaitForSeconds(5);
		Debug.Log("normal state");
		isStunned = false;
	}


	private void OnDrawGizmos() {
		if (path != null) {
			path.DrawWithGizmos();
		}
	}

	// Create Gizmos around gameObject in the inspector for the look and attack radius
	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, lookRadius);
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, attackRadius);
	}

	// This method maps a range of numbers into another range
	public float Map(float x, float in_min, float in_max, float out_min, float out_max) {
		return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
	}
}
