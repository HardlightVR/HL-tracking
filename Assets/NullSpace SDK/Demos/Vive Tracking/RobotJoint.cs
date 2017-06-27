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

	void Awake()
	{
		StartOffset = transform.localPosition;
		if (rend != null)
			rend.material.color = myColor;
	}

}