using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.AI;

public class UnitEnemy : MonoBehaviour {

	const float minPathUpdateTime = .2f;
	const float pathUpdateMoveThreshold = .5f;
	bool followingPath = true;
	private BTNode tree;

	[Header("Movement")]
	[SerializeField] private Transform player;
	private float currentSpeed;
	[SerializeField] private float speed = 8;
	[SerializeField] private float turnSpeed = 3;
	[SerializeField] private float turnDst = 5;
	[SerializeField] private float stoppingDst = 10;
	[SerializeField] private Transform[] checkpoints;
	[SerializeField] private int checkpointCounter;

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
	[SerializeField] private GameObject katanaPrefab;
	[SerializeField] private Transform[] weapons;

	[SerializeField] private Animator anim;
	[SerializeField] private AudioSource takeHitSound;
	public float slashDamage = 30f;
	private float slashInterval;
	[SerializeField] private float startTimeSlashInterval = 2f;
	public float lookRadius = 10f;
	[SerializeField] private float attackRadius = 5f;

	[SerializeField] private Image statusImage;
	[SerializeField] private Sprite attackSprite;
	private Sprite defendSprite;
	private bool isDead = false;
	private Transform guard;

	public bool hasArrivedAtCheckpoint = false;
	public bool hasArrivedAtWeaponRack = false;
	public bool playerInRange = false;
	public bool playerInAttackRange = false;
	public bool hasWeapon = false;

	CreatePath path;

	private void Awake() {
		guard = this.gameObject.transform;
	}

	private void Start() {
		currentSpeed = speed;
		currentHealth = maxHealth;
		slashInterval = startTimeSlashInterval;
		defendSprite = statusImage.sprite;

		anim = GetComponent<Animator>();
		anim.SetBool("isRunning", true);

		// Disable katana
		katana.SetActive(false);

		tree =
			 new BTSelector(
				 // Chase Player
				 new BTSequence(
					 new BTCheckPlayerInRange(guard, player),
					 new BTMoveToNearestWeaponRack(guard, player),
					 new BTWait(1.0f),
					 new BTGrabWeapon(guard),
					 new BTChasePlayer(guard, player, katanaPrefab)
				 ),
				 // Patrol
				 new BTSequence(
					 new BTMoveToCheckpoint(player, guard, checkpoints),
					 new BTWait(1.0f),
					 new BTFindNextCheckpoint(player, guard, checkpoints)
				 )
			 );
	}

	private void FixedUpdate() {
		tree?.Run();
	}

