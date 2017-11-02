using UnityEngine;
using System.Collections;

namespace Hardlight.SDK
{
	public class FrameEvaluator : MonoBehaviour
	{
		public Transform AbsoluteObject;
		public Transform IMUObject;
		public Quaternion IMUOrientation;
		public Quaternion OffsetA;
		public Quaternion OffsetB;
		public GameObject prefab;
		public GameObject imuDisplay;
		public GameObject displayA;
		public GameObject displayB;
		public float aboveAmt = .6f;

		public FrameOfReferenceDisplay absolute;
		public FrameOfReferenceDisplay IMU;
		public FrameOfReferenceDisplay DisplayA;
		public FrameOfReferenceDisplay DisplayB;

		[System.Serializable]
		public class FrameOfReferenceDisplay
		{
			public Quaternion orientation;
			public GameObject display;
			public FrameOfReferenceDisplay(GameObject displayPrefab, string name, Transform parent)
			{
				display = GameObject.Instantiate<GameObject>(displayPrefab);
				if (parent != null)
					display.transform.SetParent(parent);
				display.name = name;
			}

			public void Update(Quaternion setRotation, Vector3 setPosition)
			{
				orientation = setRotation;
				display.transform.rotation = orientation;
				display.transform.position = setPosition;
			}
		}

		void Start()
		{
			IMU = new FrameOfReferenceDisplay(prefab, "Display IMU", transform);
			absolute = new FrameOfReferenceDisplay(prefab, "Display Absolute", transform);
			DisplayA = new FrameOfReferenceDisplay(prefab, "Display A = (Abs)^-1 * IMU", transform);
			DisplayB = new FrameOfReferenceDisplay(prefab, "Display B = (IMU)^-1 * Abs", transform);
		}

		void Update()
		{
			if (AbsoluteObject && IMUObject)
			{
				IMU.Update(IMUObject.rotation, AbsoluteObject.position + Vector3.up * (aboveAmt + .0f));
				absolute.Update(AbsoluteObject.rotation, AbsoluteObject.position + Vector3.up * (aboveAmt + .3f));
				DisplayA.Update(Subtract(AbsoluteObject.rotation, IMUObject.rotation), AbsoluteObject.position + Vector3.up * (aboveAmt + .6f));
				DisplayB.Update(Subtract(IMUObject.rotation, AbsoluteObject.rotation), AbsoluteObject.position + Vector3.up * (aboveAmt + .9f));

				//OffsetB = Subtract(IMUObject.rotation, AbsoluteObject.rotation);

				//imuDisplay.transform.rotation = IMUOrientation;
				//displayA.transform.rotation = OffsetA;
				//displayB.transform.rotation = OffsetB;

				//imuDisplay.transform.position = AbsoluteObject.position + Vector3.up * aboveAmt;
				//displayA.transform.position = ;
				//displayB.transform.position = AbsoluteObject.position + Vector3.up * (aboveAmt + .8f);
				//				Quaternion newRotation = transform.rotation * otherTransform.rotation
				//				transform.rotation = newRotation * Quaternion.Inverse(otherTransform.rotation)
			}
		}
		private Quaternion BadSubtract(Quaternion A, Quaternion B)
		{
			return A * Quaternion.Inverse(B);
		}
		private Quaternion Subtract(Quaternion A, Quaternion B)
		{
			return Quaternion.Inverse(A) * B;
		}
	}
}