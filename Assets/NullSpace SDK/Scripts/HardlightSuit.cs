using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace NullSpace.SDK.Demos
{
	/// <summary>
	/// This is a UnityComponent that contains a single SuitDefinition.
	/// A SuitDefinition is comprirsed of a list of Suit Node holders and the corresponding AreaFlags that each node represents.
	/// </summary>
	public class HardlightSuit : MonoBehaviour
	{
		[SerializeField]
		public SuitDefinition _definition;
		[SerializeField]
		public SuitDefinition Definition
		{
			set { _definition = value; }
			get
			{
				if (_definition == null)
				{
					_definition = ScriptableObject.CreateInstance<SuitDefinition>();
					_definition.Init();

					//Call the transplant function.
				}
				return _definition;
			}
		}

		#region Transplant Fields
		//Make a Transplant class and have hidden fields to leverage prefab serialization.
		[SerializeField]
		public string SuitName = "Player Body";
		[SerializeField]
		public GameObject SuitRoot;

		[SerializeField]
		public List<AreaFlag> DefinedAreas;

		//The Game Objects to fill the fields (which will get hardlight collider references)
		[SerializeField]
		public List<GameObject> ZoneHolders;

		//the objects added. Will get a nice button list to quick get to each of them.
		[SerializeField]
		public List<HardlightCollider> SceneReferences;

		[SerializeField]
		public int HapticsLayer = NSManager.HAPTIC_LAYER;
		[SerializeField]
		public bool AddChildObjects = true;
		[SerializeField]
		public bool AddExclusiveTriggerCollider = true;

		public void EnsureInit()
		{
			//Ensure the lists are all valid
			Debug.LogError("Unimplemented Ensure Init()");
		}
		#endregion

		public void AddSceneReference(int index, HardlightCollider suit)
		{
			if (SceneReferences == null)
			{
				SceneReferences = new List<HardlightCollider>();
			}

			//if (SceneReferences.Count < index)
			//{
			SceneReferences.Add(suit);
			//}

		}

		public static HardlightSuit Find()
		{
			HardlightSuit body = FindObjectOfType<HardlightSuit>();
			if (body != null)
			{
				return body;
			}
			BodyMimic.Initialize();

			body = FindObjectOfType<HardlightSuit>();
			if (body != null)
			{
				return body;
			}
			return null;
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
			GameObject closest = Definition.GetNearestLocation(point, maxDistance);

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
			GameObject[] closest = Definition.GetMultipleNearestLocations(point, 1, maxDistance);

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
			GameObject[] closest = Definition.GetMultipleNearestLocations(point, 16, maxDistance);

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
			HapticLocation loc = Definition.GetRandomLocationObject().GetComponent<HapticLocation>();
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