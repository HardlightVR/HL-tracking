using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	public class UpperArmMimic : MonoBehaviour
	{
		public float RotationAmt = 0;
		public GameObject UpperArmBody;
		public GameObject UpperArmVisual;
		public GameObject Elbow;
		private Transform child;
		public Vector3 offset = new Vector3(.1f, -.1f, -.75f);
		public HardlightCollider UpperArmCollider;

		[Header("Adjust to control arm tracker orientation")]
		public float zRotation = 0;

		public void AssignSide(ArmSide side)
		{
			if (UpperArmCollider != null)
			{
				if (side == ArmSide.Left)
				{
					if (UpperArmCollider.regionID.ContainsArea(AreaFlag.Upper_Arm_Right))
					{
						UpperArmCollider.MyLocation.Where = UpperArmCollider.MyLocation.Where.AddArea(AreaFlag.Upper_Arm_Left);
						UpperArmCollider.MyLocation.Where = UpperArmCollider.MyLocation.Where.RemoveArea(AreaFlag.Upper_Arm_Right);
					}
				}
				else if (side == ArmSide.Right)
				{
					if (UpperArmCollider.regionID.ContainsArea(AreaFlag.Upper_Arm_Left))
					{
						UpperArmCollider.MyLocation.Where = UpperArmCollider.MyLocation.Where.AddArea(AreaFlag.Upper_Arm_Right);
						UpperArmCollider.MyLocation.Where = UpperArmCollider.MyLocation.Where.RemoveArea(AreaFlag.Upper_Arm_Left);
					}
				}
			}
		}
		public void Mirror()
		{
			if (!child)
				child = transform.GetChild(0);
			if(child)
				child.transform.Rotate(new Vector3(0, 0, 180));
		}

		public Vector3 GetUp()
		{
			if (!child)
				child = transform.GetChild(0);
			if(child)
				return -child.transform.up;

			return Vector3.up;
		}
		public Vector3 GetRight()
		{
			if (!child)
				child = transform.GetChild(0);
			else
				return -child.transform.right;
			return Vector3.right;
		}
		public Vector3 GetForward()
		{
			if (!child)
				child = transform.GetChild(0);
			else
				return -child.transform.forward;
			return Vector3.forward;
		}

		public void SetRotationOfUpperArm(float amt)
		{
			if (UpperArmBody != null)
			{
				UpperArmBody.transform.Rotate(Vector3.up, amt * Time.deltaTime);
			}
		}

		void Update()
		{
			SetRotationOfUpperArm(RotationAmt);
		}

		//void OnDrawGizmos()
		//{
		//	Gizmos.color = Color.green;
		//	Gizmos.DrawLine(transform.position, transform.position + transform.up * .1f);
		//	Gizmos.color = Color.red;
		//	Gizmos.DrawLine(transform.position, transform.position + transform.right * .1f);
		//	Gizmos.color = Color.blue;
		//	Gizmos.DrawLine(transform.position, transform.position + transform.forward * .1f);
		//}
	}
}