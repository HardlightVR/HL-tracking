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
	[Range(1, 3)]
	public int IKSampleRate = 4;
	public float LearningRate = 300.0f;
	public float DistanceThreshold = .1f;

	[Header("Randomization")]
	public bool RandomizeStart = true;
	public bool RandomizeTarget = false;

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
		ForwardKinematicsResult = ForwardKinematics(GetAngles().ToArray());
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

		// Gradient : [F(x+SamplingDistance) - F(x)] / h
		float f_x = DistanceFromTarget(target, angles);

		angles[angleIndex] += SamplingDistance;
		float f_x_plus_d = DistanceFromTarget(target, angles);

		float gradient = (f_x_plus_d - f_x) / SamplingDistance;

		// Restores
		angles[angleIndex] = angle;

		return gradient;
	}

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

	//To solve inverse kinematics we need to minimise the value returned here.
	public float DistanceFromTarget(Vector3 target, float[] angles, bool primaryCall = false)
	{
		Vector3 point = ForwardKinematics(angles, primaryCall);

		//We will use gradient descent here to work towards that.
		return Vector3.Distance(point, target);
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

	void OnDrawGizmos()
	{
		Gizmos.color = Color.black;
		Vector3 start = Vector3.zero;
		Vector3 end = Vector3.zero;
		bool atEnd = false;

		bool ShouldJointDraw = false;
		#region Joint Draw
		if (ShouldJointDraw)
		{
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
		#endregion

		#region Target
		if (Target != null)
		{
			Gizmos.color = new Color(.8f, .7f, 0.0f, 1.0f);
			Gizmos.DrawSphere(Target.transform.position, .025f);
			if (ShouldJointDraw)
				Gizmos.DrawSphere(Target.transform.position + visualizeOffset, .025f);
		}
		#endregion

		#region Last Target Sphere Cluster
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
		#endregion
	}
}