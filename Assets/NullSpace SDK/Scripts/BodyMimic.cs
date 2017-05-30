using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using NullSpace.SDK;

namespace NullSpace.SDK
{
	public class BodyMimic : MonoBehaviour
	{
		[Header("Body Hang Origin")]
		public GameObject hmd;

		[Range(0, .35f)]
		public float TiltAmtWithHMD = 0.0f;

		[Header("How far down the body is from HMD")]
		[Range(-2, 2)]
		public float NeckVerticalAnchor = .25f;

		[Header("How far fwd or back the body is")]
		[Range(-2, 2)]
		public float NeckFwdAnchor = 0;

		public bool UseDevHeight = false;
		public float devHeightPercentage = -0.15f;

		public Vector3 assumedForward = Vector3.zero;
		public Vector3 LastUpdatedPosition = Vector3.zero;

		private float updateRate = .05f;

		public float TimeSinceUpdate = .2f;
		private float UpdateDuration = .75f;
		private float UpdateCounter = .2f;
		Vector3 targetPosition;

		[Header("Exceed this val to force update")]
		public float SnapUpdateDist = 1.0f;
		private Vector3 LastRelativePosition;

		//[Header("Floor Evaluation")]
		//public bool UseHeadRaycasting = false;
		//public LayerMask validFloorLayers = ~((1 << 2) | (1 << 9) | (1 << 10) | (1 << 12) | (1 << 13) | (1 << 15));

		void FixedUpdate()
		{
			Vector3 hmdNoUp = hmd.transform.forward;
			hmdNoUp.y = 0;
			Vector3 hmdUpNoY = hmd.transform.up;
			hmdUpNoY.y = 0;

			//If we teleport or are too far away
			if (Vector3.Distance(hmd.transform.position, LastUpdatedPosition) > SnapUpdateDist)
			{
				//Force an update for now
				ImmediateUpdate();
			}
			else
			{
				LastRelativePosition = transform.position - hmd.transform.position;
			}

			//We want to use the HMD's Up to find which way we should actually look to solve the overtilting problem
			float mirrorAngleAmt = Vector3.Angle(hmd.transform.forward, Vector3.up);

			//Check if we need to do a mirror operation
			if (mirrorAngleAmt < 5 || mirrorAngleAmt > 175)
			{
				hmdNoUp = -hmdNoUp;
			}

			UpdateCounter += Time.deltaTime * updateRate;

			//This is logic to let us update only some of the time.
			if (UpdateCounter >= UpdateDuration)
			{
				UpdateCounter = 0;
				LastUpdatedPosition = hmd.transform.position;

				//We reset the update rate. The core of this logic was to have certain criteria that used a higher update rate (so we would get closer to the next update quicker)
				updateRate = .05f;
			}

			//float prog = UpdateDuration - UpdateCounter;
			LastUpdatedPosition = Vector3.Lerp(LastUpdatedPosition, hmd.transform.position, .5f);// Mathf.Clamp(prog / UpdateDuration, 0, 1));

			Vector3 flatRight = hmd.transform.right;
			flatRight.y = 0;

			Vector3 rep = Vector3.Cross(flatRight, Vector3.up);

			assumedForward = rep.normalized;

			Debug.DrawLine(hmd.transform.position + Vector3.up * 5.5f, hmd.transform.position + rep + Vector3.up * 5.5f, Color.grey, .08f);

			float dist = hmd.transform.position.y - hmd.transform.parent.transform.position.y;
			//Debug.Log(hit.collider.gameObject.name + "\n is " + dist + " away " + dist * beltHeightPercentage + "  " + hit.collider.gameObject.layer);
			Vector3 hmdDown = Vector3.down * dist * (UseDevHeight ? devHeightPercentage : NeckVerticalAnchor);
			targetPosition = assumedForward * (.25f + NeckFwdAnchor) + hmd.transform.position + hmdDown;

			transform.position = Vector3.Lerp(transform.position, targetPosition, updateRate);

			//Create the transform based on our position and where we should face.
			transform.LookAt(transform.position + assumedForward * 5, Vector3.up);
		}

		/// <summary>
		/// Force an update of the BodyMimic (in case of teleports, fast movement)
		/// </summary>
		void ImmediateUpdate()
		{
			transform.position = hmd.transform.position + LastRelativePosition;
		}

		/// <summary>
		/// This function creates and initializes the Body Mimic
		/// </summary>
		/// <param name="camera">The camera to hide the body from. Calls camera.HideLayer(int)</param>
		/// <param name="hapticObjectLayer">The layer that is removed from the provided camera's culling mask.</param>
		/// <returns>The created body mimic</returns>
		public static BodyMimic Initialize(Camera camera = null, int hapticObjectLayer = 31)
		{
			GameObject go = Resources.Load<GameObject>("Body Mimic");

			//Instantiate the prefab of the body mimic.
			GameObject newMimic = Instantiate<GameObject>(go);
			newMimic.name = "Body Mimic";

			BodyMimic mimic = null;

			if (newMimic != null)
			{
				//Set the BodyMimic's target to the VRObjectMimic
				mimic = newMimic.GetComponent<BodyMimic>();
				mimic.hmd = VRObjectMimic.Holder.Camera.gameObject;
				mimic.transform.SetParent(VRObjectMimic.Holder.Root.transform);
			}

			if (camera != null)
			{
				camera.HideLayer(hapticObjectLayer);
			}

			return mimic;
		}
	}
}