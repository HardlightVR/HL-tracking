using UnityEngine;
using System.Collections;

namespace NullSpace.SDK.Demos
{
	public class AutoPitcher : MonoBehaviour
	{
		Vector2 pitchFrequency = new Vector2(.40f, 1.6f);
		Vector2 spawnRange = new Vector2(25f, 35f);
		Vector2 pitchSpeed = new Vector2(35f, 60f);
		public bool pitching = false;
		private int counter = 0;
		private int levelUpCounter = 20;
		private int level = 0;

		/// <summary>
		/// Our level-up indicator.
		/// </summary>
		public ParticleSystem EscalateEffect;

		/// <summary>
		/// The different projectiles to shoot
		/// </summary>
		public GameObject[] validProjectiles;

		void Start()
		{
			StartCoroutine(AutoPitch());
		}

		IEnumerator AutoPitch()
		{
			yield return new WaitForSeconds(1);
			pitching = true;
			while (pitching)
			{
				yield return new WaitForSeconds(Random.Range(pitchFrequency.x, pitchFrequency.y));
				Pitch();
			}
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				Pitch(0);
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				Pitch(1);
			}
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				Pitch(2);
			}
			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				Pitch(3);
			}
		}
		/// <summary>
		/// Random range 0-15. Condenses it into an index (0-3) on a weighted hand-set scale.
		/// </summary>
		/// <returns></returns>
		int WeightedRandomProjectileIndex()
		{
			int val = Random.Range(0, 15);
			if (val < 5)
			{
				return 0;
			}
			if (val < 8)
			{
				return 1;
			}
			if (val < 11)
			{
				return 2;
			}
			if (val < 13)
			{
				return 3;
			}
			return 0;
		}

		void Pitch(int index = -1)
		{
			if (index < 0)
			{
				index = WeightedRandomProjectileIndex();

				//Prevent you from getting heavier hitting projectiles.
				index = Mathf.Clamp(index, 0, level);

				//for (int i = 0; i < 100; i++)
				//{
				//	index = WeightedRandomProjectileIndex();
				//	Debug.Log(index + "\n");
				//}
			}

			GameObject go = validProjectiles[Mathf.Clamp(index, 0, validProjectiles.Length - 1)];

			go = Instantiate(go, RequestSpawnPosition(), Quaternion.identity) as GameObject;

			//Shoot the projectile towards the player.
			Rigidbody rb = go.GetComponent<Rigidbody>();

			Vector3 target = HardlightSuit.Find().FindRandomLocation().transform.position;

			if (Random.Range(0, 50) > 40)
			{
				target += Vector3.right * Random.Range(-0.5f, 0.5f);
				target += Vector3.forward * Random.Range(-0.5f, 0.5f);
			}
			Debug.DrawLine(go.transform.position, target, Color.red, 5.0f);
			rb.AddForce((target - go.transform.position) * Random.Range(pitchSpeed.x, pitchSpeed.y) * (1 + index) / 4.0f);

			counter++;

			if (counter > levelUpCounter)
			{
				Escalate();
			}
		}

		//So the experience gets more intense over time?
		void Escalate()
		{
			//Level controls
			level++;
			levelUpCounter += 5;

			//Firing frequency
			pitchFrequency.x = Mathf.Clamp(pitchFrequency.x - .03f, .2f, 5);
			pitchFrequency.y = Mathf.Clamp(pitchFrequency.y - .06f, .5f, 5);

			//Firing speeds (slowly creeps into bigger ranges)
			pitchSpeed.x += 2;
			pitchSpeed.y += 3;

			//Display the level up effect.
			EscalateEffect.Play();
		}

		/// <summary>
		/// Pick a random spawn position. In front of where the player is looking. A bit left or right and varying degrees of up.
		/// </summary>
		/// <returns></returns>
		Vector3 RequestSpawnPosition()
		{
			//Get the VR Camera from the Mimic tools. Save the transform for easier referencing
			Transform camTransform = VRMimic.Instance.VRCamera.transform;

			Vector3 fwd = camTransform.forward;
			fwd.y = 0;
			
			//We never fire from below or behind the player.
			return (fwd * Random.Range(spawnRange.x, spawnRange.y) +
						Random.Range(-20, 20) * camTransform.right +
						Random.Range(3, 25) * camTransform.up);
		}

	}
}