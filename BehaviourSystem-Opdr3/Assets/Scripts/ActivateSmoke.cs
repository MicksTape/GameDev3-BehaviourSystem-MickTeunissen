using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateSmoke : MonoBehaviour {

	private Transform player;
	private UnitAlly unitAlly;

	private bool hasThrownSmoke = false;

	// Start is called before the first frame update
	private void Start() {
		player = GameObject.FindGameObjectWithTag("Player").transform;
		unitAlly = GameObject.FindGameObjectWithTag("Ally").GetComponent<UnitAlly>();
	}

	// Update is called once per frame
	private void Update() {
		if (this.gameObject != null) {
			float playerDistance = Vector3.Distance(transform.position, player.position);

			if (playerDistance <= unitAlly.keepDistanceFromPlayerRadius) {
				// Activate smoke
				if (!hasThrownSmoke) {
					Debug.Log("smoke activated");
					this.gameObject.GetComponent<MeshRenderer>().enabled = false;
					StartCoroutine(ScaleSmoke());
					hasThrownSmoke = true;
					Destroy(this.gameObject, 20.0f);
				}
			}
		}
	}

	// Increase size of smoke
	private IEnumerator ScaleSmoke() {
		Vector3 pos = new Vector3(player.transform.position.x, -0.4f, player.transform.position.z);
		var smokeCloud = Instantiate(unitAlly.smokeCloudPrefab, pos, Quaternion.identity);
		unitAlly.smokeBallSound.Play();

		Vector3 beginScale = new Vector3(0.5f, 0.5f, 0.5f);

		smokeCloud.transform.localScale = beginScale;

		while (smokeCloud.transform.localScale.z < unitAlly.smokeMaxSpread) {
			smokeCloud.transform.localScale += new Vector3(1f, 1f, 1f) / unitAlly.smokeSpreadTime * Time.deltaTime;

			yield return new WaitForEndOfFrame();
		}

		Destroy(smokeCloud, 10.0f); // Destroy after x seconds
	}
}
