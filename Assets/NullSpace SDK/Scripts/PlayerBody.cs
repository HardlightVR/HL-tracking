using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace NullSpace.SDK
{
	public class PlayerBody : MonoBehaviour
	{
		[System.Serializable]
		public class PlayerArm
		{
			//Forearm
			public GameObject forearm;
			//Upper Arm
			public GameObject upperArm;

			//Connected Shoulder
			public GameObject ShoulderConnection;
		}

		[System.Serializable]
		public class PlayerTorso
		{
			#region Body Objects
			//Left
			public GameObject LeftUpArm;
			public GameObject LeftTorsoShoulder;
			public GameObject LeftBack;
			public GameObject LeftChest;
			public GameObject LeftUpAb;
			public GameObject LeftMidAb;
			public GameObject LeftLowAb;

			//Right
			public GameObject RightUpArm;
			public GameObject RightTorsoShoulder;
			public GameObject RightBack;
			public GameObject RightChest;
			public GameObject RightUpAb;
			public GameObject RightMidAb;
			public GameObject RightLowAb;
			#endregion

			public List<GameObject> regions;

			public void Setup()
			{
				regions = new List<GameObject>();

				regions.Add(LeftTorsoShoulder);
				regions.Add(RightTorsoShoulder);
				regions.Add(LeftUpArm);
				regions.Add(RightUpArm);
				regions.Add(LeftBack);
				regions.Add(RightBack);
				regions.Add(LeftChest);
				regions.Add(RightChest);
				regions.Add(LeftUpAb);
				regions.Add(RightUpAb);
				regions.Add(LeftMidAb);
				regions.Add(RightMidAb);
				regions.Add(LeftLowAb);
				regions.Add(RightLowAb);

				SetupDictionary();
			}

			private void SetupDictionary()
			{
				Distances = new Dictionary<GameObject, float>();
				for (int i = 0; i < regions.Count; i++)
				{
					Distances.Add(regions[i], float.MaxValue);
				}
			}

			Dictionary<GameObject, float> Distances;

			public GameObject[] GetMultipleNearestLocations(Vector3 point, int maxImpacted = 1, float maxDistance = 5.0f)
			{
				if (Distances == null)
				{
					//Set up the dictionary if we haven't. This is so we don't have to create a dictionary each frame.
					SetupDictionary();
				}

				List<GameObject> hit = new List<GameObject>();

				List<GameObject> closestObjects = new List<GameObject>();

				//This could possibly be more efficient. Linq is easy.
				var sortedList = from pair in Distances
								 orderby pair.Value ascending
								 select pair;

				float closestDist = 1000;

				//For all the regions
				for (int i = 0; i < regions.Count; i++)
				{
					//Calculate the V3 distance to the point.
					float newDist = Vector3.Distance(point, regions[i].transform.position);
					Distances[regions[i]] = newDist;
				}

				foreach (KeyValuePair<GameObject, float> item in sortedList)
				{
					bool wantMore = hit.Count < maxImpacted;
					bool withinDistance = item.Value < maxDistance;

					//Don't add more elements than requested
					if (wantMore && withinDistance)
					{
						//Don't add the same element more than once. I think we could eschew this more expensive check.
						if (!hit.Contains(item.Key))
						{
							hit.Add(item.Key);
						}
					}
				}

				//Look through all the objects. Find the closest N values closest.
				for (int i = 0; i < regions.Count; i++)
				{
					float newDist = Vector3.Distance(point, regions[i].transform.position);

					if (newDist < closestDist)
					{
						closestObjects.Add(regions[i]);
					}
				}

				//Debug.Log(hit.Count + "\n");
				return hit.ToArray();
			}

			public GameObject GetNearestLocation(Vector3 point, float maxDistance = 5.0f)
			{
				GameObject closest = null;
				float closestDist = 1000;

				//Look through all the objects. Check which is closest.
				for (int i = 0; i < regions.Count; i++)
				{
					float newDist = Vector3.Distance(point, regions[i].transform.position);

					if (newDist < closestDist && newDist < maxDistance)
					{
						closest = regions[i];
						closestDist = newDist;
					}
				}
				//Debug.Log("Closest: " + closest.name + "\n");

				return closest;
			}

			/// <summary>
			/// Picks a random body gameobject. This means you can use the position or get the HapticLocation.
			/// </summary>
			/// <returns></returns>
			public GameObject GetRandomBodyPosition()
			{
				GameObject randomBodyPart = regions[Random.Range(0, regions.Count)];
				//Debug.Log("Random Body Part Selected: " + randomBodyPart.name + " \n");
				return randomBodyPart;
			}

			/// <summary>
			/// Get the position of a random body.
			/// </summary>
			/// <returns></returns>
			public Vector3 GetRandomLocation()
			{
				return GetRandomBodyPosition().transform.position;
			}
			public GameObject GetRandomLocationObject()
			{
				int index = Random.Range(0, regions.Count);

				//This is not possible.
				//if (index > regions.Count)
				//{
				//}
				//else
				if (regions[index] == null)
				{
					Debug.LogError("Attempted to get Random Location inside PlayerBody's PlayerTorso.\n\tNo locations should be null. Check to make sure the fields in the Body Mimic prefab were assigned.");
				}
				else
				{
					return regions[index];
				}
				return null;
			}

			public void SetAllVisibility(bool revealed)
			{
				if (regions != null)
				{
					return;
				}

				for (int i = 0; i < regions.Count; i++)
				{
					SetVisiblity(revealed, regions[i]);
				}
			}
			private void SetVisiblity(bool revealed, GameObject region)
			{
				if (region != null)
				{
					MeshRenderer rend = region.GetComponent<MeshRenderer>();
					if (rend != null)
					{
						rend.enabled = revealed;
					}
				}
			}
		}

		[SerializeField]
		public PlayerTorso playerTorso;
		//[SerializeField]
		//public PlayerArm LeftArm;
		//[SerializeField]
		//public PlayerArm RightArm;

		//public enum BodyLocation { Front, BackLeft, BackRight, LeftSide, RightSide }
		//public BodyLocation myLocation = BodyLocation.Front;
		/// <summary>
		/// This gets an existing PlayerBody in the scene. If none exists, it will attempt to Initialize the BodyMimic functionality.
		/// 
		/// It is suggested to call HideLayer(NSManager.HAPTIC_LAYER) on your game's cameras (to hide the visual colliders)
		/// </summary>
		/// <returns></returns>
		public static PlayerBody Find()
		{
			PlayerBody body = FindObjectOfType<PlayerBody>();
			if (body != null)
			{
				return body;
			}
			BodyMimic.Initialize();

			body = FindObjectOfType<PlayerBody>();
			if (body != null)
			{
				return body;
			}
			return null;
		}

		public void Start()
		{
			playerTorso.Setup();
		}

		/// <summary>
		/// Looks for a haptic file - "Haptics/" + file
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public HapticSequence GetSequence(string file)
		{
			HapticSequence seq = new HapticSequence();
			seq.LoadFromAsset("Haptics/" + file);
			return seq;
		}

		public void Hit(Vector3 point, string file, float maxDistance = 5.0f)
		{
			AreaFlag loc = FindNearestFlag(point, maxDistance);
			if (loc != AreaFlag.None)
			{
				HapticSequence seq = GetSequence(file);
				seq.CreateHandle(loc).Play();
			}
			else
			{
				Debug.Log("None\n");
			}
		}

		/// <summary>
		/// Calls ImpulseGenerator.BeginEmanatingEffect with the given sequence and depth.
		/// </summary>
		/// <param name="loc">The location to start the emanation.</param>
		/// <param name="seq">The sequence to use.</param>
		/// <param name="depth">How many steps you want the emanation to take.</param>
		/// <param name="duration">How long the entire impulse should take</param>
		public void EmanatingHit(AreaFlag loc, HapticSequence seq, float duration = .75f, int depth = 2)
		{
			ImpulseGenerator.BeginEmanatingEffect(loc, depth).WithDuration(duration).Play(seq);
		}

		/// <summary>
		/// Calls ImpulseGenerator.BeginTraversingImpulse with the given sequence.
		/// </summary>
		/// <param name="startLocation">The origin of the traversing impulse.</param>
		/// <param name="endLocation">The destination of the traversing impulse.</param>
		/// <param name="seq">The sequence to use.</param>
		/// <param name="duration">How long the entire impulse should take</param>
		public void TraversingHit(AreaFlag startLocation, AreaFlag endLocation, HapticSequence seq, float duration = .75f)
		{
			ImpulseGenerator.BeginTraversingImpulse(startLocation, endLocation).WithDuration(duration).Play(seq);
		}

		/// <summary>
		/// Begins an emanation at the nearest flag from the point.
		/// </summary>
		/// <param name="point">A point near the player's body</param>
		/// <param name="eff">The effect to use in the emanation.</param>
		/// <param name="maxDistance">Will not return locations further than the max distance.</param>
		/// <param name="depth">The depth of the emanating impulse</param>
		public void HitImpulse(Vector3 point, Effect eff = Effect.Pulse, float maxDistance = 5.0f, int depth = 1)
		{
			AreaFlag loc = FindNearestFlag(point, maxDistance);
			if (loc != AreaFlag.None)
			{
				ImpulseGenerator.BeginEmanatingEffect(loc, depth).WithEffect(Effect.Pulse, .2f).WithDuration(.5f).Play();
			}
			else
			{
				Debug.LogWarning("Invalid Hit at " + point + "\n");
			}
		}

		/// <summary>
		/// Finds the nearest HapticLocation.MyLocation on the PlayerTorso to the provided point
		/// </summary>
		/// <param name="point">The world space to compare to the PlayerTorso body.</param>
		/// <param name="maxDistance">Disregard body parts less than the given distance</param>
		/// <returns>Defaults to AreaFlag.None if no areas are within range.</returns>
		public AreaFlag FindNearestFlag(Vector3 point, float maxDistance = 5.0f)
		{
			//Maybe get a list of nearby regions?
			GameObject closest = playerTorso.GetNearestLocation(point, maxDistance);

			//Debug.Log("closest: " + closest.name + "\n");
			if (closest != null && closest.GetComponent<HapticLocation>() != null)
			{
				return closest.GetComponent<HapticLocation>().Where;
			}
			Debug.LogError("Could not find the closest pad. Returning an empty location\n" + closest.name);
			return AreaFlag.None;
		}

		/// <summary>
		/// Finds a Haptic Location near the source point.
		/// </summary>
		/// <param name="point">A point near the player's body</param>
		/// <returns>Returns null if no location is within the max distance</returns>
		public HapticLocation FindNearbyLocation(Vector3 point, float maxDistance = 5.0f)
		{
			//Maybe get a list of nearby regions?
			GameObject[] closest = playerTorso.GetMultipleNearestLocations(point, 1, maxDistance);

			//Debug.Log("Find Nearby: " + closest.Length + "\n");
			for (int i = 0; i < closest.Length; i++)
			{
				HapticLocation loc = closest[i].GetComponent<HapticLocation>();
				//Debug.DrawLine(source, loc.transform.position, Color.green, 15.0f);
				if (closest[i] != null && loc != null)
				{
					Debug.DrawLine(point, loc.transform.position, Color.red, 15.0f);
					return loc;
				}
			}
			return null;
		}
		/// <summary>
		/// Finds a Haptic Location near the source point. Only returns a HapticLocation that we have valid line of sight to the object.
		/// </summary>
		/// <param name="point"></param>
		/// <param name="requireLineOfSight"></param>
		/// <param name="hitLayers"></param>
		/// <returns>A single HapticLocation that is close to the point and within line of sight of it. Defaults to null if nothing within MaxDistance is within range.</returns>
		public HapticLocation FindNearbyLocation(Vector3 point, bool requireLineOfSight, LayerMask hitLayers, float maxDistance = 5.0f)
		{
			GameObject[] closest = playerTorso.GetMultipleNearestLocations(point, 16, maxDistance);

			//Debug.Log("Find Nearby: " + closest.Length + "\n");
			for (int i = 0; i < closest.Length; i++)
			{
				HapticLocation loc = closest[i].GetComponent<HapticLocation>();
				Debug.DrawLine(point, loc.transform.position, Color.green, 15.0f);
				if (closest[i] != null && loc != null)
				{
					RaycastHit hit;
					float dist = Vector3.Distance(point, loc.transform.position);
					if (Physics.Raycast(point, loc.transform.position - point, out hit, dist, hitLayers))
					{
						Debug.Log("Hit: " + hit.collider.name + "\n" + hit.collider.gameObject.layer + "\n");
						Debug.DrawLine(point, hit.point, Color.red, 15.0f);
					}
					else
					{
						Debug.DrawLine(point, loc.transform.position, Color.green, 15.0f);
						return loc;
					}
				}
			}
			return null;
		}
		/// <summary>
		/// Gets a random HapticLocation on the configured PlayerTorso.
		/// </summary>
		/// <returns>A valid HapticLocation on the body (defaults to null if none are configured or if it is configured incorrectly.</returns>
		public HapticLocation FindRandomLocation()
		{
			HapticLocation loc = playerTorso.GetRandomLocationObject().GetComponent<HapticLocation>();
			if (loc != null)
				return loc;
			else
				Debug.LogError("Failed to complete PlayerBody.FindRandomLocation(). The returned object did not have a HapticLocation component\n");
			return null;
		}

		/// <summary>
		/// A coroutine for repeating an emanation on a delay X times.
		/// </summary>
		/// <param name="impulse"></param>
		/// <param name="delay"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public IEnumerator RepeatedEmanations(ImpulseGenerator.Impulse impulse, float delay, int count)
		{
			impulse.Play();
			for (int i = 0; i < count - 1; i++)
			{
				yield return new WaitForSeconds(delay);
				impulse.Play();
			}
		}

		/// <summary>
		/// A coroutine for playing an impulse AFTER a float delay.
		/// </summary>
		/// <param name="impulse"></param>
		/// <param name="delay"></param>
		/// <returns></returns>
		IEnumerator DelayEmanation(ImpulseGenerator.Impulse impulse, float delay)
		{
			yield return new WaitForSeconds(delay);
			impulse.Play();
		}
	}
}