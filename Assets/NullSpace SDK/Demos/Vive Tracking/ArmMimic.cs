using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	public class ArmMimic : MonoBehaviour
	{
		//Easily supports disabling of the arm.

		//Shoulder Mount position
		public GameObject ShoulderMount;

		//Vive Tracked Position
		public VRObjectMimic TrackerMount;

		//Linked controller position
		public VRObjectMimic ControllerConnection;

		public bool ControllerInRange
		{
			get
			{
				Debug.Log("Not yet assigned\n");
				return true;
			}
		}

		public Component[] Visuals;

		public enum ArmSide { Right, Left }

		public ArmSide WhichSide = ArmSide.Right;

		public GameObject UpperArmParent;
		public GameObject ForearmParent;
		public GameObject JointParent;

		public Vector3 ForearmOffset;

		public enum ArmKinematicMode { ControllerOnly, ViveUpperArms, ArmsDisabled }
		public ArmKinematicMode ArmMode = ArmKinematicMode.ControllerOnly;

		public ForwardKinematicArms ControllerOnlyKinematics;
		public ForwardKinematicArms UpperArmKinematics;
		public ForwardKinematicArms LowerArmKinematics;

		public bool drawWireConnectorsOnlyWhenSelected = false;

		//Needs to be easily reskins

		public bool MimicEnabled;

		/// <summary>
		/// We will automatically reenable the Arm Mimic when the controller, shoulder and tracker are all valid
		/// </summary>
		public bool ReenableOnConditions;

		public void Initialize(ArmSide WhichSide, GameObject ShoulderMount, VRObjectMimic TrackerMount, VRObjectMimic ControllerConnection)
		{
			this.WhichSide = WhichSide;
			this.ShoulderMount = ShoulderMount;
			this.TrackerMount = TrackerMount;
			this.ControllerConnection = ControllerConnection;

			MimicEnabled = true;
			ReenableOnConditions = true;
		}

		void Start()
		{
			if (ShoulderMount == null || TrackerMount == null || ControllerConnection == null)
			{
				SetMimicEnable(false);
			}

			if (UpperArmKinematics != null && TrackerMount != null)
			{
				UpperArmKinematics.Target = TrackerMount.gameObject;
			}
			if (ControllerConnection != null)
			{
				if (LowerArmKinematics != null)
				{
					LowerArmKinematics.Target = ControllerConnection.gameObject;
				}
				if (ControllerOnlyKinematics != null)
				{
					ControllerOnlyKinematics.Target = ControllerConnection.gameObject;
				}
			}

			SetArmKinematicMode(ArmMode);
		}

		public void SetArmKinematicMode(ArmKinematicMode SetToMode)
		{
			bool EncounteredProblem = false;
			if (SetToMode == ArmKinematicMode.ViveUpperArms)
			{
				if (LowerArmKinematics != null && UpperArmKinematics != null)
				{
					ArmMode = SetToMode;
					LowerArmKinematics.enabled = true;
					UpperArmKinematics.enabled = true;
				}
				else
				{ EncounteredProblem = true; }

				if (ControllerOnlyKinematics != null)
				{
					ControllerOnlyKinematics.enabled = false;
				}
			}
			else if (SetToMode == ArmKinematicMode.ControllerOnly)
			{
				if (LowerArmKinematics != null && UpperArmKinematics != null)
				{
					LowerArmKinematics.enabled = false;
					UpperArmKinematics.enabled = false;
				}
				if (ControllerOnlyKinematics != null)
				{
					ArmMode = SetToMode;
					ControllerOnlyKinematics.enabled = true;
				}
				else
				{ EncounteredProblem = true; }
			}
			else
			{
				EncounteredProblem = true;
			}

			if (EncounteredProblem)
			{
				ArmMode = ArmKinematicMode.ArmsDisabled;
				Debug.LogError("Arm Kinematics encountered a problem. Deactivating arms.\n");
				//Turn off ALL the arm kinematics
				//Report an error
			}
		}

		void Update()
		{
			if (MimicEnabled)
			{
				UpdateMimic();
			}
			else if (ReenableOnConditions)
			{
				CheckForReenable();
			}
		}
		public void SetMimicEnable(bool enabled)
		{
			MimicEnabled = enabled;

			//Hide the visuals
			SetMimicVisual(enabled);
		}
		private void SetMimicVisual(bool visible)
		{
			if (Visuals != null)
			{
				//Set the state of the visuals
				//Visual.SetActive(visible);
			}
		}

		void UpdateMimic()
		{

		}

		void CheckForReenable()
		{
			if (ShoulderMount != null && TrackerMount != null && ControllerConnection != null)
			{
				SetMimicEnable(true);
			}
		}

		#region Gizmos
		void OnDrawGizmos()
		{
			if (!drawWireConnectorsOnlyWhenSelected)
				DrawWireframe();
		}

		void DrawWireframe()
		{
			Gizmos.color = Color.yellow;
			if (ControllerConnection != null)
				Gizmos.DrawLine(transform.position, ControllerConnection.transform.position);
			Gizmos.color = Color.cyan;
			if (ShoulderMount != null)
				Gizmos.DrawLine(transform.position, ShoulderMount.transform.position);
			Gizmos.color = Color.green;
			if (TrackerMount != null)
				Gizmos.DrawLine(transform.position, TrackerMount.transform.position);
		}
		#endregion
	}
}