	private void Update() {
		// If inside the lookRadius
		float playerDistance = Vector3.Distance(player.position, transform.position);

		if (playerDistance <= lookRadius) {
			//playerInRange = true;
			statusImage.sprite = attackSprite;

			anim.SetBool("isIdle", false);
			anim.SetBool("isRunning", true);
			anim.SetBool("isAttacking", false);
			anim.SetBool("isDeath", false);

			if (playerDistance <= attackRadius) {
				// Attack
				//attackPlayer = true;
				anim.SetBool("isIdle", false);
				anim.SetBool("isRunning", false);
				anim.SetBool("isAttacking", true);
				anim.SetBool("isDeath", false);

			} else {
				//attackPlayer = false;
				anim.SetBool("isIdle", false);
				anim.SetBool("isRunning", true);
				anim.SetBool("isAttacking", false);
				anim.SetBool("isDeath", false);
			}
		} else {
			//playerInRange = false;
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
			Debug.Log(debugMessage);
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

	//Action node
	public class BTMoveToCheckpoint : BTNode {

		private Transform guard;
		private Transform player;
		private Transform[] checkpoints;
		private UnitEnemy guardScript;

		public BTMoveToCheckpoint(Transform _player, Transform _guard, Transform[] _checkpoints) {
			player = _player;
			guard = _guard;
			checkpoints = _checkpoints;
			guardScript = _guard.GetComponent<UnitEnemy>();
		}

		public override BTResult Run() {
			// Distance to player
			float distanceToPlayer = Vector3.Distance(player.position, guard.position);
			if (distanceToPlayer <= guardScript.lookRadius) {
				guardScript.playerInRange = true;
			}

			if (guardScript.playerInRange) {
				return BTResult.Failed;
			}

			// Move to checkpoint
			guardScript.UpdatePath(checkpoints[guardScript.checkpointCounter]);

			// Distance to the checkpoint
			float patrolDistance = Vector3.Distance(checkpoints[guardScript.checkpointCounter].position, guard.position);

			// If distance is between unit and checkpoint destination is smaller or equal to destination, the unit has reached its destination
			if (patrolDistance <= guardScript.stoppingDst) {
				guardScript.hasArrivedAtCheckpoint = true;
				Debug.Log(guardScript.checkpointCounter);
				return BTResult.Success;
			}

			return BTResult.Running;
		}
	}

	public class BTFindNextCheckpoint : BTNode {

		private Transform guard;
		private Transform player;
		private Transform[] checkpoints;
		private UnitEnemy guardScript;

		public BTFindNextCheckpoint(Transform _player, Transform _guard, Transform[] _checkpoints) {
			player = _player;
			guard = _guard;
			checkpoints = _checkpoints;
			guardScript = _guard.GetComponent<UnitEnemy>();
		}

		public override BTResult Run() {
			if (guardScript.hasArrivedAtCheckpoint) {
				guardScript.hasArrivedAtCheckpoint = false;
				guardScript.checkpointCounter++;

				if (guardScript.checkpointCounter >= checkpoints.Length) {
					guardScript.checkpointCounter = 0;
				}
			}
			return BTResult.Success;
		}
	}

	//Condition node
	public class BTCheckPlayerInRange : BTNode {

		private Transform player;
		private Transform guard;
		private UnitEnemy guardScript;

		public BTCheckPlayerInRange(Transform _guard, Transform _player) {
			guard = _guard;
			player = _player;
			guardScript = guard.GetComponent<UnitEnemy>();
		}

		public override BTResult Run() {
			// Distance to player
			float distanceToPlayer = Vector3.Distance(player.position, guard.position);

			// Player is out of range
			if (distanceToPlayer <= guardScript.lookRadius) {
				guardScript.playerInRange = true;
				//Debug.LogWarning("In range!");
				return BTResult.Success;
			} else {
				guardScript.playerInRange = false;
				//Debug.LogWarning("Not in range!");
				return BTResult.Failed;
			}
		}
	}

	//Action node
	public class BTMoveToNearestWeaponRack : BTNode {

		private Transform guard;
		private Transform player;
		private UnitEnemy guardScript;

		public BTMoveToNearestWeaponRack(Transform _guard, Transform _player) {
			guard = _guard;
			player = _player;
			guardScript = guard.GetComponent<UnitEnemy>();
		}

		public override BTResult Run() {
			// Check if ally is behind cover, else find cover
			Transform nearestRock = GetNearestRock();
			guardScript.UpdatePath(nearestRock);
			//guardScript.speed = 12;

			// Distance to the nearest weapon rack
			float rockDistance = Vector3.Distance(nearestRock.position, guard.position);

			// If the guard is close enough to the weapon, take it
			if (rockDistance <= guardScript.stoppingDst / 3) {
				guardScript.hasArrivedAtWeaponRack = true;
				Debug.LogWarning("got weapon!");
				return BTResult.Success;
			}
			return BTResult.Running;
		}

		// Get the nearest rock
		private Transform GetNearestRock() {
			Transform nearestRock = null;
			float nearestDistance = Mathf.Infinity;

			foreach (Transform rock in guardScript.weapons) {
				float distance = Vector3.Distance(rock.transform.position, guard.position);
				if (distance <= nearestDistance) {
					nearestRock = rock.transform;
					nearestDistance = distance;
				}
			}

			return nearestRock;
		}
	}

	//Action node
	public class BTGrabWeapon : BTNode {

		private Transform guard;
		private UnitEnemy guardScript;

		public BTGrabWeapon(Transform _guard) {
			guard = _guard;
			guardScript = guard.GetComponent<UnitEnemy>();
		}

		public override BTResult Run() {
			guardScript.katana.SetActive(true);
			guardScript.hasWeapon = true;
			return BTResult.Success;
		}
	}

	//Action node
	public class BTChasePlayer : BTNode {

		private Transform player;
		private Transform guard;
		private UnitEnemy guardScript;
		private GameObject katanaPrefab;

		private float slashInterval = 2f;
		private float lastAttackTime;

		public BTChasePlayer(Transform _guard, Transform _player, GameObject _katanaPrefab) {
			guard = _guard;
			player = _player;
			katanaPrefab = _katanaPrefab;
			guardScript = guard.GetComponent<UnitEnemy>();
		}

		public override BTResult Run() {
			// Distance to player
			float distanceToPlayer = Vector3.Distance(player.position, guard.position);
			if (distanceToPlayer >= guardScript.lookRadius) {
				guardScript.playerInRange = false;

				// Drop katana
				guardScript.katana.SetActive(false);
				var katana = Instantiate(katanaPrefab, guard.position, Quaternion.identity);

				Vector3 katanaPosition = guard.position + new Vector3(0f, 0.1f, 0f);
				katana.transform.position = katanaPosition;

				Quaternion katanaRotation = Quaternion.Euler(
					90f, // Random X axis rotation
					guard.rotation.eulerAngles.y, // Use the Y axis rotation of the guard Transform
					Random.Range(0f, 360f) // Random Z axis rotation
				);

				katana.transform.rotation = katanaRotation;

				return BTResult.Failed;
			}

			// If player is very close, attack with SlashKatana
			if (distanceToPlayer <= guardScript.stoppingDst / 4) {
				if (Time.time - lastAttackTime > slashInterval) {
					Debug.Log("Attacking");
					lastAttackTime = Time.time;
				}
			}

			guardScript.UpdatePath(player);
			return BTResult.Running;
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
		//isStunned = true;
		yield return new WaitForSeconds(5);
		Debug.Log("normal state");
		//isStunned = false;
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
