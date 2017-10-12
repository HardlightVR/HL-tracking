using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NullSpace.SDK
{
	public class AbsoluteLowerBackTracker : AbstractTracker
	{
		public GameObject ShoulderBarEffigy;
		public GameObject LowerBodyVisual;
		bool ShouldSetupStomach = true;
		bool StomachInitialized = false;
		public GameObject SingleTorsoEffigy;
		public List<GameObject> TorsoSegments = new List<GameObject>();

		[Range(.1f, .5f)]
		public float TorsoWidth = .3f;
		[Range(.1f, .5f)]
		public float TorsoHeight = .3f;
		[Range(.1f, .5f)]
		public float TorsoDepth = .3f;

		public GameObject UpperBodyAnchor;
		public BodyVisualPrefabData BodyPartPrefabs;

		public Vector3 EulerOffset;

		[Header("Segmented Torso Approach")]
		[Range(2, 15)]
		public int SegmentCount = 15;
		public Vector3 ShoulderScale = new Vector3(.4f, .5f, .2f);
		public Vector3 WaistScale = new Vector3(.4f, .5f, .2f);
		public Vector3 TrackerOffset;
		public Vector3 ShoulderOffset;

		void Update()
		{
			transform.localPosition = Vector3.zero;
			transform.rotation = Quaternion.identity;
			if (TrackerMimic && ShoulderBarEffigy)
			{
				UpdateTorso();
			}

			if (Input.GetKeyDown(KeyCode.I))
			{
				DetachVisuals();
				ShouldSetupStomach = false;
			}
		}

		public void CreateVisuals()
		{
			if (BodyPartPrefabs == null)
			{
				Debug.LogError("Body Parts Prefab is null. It is needed to set up " + name + "'s visuals\n", this);
			}
			SetupStomach();
		}

		private void UpdateTorso()
		{
			if ((SingleTorsoEffigy == null || !StomachInitialized) && ShouldSetupStomach)
			{
				CreateVisuals();
			}
			if (SingleTorsoEffigy != null && StomachInitialized)
			{
				SingleTorsoEffigy.transform.localScale = new Vector3(TorsoWidth, TorsoHeight, TorsoDepth);
				SingleTorsoEffigy.transform.position = TrackerMimic.transform.position + Offset;
				Quaternion QOffset = Quaternion.identity;
				QOffset.eulerAngles = EulerOffset;
				SingleTorsoEffigy.transform.rotation = TrackerMimic.transform.rotation * QOffset;

				if (TorsoSegments.Count > 0)
				{
					//Default Variables
					var target = ShoulderBarEffigy.transform.rotation * QOffset;
					Vector3 segmentPosition = Vector3.zero;
					Vector3 lerpedScale = Vector3.zero;
					Quaternion segmentLerpedOrientation = Quaternion.identity;

					for (int i = 0; i < TorsoSegments.Count; i++)
					{
						if (TorsoSegments[i] != null)
						{
							float perc = (float)i / ((float)TorsoSegments.Count - 1);
							lerpedScale = Vector3.Lerp(WaistScale, ShoulderScale, perc);
							lerpedScale.y = (lerpedScale.y / TorsoSegments.Count) * 1.2f;
							TorsoSegments[i].transform.localScale = lerpedScale;
							segmentPosition = Vector3.Lerp(
								TrackerMimic.transform.position + TrackerOffset,
								ShoulderBarEffigy.transform.position + ShoulderOffset,
								perc);
							TorsoSegments[i].transform.position = segmentPosition + Offset;
							segmentLerpedOrientation = Quaternion.Lerp(TrackerMimic.transform.rotation, target, perc);
							TorsoSegments[i].transform.rotation = segmentLerpedOrientation;
						}
					}
				}
			}
		}

		/// <summary>
		/// Creates a single volume that represents the player's torso
		/// </summary>
		public void SetupStomach()
		{
			SingleTorsoEffigy = GameObject.Instantiate<GameObject>(BodyPartPrefabs.TorsoPrefab);
			SingleTorsoEffigy.name = "Torso Effigy";
			SingleTorsoEffigy.transform.localPosition = Offset;
			SingleTorsoEffigy.transform.SetParent(transform);

			StomachInitialized = true;

			//DISABLING THIS WHILE TORSO SEGMENTS ARE DEVELOPED
			SingleTorsoEffigy.SetActive(false);

			SetupTorsoSegments(SegmentCount);
		}

		/// <summary>
		/// Creates N segments from the shoulders down to the location of the back tracker.
		/// </summary>
		/// <param name="segments"></param>
		public void SetupTorsoSegments(int segments)
		{
			for (int i = 0; i < segments; i++)
			{
				var newSegment = GameObject.Instantiate<GameObject>(BodyPartPrefabs.TorsoPrefab);
				newSegment.name = "Torso Segment [" + i + "]";
				newSegment.transform.localPosition = Offset;
				newSegment.transform.SetParent(transform);
				TorsoSegments.Add(newSegment);
			}
		}

		public void DetachVisuals(bool DropInsteadOfDelete = true)
		{
			VisualDisposer disposer = new VisualDisposer();

			disposer.RecordVisual(SingleTorsoEffigy);
			SingleTorsoEffigy = null;
			disposer.RecordVisual(ShoulderBarEffigy);
			ShoulderBarEffigy = null;

			for (int i = TorsoSegments.Count - 1; i >= 0; i--)
			{
				disposer.RecordVisual(TorsoSegments[i].gameObject);
			}
			TorsoSegments.Clear();

			//Remove the hardlight colliders

			//Leave the visuals alive

			if (DropInsteadOfDelete)
				disposer.DropRecordedVisuals();
			else
				disposer.DeleteRecordedVisuals();
		}
	}
}