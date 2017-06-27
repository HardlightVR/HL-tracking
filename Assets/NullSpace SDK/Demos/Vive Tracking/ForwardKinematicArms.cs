using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class ForwardKinematicArms : MonoBehaviour
{
	//public Vector3 PZero;
	//public Vector3 POne;
	//public float DistanceOne;
	//public float AlphaZero;

	public bool ReportLogs = false;
	public bool RandomizeStart = true;
	public bool RandomizeTarget = false;
	public RobotJoint[] Joints;
	public GameObject Target;

	//[Range(-360, 360)]
	//public float AngleOne;
	//[Range(-360, 360)]
	//public float AngleTwo;
	//[Range(-360, 360)]
	//public float AngleThree;

	public float SamplingDistance = 0.1f;
	public float LearningRate = 0.25f;
	public float DistanceThreshold = .1f;

	public float[] OfficialAngles;

	public float DistanceToTarget;

	public Vector3 LastTarget;

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

		string report = "[" + gameObject.name + " - Original Positions]\n";
		for (int i = 0; i < Joints.Length; i++)
		{
			report += Joints[i].name + "  " + Joints[i].transform.position + "\n";
		}
		Debug.Log(report + "\n");
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
		LastTarget = ForwardKinematics(GetAngles().ToArray());
		//	Debug.Log("Position goes from: " + Joints[0].transform.position + " to " + result.ToString() + "\n");

		if (Target != null)
		{
			DistanceToTarget = Vector3.Distance(Target.transform.position, LastTarget);
			//Debug.Log("Distance to target: " + DistanceToTarget + "\n");

			InverseKinematics(Target.transform.position, GetAngles());
		}
	}

	public float PartialGradient(Vector3 target, float[] angles, int i)
	{
		//Debug.Log("Checking: " + i + " " + angles.Length + "\n");
		// Saves the angle,
		// it will be restored later
		float angle = angles[i];

		// Gradient : [F(x+SamplingDistance) - F(x)] / h
		float f_x = DistanceFromTarget(target, angles);

		angles[i] += SamplingDistance;
		float f_x_plus_d = DistanceFromTarget(target, angles);

		float gradient = (f_x_plus_d - f_x) / SamplingDistance;

		// Restores
		angles[i] = angle;

		return gradient;
	}

	public void InverseKinematics(Vector3 target, float[] angles)
	{
		if (DistanceFromTarget(target, angles) < DistanceThreshold)
			return;

		for (int i = Joints.Length - 1; i >= 0; i--)
		{
			//Debug.Log("Checking: " + i + " " + Joints.Length + "\n" + angles.Length);
			// Gradient descent
			// Update : Solution -= LearningRate * Gradient
			if (i < Joints.Length - 1)
			{
				float gradient = PartialGradient(target, angles, i);
				OfficialAngles[i] -= LearningRate * gradient;

				// Clamp
				//angles[i] = Mathf.Clamp(angles[i], Joints[i].MinAngle, Joints[i].MaxAngle);

				// Early termination
				if (DistanceFromTarget(target, angles) < DistanceThreshold)
					return;
			}
		}
	}

	//To solve inverse kinematics we need to minimise the value returned here.
	public float DistanceFromTarget(Vector3 target, float[] angles)
	{
		Vector3 point = ForwardKinematics(angles);

		//We will use gradient descent here to work towards that.
		return Vector3.Distance(point, target);
	}

	public Vector3 ForwardKinematics(float[] angles, int depth = int.MaxValue)
	{
		//Start with the position of the first joint
		Vector3 prevPoint = Vector3.zero;
		//Vector3 prevPoint = Joints[0].transform.position;
		Quaternion rotation = Quaternion.identity;

		string reportAngles = "";
		//For each required joint
		for (int i = 1; i < Joints.Length && i < depth; i++)
		{
			// Rotates around that joint's axis
			rotation *= Quaternion.AngleAxis(angles[i - 1], Joints[i - 1].Axis);

			float ang = angles[i - 1];

			reportAngles = (reportAngles) + (ang) + ("\t ") + rotation.ToString() + ("\n ");
			//Calculate the point of this joint based on the previous point + the rotation offset start offset.
			Vector3 ThisJointsRotatedOffset = rotation * Joints[i].StartOffset;
			Vector3 nextPoint = prevPoint + ThisJointsRotatedOffset;

			prevPoint = nextPoint;

			if (Application.isPlaying)
			{
				reportAngles += "name: " + Joints[i].name + "\t" + Joints[i].transform.position;
				//Vector3 diff = Joints[i].transform.localPosition - prevPoint;
				Joints[i].transform.rotation = rotation;
				//Joints[i].transform.localPosition = ThisJointsRotatedOffset;
			}
			//Debug.Log(prevPoint + "\n");	
			//Joints[i].transform.position = nextPoint;

		}
		if (ReportLogs)
			Debug.Log(reportAngles + "\n");

		//return prevPoint;
		return Joints[0].transform.position + prevPoint;
	}

	public float[] GetAngles()
	{
		//List<float> angles = new List<float>();
		//angles.Add(AngleOne);
		//angles.Add(AngleTwo);
		//angles.Add(AngleThree);
		//angles.Add(AngleFour);
		//angles.Add(AngleFive);
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
						start = ForwardKinematics(GetAngles(), i + 1);
						end = !atEnd ? start : ForwardKinematics(GetAngles(), i + 2);



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

		if (Target != null)
		{
			Gizmos.color = new Color(.8f, .7f, 0.0f, 1.0f);
			Gizmos.DrawSphere(Target.transform.position, .025f);
			if(ShouldJointDraw)
				Gizmos.DrawSphere(Target.transform.position + visualizeOffset, .025f);
		}

		Gizmos.DrawSphere(LastTarget + Vector3.right * .03f, .01f);
		Gizmos.DrawSphere(LastTarget - Vector3.right * .03f, .01f);
		Gizmos.DrawSphere(LastTarget + Vector3.up * .03f, .01f);
		Gizmos.DrawSphere(LastTarget - Vector3.up * .03f, .01f);
		Gizmos.DrawSphere(LastTarget + Vector3.forward * .03f, .01f);
		Gizmos.DrawSphere(LastTarget - Vector3.forward * .03f, .01f);
		Gizmos.DrawSphere(LastTarget, .01f);

		Gizmos.DrawLine(LastTarget, Target.transform.position);
	}

}