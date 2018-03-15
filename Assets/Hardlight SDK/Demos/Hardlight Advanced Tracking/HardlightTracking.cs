/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using Hardlight.SDK;
using Hardlight.SDK.Experimental;
using System;

namespace Hardlight.SDK.Experimental
{
	/// <summary>
	/// This script is for creating a hardlight tracked IMU representation.
	/// You can configure the offset parameters to get the IMU into the correct coordinate space.
	/// Use the SavedTrackingCalibration to solve the Heading problem (basically aligning Unity's Z with Real_Life.North)
	/// </summary>
	public class HardlightTracking : MonoBehaviour
	{
		public GameObject TrackedRepresentation;
		public GameObject ParentObject;
		public Imu whichIMU = Imu.Chest;
		
		public bool DisableObject = true;
		public bool ShowOnGUI = false;
		public bool VisibleIdentity = false;
		public bool AutoEnableTracking = false;
		public Quaternion rawIncQuat;
		public Quaternion lastQuat;

		/// <summary>
		/// This is used to determine the Lerp/blend rate of new data over time.
		/// If its too close to 1, it will look choppy (limitation of data channel). Too low and it'll seem sluggish.
		/// This will vary based on the character. We find that 35% seems decent.
		/// </summary>
		[Range(0, 1)]
		public float PercentOfNewData = .35f;

		//[Header("Chirality Reversal Control")]
		//This bundle of variables is used to configurate chirality for different IMUs as well as troubleshooting.
		private bool reverseX = false;
		private bool reverseY = false;
		private bool reverseZ = false;
		private bool reverseW = true;

		[Header("Use Offset")]
		[Tooltip("This is for the pre-chirality reversal offset. Arms want (0,0,0). Torso wants (0, 15, 270)")]
		///<summary>The IMUs are left-handed while Unity is right-handed. We need to reverse the chirality of the incoming data. First we want to apply an offset rotation.</summary>
		public Vector3 preChiralOffset = Vector3.zero;

		[Tooltip("This is post-chirality offset. Arms want (-270, 0, 90). Torso wants (-270, 0, 180)")]
		///<summary>This exists to get the IMU rotating in the correct Unity space compared to world space. One arm is automatically flipped (so arms will have identical offset values)</summary>
		public Vector3 Offset = new Vector3(-270, 0, 90);

		/// <summary>
		/// This is a storage value. It's default value is meaningless. It is used to save the base post-chiral Offset.
		/// During runtime it is added with the current Calibration.Heading to produce the end Z axis offset.
		/// </summary>
		private float BaseZOffsetAmount = 90;
		[Header("Hardlight SDK\\Resources\\Saved Tracking Calibration")]
		[SerializeField]
		[Tooltip("This is where we calibrate the heading rotation. The algorithms we use can't find the heading of the world's north compared to Unity's north.\n\nThis only needs to be changed when the vive setup is readjusted.")]
		private SavedTrackingCalibration _trackingCalibration;
		public SavedTrackingCalibration Calibration
		{
			get
			{
				if (_trackingCalibration == null)
				{
					_trackingCalibration = Resources.Load<SavedTrackingCalibration>("Saved Tracking Calibration");
				}
				if (_trackingCalibration == null)
					_trackingCalibration = ScriptableObject.CreateInstance<SavedTrackingCalibration>();
				return _trackingCalibration;
			}

			set
			{
				if (_trackingCalibration == null)
				{
					_trackingCalibration = Resources.Load<SavedTrackingCalibration>("Saved Tracking Calibration");
				}
				if (_trackingCalibration == null)
					_trackingCalibration = ScriptableObject.CreateInstance<SavedTrackingCalibration>();
				_trackingCalibration = value;
			}
		}
		public float AdditionalZOffsetAmount
		{
			get
			{
				return Calibration.Heading;
			}

			set
			{
				Calibration.Heading = value;
			}
		}

