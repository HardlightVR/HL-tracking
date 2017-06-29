using UnityEngine;

public class RobotJoint : MonoBehaviour
{
	public Vector3 Axis = Vector3.right;
	public Vector3 StartOffset;

	//Not used, commented out in ForwardKinematicArms
	public float MinAngle;
	public float MaxAngle;

	public Color myColor = Color.white;
	public MeshRenderer rend;

	public float JointGizmoSize = .1f;

	public bool DrawAxisOfRotation = true;

	void Awake()
	{
		StartOffset = transform.localPosition;
		if (rend != null)
			rend.material.color = myColor;
	}

	void OnDrawGizmos()
	{
		if (DrawAxisOfRotation)
			DrawWireframe();
	}

	void DrawWireframe()
	{
		//Start at transform
		//Draw 6 circles on the axis of rotation.

#if UNITY_EDITOR
		Vector3 LocalAxis = transform.rotation * Axis;
		UnityEditor.Handles.color = new Color(myColor.r, myColor.g, myColor.b, 1f);
		UnityEditor.Handles.DrawDottedLine(transform.position - LocalAxis * JointGizmoSize, transform.position +LocalAxis * JointGizmoSize, 5);
		UnityEditor.Handles.DrawWireDisc(transform.position, LocalAxis, JointGizmoSize);

		Gizmos.color = new Color(myColor.r, myColor.g, myColor.b, 1f);
		Gizmos.DrawSphere(transform.position, .01f);
#endif



	}

}