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

		public GameObject visualParent;

		public float aboveAmt = .6f;

		[Header("Runtime calculated")]
		public Quaternion OffsetCalibration;

		public FrameOfReferenceDisplay absolute;
		public FrameOfReferenceDisplay IMU;
		public FrameOfReferenceDisplay Headset;
		public FrameOfReferenceDisplay offsetFromHeadset;
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

		public FrameOfReferenceDisplay calculatedOffsetDisplay;
		public FrameOfReferenceDisplay appliedOffsetDisplay;

		public FrameOfReferenceDisplay imuToAbsolute;
		public FrameOfReferenceDisplay absoluteToIMU;
		public FrameOfReferenceDisplay ViveSpaceOffset;

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
			visualParent = new GameObject("Visual Parent [" + name + "]");
			var tr = visualParent.transform;
			//CalibratedIMU = new FrameOfReferenceDisplay(prefab,
			//	"Calibrated IMU", tr, 0.5f,						AbsoluteObject);

			Headset = new FrameOfReferenceDisplay(prefab,
				"Headset", tr, 0.0f, VRMimic.Instance.VRCamera.transform);
			IMU = new FrameOfReferenceDisplay(prefab,
				"Display IMU", tr, aboveAmt + 1.0f,				AbsoluteObject);
			absolute = new FrameOfReferenceDisplay(prefab,
				"Display Absolute", tr, aboveAmt + 1.5f,		AbsoluteObject);
			imuToAbsolute = new FrameOfReferenceDisplay(prefab,
				"IMU to Absolute", tr, aboveAmt + 2.0f,			AbsoluteObject);
			IMU2 = new FrameOfReferenceDisplay(prefab,
				"Display IMU2", tr, 0.5f,						AbsoluteObject);
			offsetFromHeadset = new FrameOfReferenceDisplay(prefab,
				"Headset Based", tr, 0.1f,						AbsoluteObject);
			absoluteToIMU = new FrameOfReferenceDisplay(prefab,
				"Absolute to IMU", tr, aboveAmt + 3.0f,			AbsoluteObject);
			calculatedOffsetDisplay = new FrameOfReferenceDisplay(prefab,
				"Calculated Offset", tr, aboveAmt + 3.5f,		AbsoluteObject);
			appliedOffsetDisplay = new FrameOfReferenceDisplay(prefab,
				"Applied Offset", tr, aboveAmt + 4.0f,			AbsoluteObject);
			ViveSpaceOffset = new FrameOfReferenceDisplay(prefab,
				"Vive to IMU", tr, aboveAmt + 5.0f,				AbsoluteObject);

			#region Old Display Frames
			//DisplayA = new FrameOfReferenceDisplay(prefab, "Display A = (Abs)^-1 * IMU", visuals.transform, aboveAmt + 0.7f, AbsoluteObject);
			//DisplayB = new FrameOfReferenceDisplay(prefab, "Display B = (IMU)^-1 * Abs", visuals.transform, aboveAmt + 1.05f, AbsoluteObject);
			//DisplayB = new FrameOfReferenceDisplay(prefab, "Display B = Euler subtraction", visuals.transform, aboveAmt + 1.5f, AbsoluteObject);
			//DisplayC = new FrameOfReferenceDisplay(prefab, "Display C = Vector3 Conversion", visuals.transform, aboveAmt + 2.0f, AbsoluteObject);
			//DisplayD = new FrameOfReferenceDisplay(prefab, "Display D = Calibrated IMU", visuals.transform, aboveAmt + 2.5f, AbsoluteObject);
			//DisplayEulerSwizzle = new FrameOfReferenceDisplay(prefab, "Display E = Euler Swizzle", visuals.transform, aboveAmt + 1.0f, AbsoluteObject);
			//DisplayF = new FrameOfReferenceDisplay(prefab, "Display F = ChiralReverse(EulerSwizzle(IMU))", visuals.transform, aboveAmt + 1.5f, AbsoluteObject);
			////DisplayG = new FrameOfReferenceDisplay(prefab, "Display G = Subtract(ChiralReverse(EulerSwizzle(IMU)))", visuals.transform, aboveAmt + 2.0f, AbsoluteObject);
			//DisplayH = new FrameOfReferenceDisplay(prefab, "Display H = Subtract(EulerSwizzle(ChiralReverse(IMU)))", visuals.transform, aboveAmt + 2.0f, AbsoluteObject);
			//DisplayReverseChiral = new FrameOfReferenceDisplay(prefab, "Display Chir = ChiralReverse(IMU)", visuals.transform, aboveAmt + 2.5f, AbsoluteObject);
			//AxisReorg = new FrameOfReferenceDisplay(prefab, "IMU Data Axis Reorg", visuals.transform, aboveAmt + 4.0f, AbsoluteObject); 
			#endregion

			Result = Quaternion.identity;
			OffsetFromRight = Quaternion.identity;
			OffsetFromUp = Quaternion.identity;

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
		[Header("Manual Offset")]
		public Vector3 angleRots = new Vector3(-90, -45, 180);

		public bool reverseX = false;
		public bool reverseY = false;
		public bool reverseZ = false;
		public bool reverseW = false;
		public bool inverse = false;

		public Quaternion SimpleQuatOffset = Quaternion.identity;

		void Update()
		{
			if (AbsoluteObject && IMUObject)
			{
				Headset.Update(VRMimic.Instance.VRCamera.transform.rotation);
				north = IMUObject.transform.up;
				rotateToNorth = Vector3.Angle(north, Vector3.forward);

				var visualAlignmentOffset = Quaternion.AngleAxis(65, Vector3.up) * Quaternion.AngleAxis(90, Vector3.right);
				var movementAlignmentOffset = Quaternion.AngleAxis(180 + 45, Vector3.up) * Quaternion.AngleAxis(90, Vector3.right);
				toNorth = Quaternion.AngleAxis(rotateToNorth, Vector3.up);
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
				var handAssignedChiraltyOffset = Quaternion.AngleAxis(-315, Vector3.up) * Quaternion.AngleAxis(-90, Vector3.right);
				var xRot = Quaternion.AngleAxis(angleRots.x, Vector3.right);
				var yRot = Quaternion.AngleAxis(angleRots.y, Vector3.up);
				var zRot = Quaternion.AngleAxis(angleRots.z, Vector3.forward);

				//var offsetQuat = zRot * yRot * xRot;
				var offsetQuat = SimpleQuatOffset;
				//var offsetQuat = SimpleQuatOffset * IMUObject.rotation;
				if (reverseX)
					offsetQuat = ReverseChiralityX(offsetQuat);
				if (reverseY)
					offsetQuat = ReverseChiralityY(offsetQuat);
				if (reverseZ)
					offsetQuat = ReverseChiralityZ(offsetQuat);
				if (reverseW)
					offsetQuat = ReverseChiralityW(offsetQuat);
				if (inverse)
					offsetQuat = Quaternion.Inverse(offsetQuat);

				Thing();
				//imuToAbsolute.Update(ReverseChiralityXZ(zRot * yRot * xRot) * IMUObject.rotation);
				var headsetQuat = SimpleQuatOffset;
				var imuRotForHeadsetDisplay = IMUObject.rotation;
				headsetQuat = invertHeadsetOffset ? Quaternion.Inverse(SimpleQuatOffset) : headsetQuat;
				imuRotForHeadsetDisplay = reverseChiralityOfHeadsetOffset ? ReverseChiralityXZ(imuRotForHeadsetDisplay) : imuRotForHeadsetDisplay;
				offsetFromHeadset.Update(headsetQuat * imuRotForHeadsetDisplay);
				//imuToAbsolute.Update(ReverseChiralityXZ(zRot * yRot * xRot) * IMUObject.rotation);

				//Debug.Log("RevChirXZ(ZYX)   : " + ReverseChiralityXZ(zRot * yRot * xRot) + "\t\t\tRevChirXZ(ZYX)^-1: " + Quaternion.Inverse(ReverseChiralityXZ(zRot * yRot * xRot)) + "\n" +
				//	"RevChirXZ(XYZ)   : " + ReverseChiralityXZ(xRot * yRot * zRot) + "\t\t\tRevChirXZ(XYZ)^-1: " + Quaternion.Inverse(ReverseChiralityXZ(xRot * yRot * zRot)) + "\n");

				//imuToAbsolute.Update(offsetQuat * IMUObject.rotation);



				//ViveSpaceOffset.Update(IMUObject.rotation);
				//imuToAbsolute.Update(IMUObject.rotation * offsetQuat);
				//imuToAbsolute.Update(offsetQuat);
				//imuToAbsolute.Update(ReverseChiralityXZ(handAssignedChiraltyOffset * yHalfRotation * IMUObject.rotation));
				//imuToAbsolute.Update(ReverseChiralityXZ(handAssignedChiraltyOffset * yHalfRotation * IMUObject.rotation));

				//ManualCalibration();

				Result = OffsetFromRight * OffsetFromUp;

				//calibratedQuat = IMUObject.rotation;
				//var thing = Subtract(Quaternion.Inverse(handAssignedChiraltyOffset) * AbsoluteObject.rotation, IMUObject.rotation);
				//CalibratedIMU.Update(thing * IMUObject.rotation);

				//CalibratedIMU.Update(Quaternion.Inverse(calibratedQuat * IMUObject.rotation));
				//DrawThingy(IMU.pos + Vector3.right * .65f + Vector3.forward * .65f, Result);
				//absoluteToIMU.Update(Quaternion.Inverse(handAssignedChiraltyOffset) * ReverseChiralityXZ(AbsoluteObject.rotation));

				//absoluteToIMU.Update(Quaternion.Inverse(handAssignedChiraltyOffset) * ReverseChiralityXZ(AbsoluteObject.rotation));

				//Old golden case for watching the calculated offset
				//var calculatedOffset = Subtract(Quaternion.Inverse(handAssignedChiraltyOffset) * ReverseChiralityXZ(AbsoluteObject.rotation), IMUObject.rotation);
				var handOffset = Quaternion.AngleAxis(-315, Vector3.up) * Quaternion.AngleAxis(-90, Vector3.right);
				//var otherHandOffset = Quaternion.AngleAxis(315, Vector3.up) * Quaternion.AngleAxis(-90, Vector3.right);
				//var thingy = Quaternion.AngleAxis(-315, -Vector3.up) * Quaternion.AngleAxis(-90, Vector3.right);
				//var thingTwo = Quaternion.AngleAxis(135, -Vector3.up) * Quaternion.AngleAxis(-90, Vector3.right);

				//				float angle  Vector3 vector 
				//Quaternion.AngleAxis(-angle, -vector) == Quaternion.AngleAxis(angle, vector)

				//PrintInverse(handOffset);
				//PrintInverse(handAssignedChiraltyOffset);
				//PrintInverse(otherHandOffset);
				//PrintInverse(thingy);
				//PrintInverse(thingTwo);


				var calculatedOffset = Subtract(Quaternion.Inverse(handOffset) * ReverseChiralityXZ(AbsoluteObject.rotation), IMUObject.rotation);

				#region Old pre-chiralty removal
				////calibratedQuat = IMUObject.rotation;
				//var thing = Subtract(Quaternion.Inverse(chiralityOffset) * AbsoluteObject.rotation, IMUObject.rotation);
				//CalibratedIMU.Update(Result * IMUObject.rotation);

				////CalibratedIMU.Update(Quaternion.Inverse(calibratedQuat * IMUObject.rotation));
				//DrawThingy(IMU.pos + Vector3.right * .65f + Vector3.forward * .65f, Result);
				//absoluteToIMU.Update(Quaternion.Inverse(chiralityOffset) * ReverseChiralityXZ(AbsoluteObject.rotation));

				////imuToAbsolute.Update(ReverseChiralityXZ(IMUObject.rotation));
				//absoluteToIMU.Update(Quaternion.Inverse(chiralityOffset) * ReverseChiralityXZ(AbsoluteObject.rotation));

				////Old golden case for watching the calculated offset
				//var calculatedOffset = Subtract(Quaternion.Inverse(chiralityOffset) * ReverseChiralityXZ(AbsoluteObject.rotation), IMUObject.rotation);

				//var calculatedOffset = Subtract(Quaternion.Inverse(chiralityOffset) * (AbsoluteObject.rotation), IMUObject.rotation); 
				#endregion
				calculatedOffsetDisplay.Update(calculatedOffset);
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

		[Header("IMU Space to Vive Space Offset")]
		public Vector3 north;
		public float rotateToNorth;
		public Quaternion toNorth;

		[Header("Headset IMU Calibration Parameters")]
		public bool invertHeadsetOffset;
		public bool reverseChiralityOfHeadsetOffset;

		private void Thing()
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				SimpleQuatOffset = Subtract(IMUObject.rotation, VRMimic.Instance.VRCamera.transform.rotation);
				Debug.Log("Quat Sub: " + IMUObject.rotation.ToString() + "  from " + VRMimic.Instance.VRCamera.transform.rotation + "\n\tResult: " + SimpleQuatOffset);
			}
		}

		private void PrintInverse(Quaternion quat)
		{
			Debug.Log("Base Quat: " + quat.ToString() + "\n" + "Invs Quat: " + Quaternion.Inverse(quat).ToString() + "\n\n");
		}

		//[Header("Saved Calibration Vectors")]
		//public Vector3 ForwardCase;
		//public Vector3 UpCase;
		//public Vector3 RightCase;

		public Quaternion CreateQuatFromVectors(Vector3 v1, Vector3 v2)
		{
			Quaternion quat;
			var cross = Vector3.Cross(v1, v2);
			quat.x = cross.x;
			quat.y = cross.y;
			quat.z = cross.z;
			quat.w = Mathf.Sqrt((v1.magnitude * v1.magnitude) * (v2.magnitude * v2.magnitude)) + Vector3.Dot(v1, v2);
			quat = quat.Normalize();
			return quat;
		}

		public bool CalcCalibrated = true;
		void ManualCalibration()
		{
			#region Input
			Debug.DrawLine(AbsoluteObject.transform.position, AbsoluteObject.transform.position + IMUObject.transform.right * .1f, Color.red);
			Debug.DrawLine(AbsoluteObject.transform.position, AbsoluteObject.transform.position + IMUObject.transform.up * .1f, Color.green);
			Debug.DrawLine(AbsoluteObject.transform.position, AbsoluteObject.transform.position + IMUObject.transform.forward * .1f, Color.blue);
			//if (Input.GetKeyDown(KeyCode.R))
			//{
			//	RightQuaternion = IMUObject.rotation;
			//	//OffsetFromRight = Quaternion.identity;
			//	//OffsetFromRight = Subtract(OffsetFromRight, RightQuaternion);
			//	OffsetFromRight = Subtract(RightQuaternion, VRMimic.Instance.VRCamera.transform.rotation);

			//	RightCase = IMUObject.transform.right;
			//	Debug.DrawLine(AbsoluteObject.transform.position, AbsoluteObject.transform.position + IMUObject.transform.right * .1f, Color.red, 1000);
			//}
			//if (Input.GetKeyDown(KeyCode.Y))
			//{
			//	UpQuaternion = IMUObject.rotation;
			//	//OffsetFromUp = Quaternion.identity;
			//	//OffsetFromUp = Subtract(OffsetFromUp, UpQuaternion);
			//	OffsetFromUp = Subtract(UpQuaternion, VRMimic.Instance.VRCamera.transform.rotation);
			//	UpCase = IMUObject.transform.up;
			//	Debug.DrawLine(AbsoluteObject.transform.position, AbsoluteObject.transform.position + IMUObject.transform.up * .1f, Color.green, 1000);
			//}
			//if (Input.GetKeyDown(KeyCode.F))
			//{
			//	ForwardCase = IMUObject.transform.forward;
			//	Debug.DrawLine(AbsoluteObject.transform.position, AbsoluteObject.transform.position + IMUObject.transform.forward * .1f, Color.blue, 1000);
			//}
			//if (RightCase != Vector3.zero && UpCase != Vector3.zero && ForwardCase != Vector3.zero && CalcCalibrated)
			//{
			//	CalcCalibrated = false;
			//	calibratedQuat = CreateQuatFromVectors(RightCase, UpCase);
			//}
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
			#endregion
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
		public Quaternion calibratedQuat;

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
		private Quaternion ReverseChiralityW(Quaternion quat)
		{
			var q = quat;
			q.w = -q.w;
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