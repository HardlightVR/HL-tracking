using UnityEngine;

public class RobotJoint : MonoBehaviour
{
	public Vector3 Axis = Vector3.right;
	public Vector3 StartOffset;

	//Not used, commented out in ForwardKinematicArms
	public float MinAngle = -180;
	public float MaxAngle = 180;

	public float AngleFromUp;
	public float AngleFromRight;
	public float AngleFromComfort;
	public Vector3 ComfortUp = Vector3.zero;
	public Vector3 ComfortRight = Vector3.zero;

	public Color myColor = Color.white;
	public MeshRenderer rend;

	public float JointDiscSize = .05f;
	public float JointGizmoSize = .1f;

	public bool DrawJointGizmos = true;

	void Awake()
	{
		StartOffset = transform.localPosition;
		if (rend != null)
			rend.material.color = myColor;
	}

	void Update()
	{
		if (ComfortUp == Vector3.zero || ComfortRight == Vector3.zero)
		{
			AngleFromRight = 0;
			AngleFromUp = 0;
		}
		else
		{
			AngleFromRight = Vector3.Angle(ComfortRight, transform.right);
			AngleFromUp = Vector3.Angle(ComfortUp, transform.up);
		}

		AngleFromComfort = AngleFromRight + AngleFromUp;
	}

	void OnDrawGizmos()
	{
		if (DrawJointGizmos)
			DrawWireframe();
	}

	void DrawWireframe()
	{
		//Start at transform
		//Draw 6 circles on the axis of rotation.

#if UNITY_EDITOR
		Vector3 LocalAxis = transform.rotation * Axis;
		UnityEditor.Handles.color = myColor;
		UnityEditor.Handles.DrawDottedLine(transform.position - LocalAxis * JointGizmoSize, transform.position + LocalAxis * JointGizmoSize, 5);

		UnityEditor.Handles.DrawWireDisc(transform.position, LocalAxis, JointDiscSize);

		Gizmos.color = myColor;
		//Gizmos.DrawSphere(transform.position, .01f);
		if (ComfortUp != Vector3.zero)
		{
			Gizmos.color = myColor - new Color(0, 0, 0, .25f);
			Vector3 comfortPos = transform.position + ComfortUp * JointGizmoSize + Axis * .005f;
			Vector3 upPos = transform.position + transform.up * JointGizmoSize * 1.25f + Axis * .005f;
			Gizmos.DrawSphere(comfortPos, .01f);
			Gizmos.DrawSphere(upPos, .015f);

			UnityEditor.Handles.DrawLine(comfortPos, upPos);
			//UnityEditor.Handles.DrawLine(transform.position + Vector3.forward * .01f, transform.position + transform.up * JointGizmoSize * 1.5f + Vector3.forward * .01f);
			//UnityEditor.Handles.DrawLine(transform.position + Vector3.forward * .02f, transform.position + ComfortUp * JointGizmoSize * 2 + Vector3.forward * .01f);
		}
#endif
	}

}