using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Hardlight.SDK
{
	public class FrameEvaluator : MonoBehaviour
	{
		public Transform AbsoluteObject;
		public Transform IMUObject;
		public GameObject prefab;

		public float aboveAmt = .6f;

		[Header("Runtime calculated")]
		public Quaternion OffsetCalibration;

		public FrameOfReferenceDisplay absolute;
		public FrameOfReferenceDisplay IMU;
		public FrameOfReferenceDisplay CalibratedIMU;
		public FrameOfReferenceDisplay IMU2;
		#region Junk Frame of Reference Displays
		//public FrameOfReferenceDisplay DisplayA;
		//public FrameOfReferenceDisplay DisplayB;
		//public FrameOfReferenceDisplay DisplayC;
		//public FrameOfReferenceDisplay DisplayD;
		//public FrameOfReferenceDisplay DisplayEulerSwizzle;
		//public FrameOfReferenceDisplay DisplayF;
		//public FrameOfReferenceDisplay DisplayG;
		//public FrameOfReferenceDisplay DisplayH;
		//public FrameOfReferenceDisplay DisplayReverseChiral;
		//public FrameOfReferenceDisplay AxisReorg; 

		//public Vector3 eulerAbsolute;
		//public Vector3 eulerIMU;
		//public Vector3 difference;
		#endregion

		public FrameOfReferenceDisplay OffsetDisplay;
		public FrameOfReferenceDisplay appliedOffsetDisplay;

		public FrameOfReferenceDisplay imuToAbsolute;
		public FrameOfReferenceDisplay absoluteToIMU;

		[System.Serializable]
		public class FrameOfReferenceDisplay
		{
			public Quaternion Offset;
			public Quaternion orientation;
			public GameObject display;
			public Transform displayLocation;
			public float upComponent;
			public Vector3 pos
			{
				get { return displayLocation.position + Vector3.up * upComponent; }
			}
			public FrameOfReferenceDisplay(GameObject displayPrefab, string name, Transform parent, float upComp, Transform objLoc)
			{
				display = GameObject.Instantiate<GameObject>(displayPrefab);
				if (parent != null)
					display.transform.SetParent(parent);
				display.name = name;
				Offset = Quaternion.identity;
				displayLocation = objLoc;
				upComponent = upComp;
			}

			public void Update(Quaternion setRotation)
			{
				orientation = setRotation;
				display.transform.rotation = Offset * orientation;
				display.transform.position = pos;

				Vector3 upGoal = pos + Vector3.up * .24f;
				Vector3 rightGoal = pos + Vector3.right * .24f;
				Vector3 fwdGoal = pos + Vector3.forward * .24f;

				Vector3 myUp = pos + display.transform.rotation * Vector3.up * .2f;
				Vector3 myRight = pos + display.transform.rotation * Vector3.right * .2f;
				Vector3 myFwd = pos + display.transform.rotation * Vector3.forward * .2f;

				Debug.DrawLine(pos, upGoal, Color.green);
				Debug.DrawLine(pos, rightGoal, Color.red);
				Debug.DrawLine(pos, fwdGoal, Color.blue);

				Debug.DrawLine(myUp, upGoal, Color.green);
				Debug.DrawLine(myRight, rightGoal, Color.red);
				Debug.DrawLine(myFwd, fwdGoal, Color.blue);
			}

		}

		void Start()
		{
			IMU = new FrameOfReferenceDisplay(prefab, "Display IMU", transform, aboveAmt + 1.0f, AbsoluteObject);
			CalibratedIMU = new FrameOfReferenceDisplay(prefab, "Calibrated IMU", transform, aboveAmt + .5f, AbsoluteObject);
			IMU2 = new FrameOfReferenceDisplay(prefab, "Display IMU2", transform, aboveAmt + 2.5f, AbsoluteObject);
			absolute = new FrameOfReferenceDisplay(prefab, "Display Absolute", transform, aboveAmt + 1.5f, AbsoluteObject);
			//DisplayA = new FrameOfReferenceDisplay(prefab, "Display A = (Abs)^-1 * IMU", transform, aboveAmt + 0.7f, AbsoluteObject);
			//DisplayB = new FrameOfReferenceDisplay(prefab, "Display B = (IMU)^-1 * Abs", transform, aboveAmt + 1.05f, AbsoluteObject);
			//DisplayB = new FrameOfReferenceDisplay(prefab, "Display B = Euler subtraction", transform, aboveAmt + 1.5f, AbsoluteObject);
			//DisplayC = new FrameOfReferenceDisplay(prefab, "Display C = Vector3 Conversion", transform, aboveAmt + 2.0f, AbsoluteObject);
			//DisplayD = new FrameOfReferenceDisplay(prefab, "Display D = Calibrated IMU", transform, aboveAmt + 2.5f, AbsoluteObject);
			//DisplayEulerSwizzle = new FrameOfReferenceDisplay(prefab, "Display E = Euler Swizzle", transform, aboveAmt + 1.0f, AbsoluteObject);
			//DisplayF = new FrameOfReferenceDisplay(prefab, "Display F = ChiralReverse(EulerSwizzle(IMU))", transform, aboveAmt + 1.5f, AbsoluteObject);
			////DisplayG = new FrameOfReferenceDisplay(prefab, "Display G = Subtract(ChiralReverse(EulerSwizzle(IMU)))", transform, aboveAmt + 2.0f, AbsoluteObject);
			//DisplayH = new FrameOfReferenceDisplay(prefab, "Display H = Subtract(EulerSwizzle(ChiralReverse(IMU)))", transform, aboveAmt + 2.0f, AbsoluteObject);
			//DisplayReverseChiral = new FrameOfReferenceDisplay(prefab, "Display Chir = ChiralReverse(IMU)", transform, aboveAmt + 2.5f, AbsoluteObject);
			//AxisReorg = new FrameOfReferenceDisplay(prefab, "IMU Data Axis Reorg", transform, aboveAmt + 4.0f, AbsoluteObject);
			imuToAbsolute = new FrameOfReferenceDisplay(prefab, "IMU to Absolute", transform, aboveAmt + 2.0f, AbsoluteObject);
			absoluteToIMU = new FrameOfReferenceDisplay(prefab, "Absolute to IMU", transform, aboveAmt + 3.0f, AbsoluteObject);

			OffsetDisplay = new FrameOfReferenceDisplay(prefab, "Calculated Offset", transform, aboveAmt + 3.5f, AbsoluteObject);
			appliedOffsetDisplay = new FrameOfReferenceDisplay(prefab, "Applied Offset", transform, aboveAmt + 4.0f, AbsoluteObject);

			#region SteamVR Absolute Frame (not helpful, requires Matrix usage)
			//[StructLayout(LayoutKind.Sequential)]
			//public struct TrackedDevicePose_t
			//{
			//	public HmdMatrix34_t mDeviceToAbsoluteTracking;
			//	public HmdVector3_t vVelocity;
			//	public HmdVector3_t vAngularVelocity;
			//	public ETrackingResult eTrackingResult;
			//	[MarshalAs(UnmanagedType.I1)]
			//	public bool bPoseIsValid;
			//	[MarshalAs(UnmanagedType.I1)]
			//	public bool bDeviceIsConnected;
			//}

			//Valve.VR.TrackedDevicePose_t[] thingies = new Valve.VR.TrackedDevicePose_t[1];
			//steamVR.hmd.GetDeviceToAbsoluteTrackingPose(Valve.VR.ETrackingUniverseOrigin.TrackingUniverseRawAndUncalibrated, 0, thingies);

			//Matrix4x4 newMat = new Matrix4x4();
			//newMat[00] = thingies[0].mDeviceToAbsoluteTracking.m0;
			//newMat[01] = thingies[0].mDeviceToAbsoluteTracking.m1;
			//newMat[02] = thingies[0].mDeviceToAbsoluteTracking.m2;
			//newMat[03] = thingies[0].mDeviceToAbsoluteTracking.m3;
			//newMat[10] = thingies[0].mDeviceToAbsoluteTracking.m4;
			//newMat[11] = thingies[0].mDeviceToAbsoluteTracking.m5;
			//newMat[12] = thingies[0].mDeviceToAbsoluteTracking.m6;
			//newMat[13] = thingies[0].mDeviceToAbsoluteTracking.m7;
			//newMat[20] = thingies[0].mDeviceToAbsoluteTracking.m8;
			//newMat[21] = thingies[0].mDeviceToAbsoluteTracking.m9;
			//newMat[22] = thingies[0].mDeviceToAbsoluteTracking.m10;
			//newMat[23] = thingies[0].mDeviceToAbsoluteTracking.m11;

			//newMat. 
			#endregion
		}



		void Update()
		{
			if (AbsoluteObject && IMUObject)
			{
				var visualAlignmentOffset = Quaternion.AngleAxis(65, Vector3.up) * Quaternion.AngleAxis(90, Vector3.right);
				var movementAlignmentOffset = Quaternion.AngleAxis(180 + 45, Vector3.up) * Quaternion.AngleAxis(90, Vector3.right);
				//A Raw IMU rotation
				IMU.Update(IMUObject.rotation);
				//A Raw IMU rotation
				IMU2.Update(IMUObject.rotation);

				#region Junk Cases
				//IMU.Update(visualAlignmentOffset * IMUObject.rotation);									//B Visual Offset IMU Rotation
				//IMU.Update(movementAlignmentOffset * IMUObject.rotation);									//B-2 Movement Offset IMU Rotation
				//IMU.Update(IMUObject.rotation * visualAlignmentOffset);									//C Axis switched by frame of reference (not useful)
				//IMU.Update(ReverseChiralityY(movementAlignmentOffset * IMUObject.rotation));				//D Chirality (not useful)
				//var postChiralityAttempt = Quaternion.AngleAxis(180 + 45, Vector3.up) * Quaternion.AngleAxis(90, Vector3.right);
				//IMU.Update(postChiralityAttempt * ReverseChiralityY(IMUObject.rotation));                        //E Chirality Y (not useful)
				//IMU.Update(postChiralityAttempt * ReverseChiralityX(IMUObject.rotation));						//F Chirality (not useful)
				//IMU.Update(postChiralityAttempt * ReverseChiralityZ(IMUObject.rotation));						//G Chirality (not useful)
				//IMU.Update(chiralityOffset * ReverseChiralityXZ(IMUObject.rotation));                          //H Chirality XZ (shows promise) 
				#endregion

				//[GOLDEN CASE] I Chirality XZ
				//var chiralityOffset = Quaternion.AngleAxis(45, Vector3.up) * Quaternion.AngleAxis(90, Vector3.right);
				//imuToAbsolute.Update(ReverseChiralityXZ(chiralityOffset * IMUObject.rotation)); //This was before the chiralirty happened in the plugin

				//[GOLDEN CASE] CHIRALITY SWITCH IN PLUGIN
				//var chiralityOffset = Quaternion.AngleAxis(-315, Vector3.up) * Quaternion.AngleAxis(-90, Vector3.right);
				var chiralityOffset = Quaternion.AngleAxis(-315, Vector3.up) * Quaternion.AngleAxis(-90, Vector3.right);
				imuToAbsolute.Update(chiralityOffset * IMUObject.rotation);

				#region Input
				if (Input.GetKeyDown(KeyCode.R))
				{
					RightQuaternion = IMUObject.rotation;
					OffsetFromRight = Quaternion.identity;
					OffsetFromRight = Subtract(OffsetFromRight, RightQuaternion);
				}
				if (Input.GetKeyDown(KeyCode.Y))
				{
					UpQuaternion = IMUObject.rotation;
					OffsetFromUp = Quaternion.identity;
					OffsetFromUp = Subtract(OffsetFromUp, UpQuaternion);
				}
				#endregion

				#region QuatDraw
				if (RightQuaternion != Quaternion.identity)
				{
					DrawThingy(IMU.pos + Vector3.right * .5f, RightQuaternion);
				}
				if (RightQuaternion != Quaternion.identity)
				{
					DrawThingy(IMU.pos + Vector3.right * .85f, OffsetFromRight);
				}
				if (OffsetFromUp != Quaternion.identity)
				{
					DrawThingy(IMU.pos + Vector3.forward * .5f, UpQuaternion);
				}
				if (OffsetFromRight != Quaternion.identity)
				{
					DrawThingy(IMU.pos + Vector3.forward * .85f, OffsetFromUp);
				}

				Result = OffsetFromRight * OffsetFromUp;
				
				CalibratedIMU.Update(Subtract(Quaternion.identity, Result) * IMUObject.transform.rotation);
				DrawThingy(IMU.pos + Vector3.right * .65f + Vector3.forward * .65f, Result);
				absoluteToIMU.Update(Quaternion.Inverse(chiralityOffset) * ReverseChiralityXZ(AbsoluteObject.rotation));
				#endregion

				//imuToAbsolute.Update(ReverseChiralityXZ(IMUObject.rotation));
				absoluteToIMU.Update(Quaternion.Inverse(chiralityOffset) * ReverseChiralityXZ(AbsoluteObject.rotation));

				var calculatedOffset = Subtract(Quaternion.Inverse(chiralityOffset) * ReverseChiralityXZ(AbsoluteObject.rotation), IMUObject.rotation);
				OffsetDisplay.Update(calculatedOffset);
				appliedOffsetDisplay.Update(calculatedOffset * IMUObject.rotation);

				absolute.Update(AbsoluteObject.rotation);


				#region Junk Cases
				//eulerAbsolute = AbsoluteObject.rotation.eulerAngles;
				//eulerIMU = IMUObject.rotation.eulerAngles;
				//difference = eulerAbsolute - eulerIMU;
				//DisplayA.Update(Subtract(AbsoluteObject.rotation, IMUObject.rotation)); 
				//DisplayB.Update(Subtract(IMUObject.rotation, AbsoluteObject.rotation));

				//Euler Subtraction
				//DisplayB.Update(Quaternion.Euler(difference) * IMUObject.rotation);

				//Vector3 up = Vector3.up * .3f;
				//Vector3 right = Vector3.right * .3f;
				//Vector3 fwd = Vector3.forward * .3f;
				//Vector3 imuUp = IMUObject.rotation * up;
				//Vector3 absUp = AbsoluteObject.rotation * up;

				//Vector3 imuRight = IMUObject.rotation * right;
				//Vector3 absRight = AbsoluteObject.rotation * right;

				//Vector3 imuFwd = IMUObject.rotation * fwd;
				//Vector3 absFwd = AbsoluteObject.rotation * fwd;

				//Debug.DrawLine(absolute.pos, absolute.pos + absUp, Color.green);
				//Debug.DrawLine(absolute.pos, absolute.pos + absUp, Color.green);
				//Debug.DrawLine(absolute.pos, absolute.pos + absUp, Color.green);

				//OffsetCalibration = RotationBetweenVectors(absUp, imuUp);
				//DisplayC.Update(OffsetCalibration);
				//DisplayD.Update(OffsetCalibration * IMUObject.rotation);
				//DisplayEulerSwizzle.Update(EulerSwizzle(IMUObject.rotation));

				//DisplayF.Update(ReverseChiralityX(EulerSwizzle(IMUObject.rotation)));

				//Important Lesson: EulerSwizzle(ReverseChirality(rot)) == ReverseChirality(EulerSwizzle(rot))
				//DisplayG.Update(Subtract(ReverseChirality(EulerSwizzle(IMUObject.rotation)), AbsoluteObject.rotation));
				//DisplayH.Update(Subtract(EulerSwizzle(ReverseChiralityX(IMUObject.rotation)), AbsoluteObject.rotation));

				//DisplayReverseChiral.Update(ReverseChiralityX(IMUObject.rotation));
				//AxisReorg.Update(ReorderAxis(IMUObject.rotation));
				//SubReorgFromAbs.Update(Subtract(ReorderAxis(IMUObject.rotation), AbsoluteObject.rotation));
				//DisplayE.Update(ReverseChirality(EulerSwizzle(IMUObject.rotation)));
				//DisplayC.Update(IMUObject.rotation * AbsoluteObject.rotation, AbsoluteObject.position + Vector3.up * (aboveAmt + 1.2f)); 
				#endregion
			}
		}

		void DrawThingy(Vector3 position, Quaternion draw)
		{
			float Offset = 1.0f;

			Vector3 upGoal = position + Vector3.up * .24f;
			Vector3 rightGoal = position + Vector3.right * .24f;
			Vector3 fwdGoal = position + Vector3.forward * .24f;

			Vector3 myUp = position + draw * Vector3.up * .2f;
			Vector3 myRight = position + draw * Vector3.right * .2f;
			Vector3 myFwd = position + draw * Vector3.forward * .2f;

			Debug.DrawLine(position, upGoal, Color.green);
			Debug.DrawLine(position, rightGoal, Color.red);
			Debug.DrawLine(position, fwdGoal, Color.blue);

			Debug.DrawLine(myUp, upGoal, Color.green);
			Debug.DrawLine(myRight, rightGoal, Color.red);
			Debug.DrawLine(myFwd, fwdGoal, Color.blue);
		}

		public Quaternion RightQuaternion;
		public Quaternion UpQuaternion;
		public Quaternion OffsetFromRight;
		public Quaternion OffsetFromUp;
		public Quaternion Result;

		private KeyValuePair<Vector3, Quaternion> RightKVP;
		private KeyValuePair<Vector3, Quaternion> UpKVP;

		private Quaternion Subtract(Quaternion A, Quaternion B)
		{
			//Inverse(A) * B == A * Inverse(B)
			return Quaternion.Inverse(A) * B;
		}

		//Credit: http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-17-quaternions/
		private Quaternion RotationBetweenVectors(Vector3 start, Vector3 dest)
		{
			start.Normalize();
			dest.Normalize();

			float cosTheta = Vector3.Dot(start, dest);
			Vector3 rotationAxis;

			if (cosTheta < -1 + 0.001f)
			{
				// special case when vectors in opposite directions:
				// there is no "ideal" rotation axis
				// So guess one; any will do as long as it's perpendicular to start
				rotationAxis = Vector3.Cross(Vector3.forward, start);

				if (rotationAxis.magnitude < 0.01) // bad luck, they were parallel, try again!
					rotationAxis = Vector3.Cross(Vector3.right, start);

				rotationAxis.Normalize();
				return Quaternion.AngleAxis(180, rotationAxis);
			}

			rotationAxis = Vector3.Cross(start, dest);

			float s = Mathf.Sqrt((1 + cosTheta) * 2);
			float invs = 1 / s;

			return new Quaternion(
				s * 0.5f,
				rotationAxis.x * invs,
				rotationAxis.y * invs,
				rotationAxis.z * invs
			);

		}

		/// <summary>
		/// Our golden case for turning IMU data into our coordinate frame
		/// </summary>
		/// <param name="quat"></param>
		/// <returns></returns>
		private Quaternion ReverseChiralityXZ(Quaternion quat)
		{
			var q = quat;
			q.x = -q.x;
			q.z = -q.z;
			return q;
		}

		#region Non-useful Functions
		private Quaternion ReverseChiralityX(Quaternion quat)
		{
			var q = quat;
			q.x = -q.x;
			return q;
		}
		private Quaternion ReverseChiralityY(Quaternion quat)
		{
			var q = quat;
			q.y = -q.y;
			return q;
		}
		private Quaternion ReverseChiralityZ(Quaternion quat)
		{
			var q = quat;
			q.z = -q.z;
			return q;
		}

		private Quaternion EulerSwizzle(Quaternion quat)
		{
			var q = quat.eulerAngles;
			var z = q.z;
			q.z = q.y;
			q.y = z;

			return Quaternion.Euler(q);
		}

		private Quaternion ReorderAxis(Quaternion rotation)
		{
			//Z Up
			//Y North
			//X East

			//Y Up
			//Z North
			//X East

			Quaternion quat = new Quaternion(rotation.x, rotation.z, rotation.y, rotation.w);
			//outputMsg.pose.orientation.x = msg->pose.orientation.y;
			//outputMsg.pose.orientation.y = msg->pose.orientation.z;
			//outputMsg.pose.orientation.z = msg->pose.orientation.x;
			//outputMsg.pose.orientation.w = msg->pose.orientation.w;

			return quat;
		}
		#endregion
	}
}