		void Start()
		{
			BaseZOffsetAmount = Offset.z;

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
				//if (Input.GetKeyDown(KeyCode.Y))
				//{
				//	DisableTrackedRepresentationVisual = !DisableTrackedRepresentationVisual;
				//}

				Offset.z = BaseZOffsetAmount + AdditionalZOffsetAmount;

				var tracking = HardlightManager.Instance.PollTracking();
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

				assign.y = -assign.y;

				rawIncQuat = assign;
				lastQuat = assign;

				//float angle = 0;
				//Vector3 axis = Vector3.zero;

				//string infoDetails = name + " Diag\n";

				#region PreChiralOffset Quaternion Creation
				Quaternion preChiralOffsetQuat = Quaternion.identity;
				preChiralOffsetQuat = Quaternion.AngleAxis(preChiralOffset.z, Vector3.forward) * preChiralOffsetQuat;
				preChiralOffsetQuat = Quaternion.AngleAxis(preChiralOffset.y, Vector3.up) * preChiralOffsetQuat;
				preChiralOffsetQuat = Quaternion.AngleAxis(preChiralOffset.x, Vector3.right) * preChiralOffsetQuat;
				#endregion

				//preChiralOffsetQuat.ToAngleAxis(out angle, out axis);
				//infoDetails += "preChirQuad Axis: " + axis + "   -    " + angle + "   -    " + preChiralOffsetQuat + "\n";

				//=============================================
				assign = ReverseChirality(assign * preChiralOffsetQuat);
				//=============================================

				//assign.ToAngleAxis(out angle, out axis);
				//infoDetails += "RevChir(Assign*PreChiralQuat): " + axis + "   -    " + angle + "   -    " + assign + "\n\n";

				#region PostChiralOffset Quaternion Creation
				Quaternion postChiralOffsetQuat = Quaternion.identity;
				postChiralOffsetQuat = Quaternion.AngleAxis(Offset.z, Vector3.forward) * postChiralOffsetQuat;
				postChiralOffsetQuat = Quaternion.AngleAxis(Offset.y, Vector3.up) * postChiralOffsetQuat;
				postChiralOffsetQuat = Quaternion.AngleAxis(Offset.x, Vector3.right) * postChiralOffsetQuat;
				postChiralOffsetQuat = ReverseChiralityXZ(postChiralOffsetQuat);
				#endregion

				//postChiralOffsetQuat.ToAngleAxis(out angle, out axis);
				//infoDetails += "Offset Axis: " + axis + "  -  " + angle + "   -    " + postChiralOffsetQuat + "\n\n";
				//Debug.DrawLine(pos - axis * .1f, pos + axis * .1f, Color.black);

				//=============================================
				var FinalizedQuat = postChiralOffsetQuat * assign;
				//=============================================

				//FinalizedQuat.ToAngleAxis(out angle, out axis);
				//infoDetails += "Final Axis: " + axis + "   -    " + angle + "   -    " + FinalizedQuat + "\n\n";

				SetRepresentationOrientation(FinalizedQuat);

				//Debug.Log(infoDetails + "\n");
			}
		}

		private void SetRepresentationOrientation(Quaternion target)
		{
			TrackedRepresentation.transform.rotation = Quaternion.Lerp(TrackedRepresentation.transform.rotation, target, PercentOfNewData);
		}

		private void DrawTrackingUpdate(Quaternion assign, Vector3 north, Vector3 imuUp)
		{
			#region TrackingUpdate North/Up vectors
			Vector3 up = Vector3.up;
			//Debug.DrawLine(up * 3, up * 3 + north.normalized, Color.red);
			//Debug.DrawLine(up * 3, up * 3 + assign * north.normalized, Color.magenta);
			//Debug.DrawLine(up * 3, up * 3 + Quaternion.Inverse(assign) * north.normalized, Color.black);

			//Debug.DrawLine(up * 5, up * 5 + imuUp.normalized, Color.green);
			//Debug.DrawLine(up * 5, up * 5 + assign * imuUp.normalized, Color.cyan);
			//Debug.DrawLine(up * 5, up * 5 + Quaternion.Inverse(assign) * imuUp.normalized, Color.white);

			Vector3 chestRight = Vector3.Cross(imuUp.normalized, north.normalized);
			//Debug.DrawLine(up * 4, up * 4 + chestRight.normalized, Color.blue);
			//Debug.DrawLine(up * 4, up * 4 + assign * chestRight.normalized, Color.yellow);
			//Debug.DrawLine(up * 4, up * 4 + Quaternion.Inverse(assign) * chestRight.normalized, Color.grey);

			//Debug.Log("Chest North: " + north.normalized + "  Red \t\tMagenta: " + assign * north.normalized +
			//	"\n\t\tBlack: " + (Quaternion.Inverse(assign) * north.normalized).normalized);
			//Debug.Log("Chest Up:    " + imuUp.normalized + "  Green \t\tCyan:    " + assign * imuUp.normalized +
			//	"\n\t\tWhite: " + (Quaternion.Inverse(assign) * imuUp.normalized).normalized);
			#endregion
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