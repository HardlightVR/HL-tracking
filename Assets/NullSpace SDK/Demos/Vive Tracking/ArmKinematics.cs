using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ArmKinematics : MonoBehaviour
{
	private bool ReportLogs = false;

	[Header("Update Specifics")]
	public bool UpdateRegardlessOfArmMimic = false;
	public bool ApplyForwardKinematics = true;
	public bool ApplyInverseKinematics = true;
	public bool ApplyJointClamping = true;
	public bool UseRotationBase = false;
	public bool ShouldDrawKinematicGizmos = true;
	public bool UseOrientationFitness = false;
	[Range(0, .1f)]
	public float EndManipulatorGizmoSize = .02f;

	public Transform RotationBase;

	[Header("Kinematic Locations")]
	public RobotJoint[] Joints;
	public float[] OfficialAngles;
	public Vector3[] Offsets;

	[Header("Target Information")]
	public GameObject Target;
	public Vector3 ForwardKinematicsResult;
	public Vector3 LastTargetPosition;
	public float DistanceToTarget;
	public float WorldDistanceToTarget;

	[Header("IK Parameters")]
	public float SamplingDistance = 0.25f;
	public float OrientationSamplingDistance = 0.25f;
	[Range(1, 3)]
	public int IKSampleRate = 4;
	public float LearningRate = 300.0f;
	public float DistanceThreshold = .1f;

	[Header("Weights")]
	[Range(0.00001f, 1.0f)]
	public float PositionalWeight = 1.0f;
	[Range(0.00001f, 1.0f)]
	public float OrientationWeight = .75f;

	[Header("Randomization")]
	public bool RandomizeStart = true;
	public bool RandomizeTarget = false;

	[SerializeField]
	public KinematicInfo CurrentResults;

	public void Start()
	{
		if (Target == null)
		{
			Target = new GameObject();
			Target.name = "Forward Kinematics Target [Runtime Created]";
		}

		if (RandomizeTarget)
		{
			Target.transform.position = Joints[0].transform.position + Random.insideUnitSphere;
		}
		if (RandomizeStart)
		{
			if (Joints != null)
			{
				OfficialAngles = new float[Joints.Length - 1];
				for (int i = 0; i < OfficialAngles.Length; i++)
				{
					OfficialAngles[i] = Random.Range(-360.0f, 360.0f);
				}
			}
		}

		Offsets = new Vector3[Joints.Length];

		//string report = "[" + gameObject.name + " - Original Positions]\n";
		//for (int i = 0; i < Joints.Length; i++)
		//{
		//	report += Joints[i].name + "  " + Joints[i].transform.position + "\n";
		//}
		//Debug.Log(report + "\n");
	}

	//Calculate Step:
	//POne = PZero + rotate(DistanceOne, PZero, alphaZero)
	//PTwo = POne + rotate(DistanceTwo, POne, alphaOne)

	//General Equation
	// P[i-1] + rotate (D[i], P[i-1], Sigma [i-1, k=0] alpha[k] )

	//private class DegreeOfFreedom
	//{
	//	public DegreeOfFreedom Before;
	//	public DegreeOfFreedom After;
	//}

	public void Update()
	{
		if (UpdateRegardlessOfArmMimic && !Application.isPlaying)
		{
			UpdateKinematics();
		}
	}
	/// <summary>
	/// Called in ArmMimic.Update()
	/// </summary>
	public void UpdateKinematics()
	{
		//Debug.Log("Updating: " + name + " FK: " + ApplyForwardKinematics + " IK: " + ApplyInverseKinematics + "\n", this);
		if (isActiveAndEnabled)
		{
			if (ApplyForwardKinematics)
			{
				//Debug.Log("Updating [" + name + "] forward kinematics\n");
				HandleForwardKinematics();
			}

			if (ApplyInverseKinematics && Application.isPlaying)
			{
				//Debug.Log("Updating [" + name + "] inverse kinematics\n");
				HandleInverseKinematics();
			}
		}

		UpdateWorldDistanceToTarget();
		SimplifyAngleValues();
	}

	public void HandleForwardKinematics()
	{
		CurrentResults = FwdKinematics(GetAngles());
		//Debug.Log(CurrentResults.ToString());

		ForwardKinematicsResult = ForwardKinematics(GetAngles());
		LastTargetPosition = Joints[Joints.Length - 1].transform.position;
		//	Debug.Log("Position goes from: " + Joints[0].transform.position + " to " + result.ToString() + "\n");
	}

	public void HandleInverseKinematics()
	{
		if (Target != null)
		{
			DistanceToTarget = Vector3.Distance(Target.transform.position, Joints[Joints.Length - 1].transform.position);

			for (int i = 0; i < IKSampleRate; i++)
			{
				InverseKinematics(Target.transform.position, GetAngles());
			}
		}
	}

	public KinematicInfo FwdKinematics(float[] angles, bool primaryCall = false, int depth = int.MaxValue)
	{
		KinematicInfo info = new KinematicInfo(angles, depth);
		//Start with the position of the first joint
		Vector3 prevPoint = Vector3.zero;

		Vector3 prevUp = Vector3.up;
		Vector3 prevRight = Vector3.right;
		Vector3 nextUp = Vector3.up;
		Vector3 nextRight = Vector3.right;

		//Vector3 prevPoint = Joints[0].transform.position;
		Quaternion rotation = Quaternion.identity;

		string reportAngles = "";
		//For each required joint
		for (int i = 1; i < Joints.Length && i < depth; i++)
		{
			//Make sure we're using the CORRECT current index
			int currentIndex = i - 1;

			// Rotates around that joint's axis
			rotation *= Quaternion.AngleAxis(angles[currentIndex], Joints[currentIndex].Axis);

			//Calculate the point of this joint based on the previous point + the rotation offset start offset.
			Vector3 ThisJointsRotatedOffset = rotation * Joints[i].StartOffset;
			Vector3 nextPoint = prevPoint + ThisJointsRotatedOffset;
			nextUp = rotation * prevUp;
			nextRight = rotation * prevRight;

			if (Offsets != null && Offsets.Length > i)
			{
				reportAngles += "[" + Joints[currentIndex].name + "] - assigned " + Joints[currentIndex].StartOffset + "  " + Joints[currentIndex].transform.localPosition + "\n";
				Offsets[currentIndex] = Joints[currentIndex].StartOffset;
			}

			prevPoint = nextPoint;
			prevUp = nextUp;
			prevRight = nextRight;

			info.Set(i, nextPoint, nextUp, nextRight, Joints[i]);

			if (RotationBase != null && UseRotationBase)
			{
				Joints[currentIndex].transform.rotation = RotationBase.transform.rotation * rotation;
			}
			else
			{
				Joints[currentIndex].transform.rotation = rotation;
			}
		}

		if (ReportLogs)
			Debug.Log(reportAngles + "\n");

		//return Joints[0].transform.position + prevPoint;
		return info;
	}

	public Vector3 ForwardKinematics(float[] angles, bool primaryCall = false, int depth = int.MaxValue)
	{
		//Start with the position of the first joint
		Vector3 prevPoint = Vector3.zero;
		//Vector3 prevPoint = Joints[0].transform.position;
		Quaternion rotation = Quaternion.identity;

		string reportAngles = "";
		//For each required joint
		for (int i = 1; i < Joints.Length && i < depth; i++)
		{
			//Make sure we're using the CORRECT current index
			int currentIndex = i - 1;

			// Rotates around that joint's axis
			rotation *= Quaternion.AngleAxis(angles[currentIndex], Joints[currentIndex].Axis);

			//Calculate the point of this joint based on the previous point + the rotation offset start offset.
			Vector3 ThisJointsRotatedOffset = rotation * Joints[i].StartOffset;
			Vector3 nextPoint = prevPoint + ThisJointsRotatedOffset;

			if (Offsets != null && Offsets.Length > i)
			{
				reportAngles += "[" + Joints[currentIndex].name + "] - assigned " + Joints[currentIndex].StartOffset + "  " + Joints[currentIndex].transform.localPosition + "\n";
				Offsets[currentIndex] = Joints[currentIndex].StartOffset;
			}

			prevPoint = nextPoint;

			if (RotationBase != null && UseRotationBase)
			{
				Joints[currentIndex].transform.rotation = RotationBase.transform.rotation * rotation;
			}
			else
			{
				Joints[currentIndex].transform.rotation = rotation;
			}
		}

		if (ReportLogs)
			Debug.Log(reportAngles + "\n");

		return Joints[0].transform.position + prevPoint;
	}

	#region Inverse Kinematics
	public void InverseKinematics(Vector3 target, float[] angles)
	{
		if (DistanceFromTarget(target, angles) < DistanceThreshold)
		{
			return;
		}

		//Start at the end of the joints and work backwards (core part of INVERSE Kinematics)
		for (int i = Joints.Length - 1; i >= 0; i--)
		{
			// Gradient descent
			// Update : Solution -= LearningRate * Gradient
			if (i < Joints.Length - 1)
			{
				float gradient = PartialGradient(target, angles, i);
				OfficialAngles[i] -= LearningRate * gradient;

				if (ApplyJointClamping)
				{
					angles[i] = Mathf.Clamp(angles[i], Joints[i].MinAngle, Joints[i].MaxAngle);
				}

				// Early termination
				if (DistanceFromTarget(target, angles) < DistanceThreshold)
					return;
			}
		}
	}
	public float PartialGradient(Vector3 target, float[] angles, int angleIndex)
	{
		//Debug.Log("Checking: " + i + " " + angles.Length + "\n");
		// Saves the angle,
		// it will be restored later
		float angle = angles[angleIndex];

		//Evaluate before
		KinematicInfo beforeInfo = FwdKinematics(angles);
		float f_x = DistanceFromTarget(target, beforeInfo, angleIndex);
		float f_y = CalculateOrientationAnglesForFullArm(Vector3.up, beforeInfo, angleIndex);

		angles[angleIndex] += SamplingDistance;

		KinematicInfo afterInfo = FwdKinematics(angles);
		float f_x_plus_d = DistanceFromTarget(target, afterInfo, angleIndex);
		float f_y_plus_d = CalculateOrientationAnglesForFullArm(Vector3.up, afterInfo, angleIndex);

		// Gradient : [F(x+SamplingDistance) - F(x)] / h
		//float f_x = DistanceFromTarget(target, angles);
		//float f_y = CalculateOrientationAnglesForFullArm(Vector3.up, angles);

		//angles[angleIndex] += SamplingDistance;
		//float f_x_plus_d = DistanceFromTarget(target, angles);
		//float f_y_plus_d = CalculateOrientationAnglesForFullArm(Vector3.up, angles);

		float distanceGradient = (f_x_plus_d - f_x) / SamplingDistance;
		float orientationGradient = (f_y_plus_d - f_y) / SamplingDistance;

		float gradient = distanceGradient;

		if (UseOrientationFitness)
		{
			float percentDistance = distanceGradient * (PositionalWeight / (PositionalWeight + OrientationWeight));
			float percentOrientation = orientationGradient * (OrientationWeight / (PositionalWeight + OrientationWeight));
			percentOrientation = percentOrientation / 100;
			gradient = percentDistance + percentOrientation;

		}
		// Restores
		angles[angleIndex] = angle;

		return gradient;
	}
	public float DistanceFromTarget(Vector3 target, KinematicInfo info, int angleIndex)
	{
		Vector3 point = info.GetLastJoint().Position;

		//We will use gradient descent here to work towards that.
		return Vector3.Distance(point, target);
	}

	public float CalculateOrientationAnglesForFullArm(Vector3 target, KinematicInfo info, int angleIndex)
	{
		return info.SumAnglesFromComfortForEachJoint();
	}

	//To solve inverse kinematics we need to minimise the value returned here.
	public float DistanceFromTarget(Vector3 target, float[] angles, bool primaryCall = false)
	{
		Vector3 point = ForwardKinematics(angles, primaryCall);

		//We will use gradient descent here to work towards that.
		return Vector3.Distance(point, target);
	}

	//Calculates the orientation offset from the target.
	public float CalculateOrientationAnglesForFullArm(Vector3 comfortUp, float[] angles, bool primaryCall = false)
	{
		return 0;
	}
	#endregion

	#region Collapse
	private void SimplifyAngleValues()
	{
		if (ApplyJointClamping)
		{
			for (int i = 0; i < OfficialAngles.Length; i++)
			{
				float currentAngle = Mathf.FloorToInt(OfficialAngles[i]);

				//This lets us keep the angles between 180 to -180
				if (currentAngle > 180)
				{
					float overflow = currentAngle - 180;
					currentAngle = -180 + overflow;
				}
				else if (currentAngle < -180)
				{
					float overflow = currentAngle + 180;
					currentAngle = 180 - overflow;
				}

				OfficialAngles[i] = currentAngle;
			}
		}
	}

	private void UpdateWorldDistanceToTarget()
	{
		WorldDistanceToTarget = Vector3.Distance(Target.transform.position, Joints[Joints.Length - 1].transform.position);
	}

	public float GetMaxArmReach()
	{
		throw new System.Exception("Max arm reach not implemented\n");
	}

	public float[] GetAngles()
	{
		return OfficialAngles.ToArray();
	}

	public Vector3 visualizeOffset = Vector3.forward * .1f;
	#endregion

	#region Gizmos
	void OnDrawGizmos()
	{
		Gizmos.color = Color.black;


		bool ShouldDrawGizmos = false;

		if (ShouldDrawGizmos)
		{
			DrawKinematicInfoGizmos();
			DrawJoints();
			DrawTarget();
			DrawLastTargetSphereCluster();
		}
		DrawEndManipulator();
		DrawEndTarget();
	}

	private void DrawKinematicInfoGizmos()
	{
		Vector3 VerticalOffset = Vector3.up * 1.25f;

		KinematicInfo info = CurrentResults;
		if (enabled && ShouldDrawKinematicGizmos)
		{
			for (int i = 0; i < info.JointCount; i++)
			{
				JointInformation joint = info.GetJointInformation(i);
				Gizmos.color = new Color(1, 1, 1, .5f);
				Gizmos.DrawCube(VerticalOffset + joint.Position + Vector3.up * i * .05f, Vector3.one * .05f);
				Gizmos.color = Color.green;
				Gizmos.DrawRay(VerticalOffset + joint.Position + Vector3.up * i * .05f, joint.UpDirection.normalized);
				Gizmos.color = Color.blue;
				Gizmos.DrawRay(VerticalOffset + joint.Position + Vector3.up * i * .05f, joint.RightDirection.normalized);
			}
		}
	}
	private void DrawJoints()
	{
		Vector3 start = Vector3.zero;
		Vector3 end = Vector3.zero;
		bool atEnd = false;
		if (Joints != null)
		{
			for (int i = 0; i < Joints.Length; i++)
			{
				if (Joints[i] != null)
				{
					Gizmos.color = new Color(Joints[i].myColor.r, Joints[i].myColor.g, Joints[i].myColor.b, 1);

					atEnd = Joints.Length > i + 1;

					//Debug.Log(Joints.Length + "  " + i + "\n" + atEnd);
					start = Joints[i].transform.position;
					end = !atEnd ? start : Joints[i + 1].transform.position;

#if UNITY_EDITOR
					Vector3 LocalAxis = transform.rotation * Joints[i].Axis;
					UnityEditor.Handles.color = Gizmos.color;
					UnityEditor.Handles.DrawDottedLine(start + visualizeOffset, end + visualizeOffset, 3);
					//UnityEditor.Handles.DrawSphere(Joints[i].transform.position, LocalAxis, Joints[i].JointGizmoSize);
					//Gizmos.DrawLine(start + visualizeOffset, end + visualizeOffset);
					Gizmos.DrawSphere(start + visualizeOffset, .05f);
#endif
				}
			}
			for (int i = 0; i < Joints.Length; i++)
			{
				if (Joints[i] != null)
				{
					Gizmos.color = new Color(Joints[i].myColor.r, Joints[i].myColor.g, Joints[i].myColor.b, 1);

					atEnd = Joints.Length > i + 1;
					start = ForwardKinematics(GetAngles(), false, i + 1);
					end = !atEnd ? start : ForwardKinematics(GetAngles(), false, i + 2);

#if UNITY_EDITOR
					Vector3 LocalAxis = transform.rotation * Joints[i].Axis;
					UnityEditor.Handles.color = Gizmos.color;
					UnityEditor.Handles.DrawDottedLine(start + visualizeOffset * 2, end + visualizeOffset * 2, 3);
					//UnityEditor.Handles.DrawSphere(Joints[i].transform.position, LocalAxis, Joints[i].JointGizmoSize);
					//Gizmos.DrawLine(start + visualizeOffset, end + visualizeOffset);
					Gizmos.DrawSphere(start + visualizeOffset * 2, .05f);
#endif
				}
			}
		}
	}
	private void DrawTarget()
	{
		if (Target != null)
		{
			Gizmos.color = new Color(.8f, .7f, 0.0f, 1.0f);
			Gizmos.DrawSphere(Target.transform.position, .025f);
			Gizmos.DrawSphere(Target.transform.position + visualizeOffset, .025f);
		}
	}
	private void DrawLastTargetSphereCluster()
	{
		if (LastTargetPosition != Vector3.zero)
		{
			Gizmos.DrawSphere(LastTargetPosition + Vector3.right * .03f, .01f);
			Gizmos.DrawSphere(LastTargetPosition - Vector3.right * .03f, .01f);
			Gizmos.DrawSphere(LastTargetPosition + Vector3.up * .03f, .01f);
			Gizmos.DrawSphere(LastTargetPosition - Vector3.up * .03f, .01f);
			Gizmos.DrawSphere(LastTargetPosition + Vector3.forward * .03f, .01f);
			Gizmos.DrawSphere(LastTargetPosition - Vector3.forward * .03f, .01f);
			Gizmos.DrawSphere(LastTargetPosition, .01f);

			Gizmos.DrawLine(LastTargetPosition, Target.transform.position);
			Gizmos.color = Color.red;
			//Debug.Log(Joints[Joints.Length - 1].name + "\n");
			Gizmos.DrawLine(Target.transform.position, Joints[Joints.Length - 1].transform.position);
		}
	}
	private void DrawEndManipulator()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawSphere(Joints.Last().transform.position, EndManipulatorGizmoSize);
	}
	private void DrawEndTarget()
	{
		Gizmos.color = Color.yellow;
		if (EndManipulatorGizmoSize > 0)
		{
			Gizmos.DrawLine(Joints.Last().transform.position, Target.transform.position);
		}

		Gizmos.DrawSphere(Target.transform.position, EndManipulatorGizmoSize);
	}
	#endregion

	//Hand in an array of angles & a depth.
	//Get back an object with an array of positions + orientations

	[System.Serializable]
	public class JointInformation
	{
		public Vector3 Position;
		public Vector3 UpDirection;
		public Vector3 RightDirection;
		public RobotJoint jointRef;

		public float GetComfortAngle()
		{
			if (jointRef == null)
				return 0;
			//If we don't have a specified comfort, we are 0 distance from our comfort.
			if (jointRef.ComfortUp == Vector3.zero && jointRef.ComfortRight == Vector3.zero)
				return 0;

			return Vector3.Angle(UpDirection, jointRef.ComfortUp) + Vector3.Angle(RightDirection, jointRef.ComfortRight);
		}

		public override string ToString()
		{
			string output = string.Empty;
			output += "Position: " + Position.ToString() + "  My Up: " + UpDirection.ToString() + "  My Right: " + RightDirection.ToString();
			return output;
		}
	}

	[System.Serializable]
	public class KinematicInfo
	{
		[SerializeField]
		public JointInformation[] jointsInfo;
		[SerializeField]
		public float[] Angles;
		public int JointCount
		{
			get
			{
				if (jointsInfo != null)
				{
					return jointsInfo.Length;
				}
				return 0;
			}
		}
		public float ComfortSum = float.MaxValue;

		public KinematicInfo(float[] angles, int depth)
		{
			Angles = angles;
			int numberOfJoints = Mathf.Min(angles.Length, depth);
			jointsInfo = new JointInformation[numberOfJoints];
			for (int i = 0; i < numberOfJoints; i++)
			{
				jointsInfo[i] = new JointInformation();
				//Debug.Log("Joint at :" + i + " is no longer null\n");
			}
		}

		public JointInformation Set(int index, Vector3 jointPosition, Vector3 jointUp, Vector3 jointRight, RobotJoint jointRef)
		{
			jointsInfo[index].Position = jointPosition;
			jointsInfo[index].UpDirection = jointUp;
			jointsInfo[index].RightDirection = jointRight;
			jointsInfo[index].jointRef = jointRef;

			return jointsInfo[index];
		}
		public JointInformation GetJointInformation(int index)
		{
			return jointsInfo[index];
		}
		public JointInformation GetLastJoint()
		{
			return jointsInfo[jointsInfo.Length - 1];
		}

		public float SumAnglesFromComfortForEachJoint()
		{
			float sumAngle = 0;
			//For each joint
			for (int i = 0; i < JointCount; i++)
			{
				//Look at how it is angled.
				sumAngle += jointsInfo[i].GetComfortAngle();
			}
			ComfortSum = sumAngle;
			return sumAngle;
		}

		public override string ToString()
		{
			string result = "Kinematic Info [" + JointCount + "]\n";
			for (int i = 0; i < JointCount; i++)
			{
				if (jointsInfo[i] != null)
				{
					result += jointsInfo[i].ToString() + "   Angle: " + Angles[i] + "\n";
				}
			}
			return result;
		}
	}
}