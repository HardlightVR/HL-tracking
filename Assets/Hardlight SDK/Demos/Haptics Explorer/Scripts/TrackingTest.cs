/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using Hardlight.SDK;
using Hardlight.SDK.Tracking;

namespace Hardlight.SDK.Tracking
{
	public class TrackingTest : MonoBehaviour
	{
		private IImuCalibrator imus;
		public GameObject TrackedRepresentation;
		public GameObject ParentObject;
		public Imu whichIMU = Imu.Chest;
		public bool DisableObject = true;
		public bool ShowOnGUI = false;
		public bool VisibleIdentity = false;
		public bool AutoEnableTracking = false;
		public Quaternion lastQuat;

		[Range(0, 1)]
		public float PercentOfNewData = .95f;

		[Header("Chirality Reversal Control")]
		public bool reverseX = false;
		public bool reverseY = false;
		public bool reverseZ = false;
		public bool reverseW = true;

		[Header("Use Offset")]
		public bool UseOffset = true;
		public Vector3 Offset;
		public bool reverseOffsetXZ = true;
		public bool reverseOrder = false;

		[Header("Calibrating")]
		public Vector3 baseVector;

		void Start()
		{
			xQuat = Quaternion.identity;
			yQuat = Quaternion.identity;
			zQuat = Quaternion.identity;

			imus = GetComponent<DefaultImuCalibrator>();
			HardlightManager.Instance.SetImuCalibrator(GetComponent<DefaultImuCalibrator>());

			if (ParentObject != null)
			{
				ParentObject.SetActive(!DisableObject);
			}

			if (AutoEnableTracking)
				EnableTracking();
		}

		public void EnableTracking()
		{
			if (ParentObject != null)
			{
				ParentObject.SetActive(true);
			}
			HardlightManager.Instance.EnableTracking();
		}

		public void DisableTracking()
		{
			if (ParentObject != null)
			{
				ParentObject.SetActive(false);
			}
			HardlightManager.Instance.DisableTracking();
		}

		void Update()
		{
			if (TrackedRepresentation != null)
			{
				var tracking = HardlightManager.Instance.PollTracking();
				//Debug.Log(tracking.Chest + "\n" + imus.GetType().ToString());
				Quaternion assign = Quaternion.identity;

				if (whichIMU == Imu.Chest)
				{
					assign = tracking.Chest;
				}
				else if (whichIMU == Imu.Left_Upper_Arm)
				{
					assign = tracking.LeftUpperArm;
				}
				else if (whichIMU == Imu.Right_Upper_Arm)
				{
					assign = tracking.RightUpperArm;
				}
				lastQuat = assign;
				Quaternion rawQuat = assign;


				//if (true)
				//{
				//	Vector3 imuX = assign * Vector3.right;
				//	Vector3 imuY = assign * Vector3.up;
				//	Vector3 imuZ = assign * Vector3.forward;

				//	Debug.DrawLine(TrackedRepresentation.transform.position, TrackedRepresentation.transform.position + imuX, Color.red);
				//	Debug.DrawLine(TrackedRepresentation.transform.position, TrackedRepresentation.transform.position + imuY, Color.green);
				//	Debug.DrawLine(TrackedRepresentation.transform.position, TrackedRepresentation.transform.position + imuZ, Color.blue);
				//	TrackedRepresentation.transform.rotation = rawQuat;// imus.GetOrientation(MyIMU);
				//}
				//if (false)
				//{
				assign = ReverseChirality(assign);
				VisibleIdentity = assign != Quaternion.identity;

				Quaternion quat = Quaternion.identity;
				if (UseOffset)
				{
					quat = Quaternion.AngleAxis(Offset.z, Vector3.forward) * quat;
					quat = Quaternion.AngleAxis(Offset.y, Vector3.up) * quat;
					quat = Quaternion.AngleAxis(Offset.x, Vector3.right) * quat;
					quat = reverseOffsetXZ ? ReverseChiralityXZ(quat) : quat;
				}

				var myQuat = reverseOrder ? assign * quat : quat * assign;
				TrackedRepresentation.transform.rotation = Quaternion.Lerp(TrackedRepresentation.transform.rotation, myQuat, PercentOfNewData);

				if (Input.GetKeyDown(KeyCode.Alpha1))
				{
					SaveX(myQuat);
				}
				if (Input.GetKeyDown(KeyCode.Alpha2))
				{
					SaveY(myQuat);
				}
				if (Input.GetKeyDown(KeyCode.Alpha3))
				{
					SaveZ(myQuat);
				}

				Vector3 saveVector = myQuat * baseVector;
				Debug.DrawLine(TrackedRepresentation.transform.position, TrackedRepresentation.transform.position + saveVector, Color.white);

				if (xQuat != Quaternion.identity || yQuat != Quaternion.identity || zQuat != Quaternion.identity)
				{
					Vector3 xRight = xQuat * baseVector;
					Vector3 yUp = yQuat * baseVector;
					Vector3 zFwd = zQuat * baseVector;

					Debug.DrawLine(TrackedRepresentation.transform.position, TrackedRepresentation.transform.position + xRight, Color.red);
					Debug.DrawLine(TrackedRepresentation.transform.position, TrackedRepresentation.transform.position + yUp, Color.green);
					Debug.DrawLine(TrackedRepresentation.transform.position, TrackedRepresentation.transform.position + zFwd, Color.blue);
				}
			}
		}

		Quaternion xQuat;
		Quaternion yQuat;
		Quaternion zQuat;
		private void SaveX(Quaternion quat)
		{
			xQuat = quat;
		}
		private void SaveY(Quaternion quat)
		{
			yQuat = quat;
		}
		private void SaveZ(Quaternion quat)
		{
			zQuat = quat;
		}

		private Quaternion ReverseChiralityXZ(Quaternion quat)
		{
			var q = quat;
			q.x = -q.x;
			q.z = -q.z;
			return q;
		}

		private UnityEngine.Quaternion ReverseChirality(UnityEngine.Quaternion quat)
		{
			if (reverseX)
				quat.x = -quat.x;
			if (reverseY)
				quat.y = -quat.y;
			if (reverseZ)
				quat.z = -quat.z;
			if (reverseW)
				quat.w = -quat.w;
			return quat;
		}
	}
}