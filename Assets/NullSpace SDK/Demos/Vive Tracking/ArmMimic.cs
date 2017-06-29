using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	public class ArmMimic : MonoBehaviour
	{
		[Header("Key External Objects")]
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

		[Header("Arm Mimic Config & State")]
		public ArmSide WhichArm = ArmSide.Right;
		public enum ArmSide { Right, Left }

		public ArmKinematicMode ArmMode = ArmKinematicMode.ControllerOnly;
		public enum ArmKinematicMode { ControllerOnly, ViveUpperArms, ArmsDisabled }

		private bool _mimicEnabled;
		/// <summary>
		/// We will automatically reenable the Arm Mimic when the controller, shoulder and tracker are all valid
		/// </summary>
		public bool AttemptReenable;
		[SerializeField]
		private bool _armRenderersEnabled;
		public bool drawWireConnectorsOnlyWhenSelected = false;

		[Header("Kinematic References")]
		public ForwardKinematicArms ControllerOnlyKinematics;
		public ForwardKinematicArms UpperArmKinematics;
		public ForwardKinematicArms LowerArmKinematics;

		[Header("Reference Lists")]
		public Renderer[] ArmRenderers;

		public GameObject[] ArmElementsToMirror;

		[Header("Haptic Colliders")]
		public HardlightCollider Forearm;
		public HardlightCollider UpperArm;

		#region Properties
		public bool MimicEnabled
		{
			get { return _mimicEnabled; }
			set
			{
				_mimicEnabled = value;
				SetArmRendererEnableState = value;
			}
		}

		public bool SetArmRendererEnableState
		{
			get { return _armRenderersEnabled; }
			set
			{
				_armRenderersEnabled = value;

				for (int i = 0; i < ArmRenderers.Length; i++)
				{
					if (ArmRenderers[i] != null)
					{
						ArmRenderers[i].enabled = value;
					}
				}
			}
		}

		/// <summary>
		/// A property for setting the controller mimic with the ArmMimic.
		/// </summary>
		public VRObjectMimic ControllerKinematicTarget
		{
			set
			{
				if (LowerArmKinematics != null)
				{
					LowerArmKinematics.Target = value.gameObject;
				}
				if (ControllerOnlyKinematics != null)
				{
					ControllerOnlyKinematics.Target = value.gameObject;
				}
			}
		}
		/// <summary>
		/// A property for setting the arm tracker mimic with the ArmMimic.
		/// </summary>
		public VRObjectMimic ArmViveTrackerTarget
		{
			set
			{
				if (UpperArmKinematics != null)
				{
					UpperArmKinematics.Target = value.gameObject;
				}
			}
		}
		#endregion

		public void Initialize(ArmSide WhichSide, GameObject ShoulderMount, VRObjectMimic TrackerMount, VRObjectMimic ControllerConnection)
		{
			name = "Arm Mimic [" + WhichSide.ToString() + "]";

			this.WhichArm = WhichSide;
			this.ShoulderMount = ShoulderMount;
			this.TrackerMount = TrackerMount;
			this.ControllerConnection = ControllerConnection;

			SetArmColliderAreaFlags();

			MimicEnabled = true;
			AttemptReenable = true;
		}

		void Start()
		{
			if (ShoulderMount == null || TrackerMount == null || ControllerConnection == null)
			{
				MimicEnabled = false;
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

		/// <summary>
		/// By default the elements are configured for the right.
		/// This mirrors parts of the shoulder configuration.
		/// </summary>
		public void MirrorKeyArmElements()
		{
			if (ArmElementsToMirror != null)
			{
				for (int i = 0; i < ArmElementsToMirror.Length; i++)
				{
					Vector3 current = ArmElementsToMirror[i].transform.localPosition;
					ArmElementsToMirror[i].transform.localPosition = new Vector3(current.x * -1, current.y, current.z);
				}
			}
		}

		public void SetArmColliderAreaFlags()
		{
			//Set our colliders to use the correct side.
			Forearm.regionID = WhichArm == ArmSide.Left ? AreaFlag.Forearm_Left : AreaFlag.Forearm_Right;
			UpperArm.regionID = WhichArm == ArmSide.Left ? AreaFlag.Upper_Arm_Left : AreaFlag.Upper_Arm_Right;

			//Add the arms to the suit themselves
			HardlightSuit.Find().ModifyValidRegions(Forearm.regionID, Forearm.gameObject, Forearm);
			HardlightSuit.Find().ModifyValidRegions(UpperArm.regionID, UpperArm.gameObject, UpperArm);
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
			else if (AttemptReenable)
			{
				CheckForReenable();
			}
		}

		void UpdateMimic()
		{

		}

		void CheckForReenable()
		{
			if (ShoulderMount != null && TrackerMount != null && ControllerConnection != null)
			{
				MimicEnabled = true;
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