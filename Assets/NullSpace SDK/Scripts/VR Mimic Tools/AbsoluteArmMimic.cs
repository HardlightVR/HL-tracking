﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace NullSpace.SDK
{
	public class AbsoluteArmMimic : AbstractArmMimic
	{
		public override ArmKinematicMode ArmMode
		{
			get
			{
				return ArmKinematicMode.ViveUpperArms;
			}
		}
		//Unity doesn't let us serialize a property or put this before a property... (dumbnitude)
		[Header("Arm Type:\t[Vive Puck Upper Arms]", order = 0)]
		[Space(12, order = 1)]
		public bool ValidDataModelArms = false;
		public GameObject elbowObject;
		public GameObject ForearmRepresentation;
		public GameObject WristObject;
		public GameObject ShoulderJoint;

		public GameObject WristObjectVisual;
		public GameObject ShoulderJointVisual;

		public Color GizmoColor = Color.green;
		public float ForearmLength = .5f;

		[Range(0, 1)]
		public float PercentagePlacement = .5f;
		[Range(0, 4)]
		public float ArmScale = .5f;

		[Header("Modified each Update")]
		public Vector3 elbowToWrist = Vector3.zero;
		public float potentialForearmDistance;

		/// <summary>
		/// Runtime adjustment (for making wrist roll movements)
		/// </summary>
		public float RollRotationOfForearm = 0;

		private bool _enableEditing = false;
		public bool EnableEditing
		{
			get { return _enableEditing; }
			set
			{
				//Turn on/off all the VRTK editing objects?
				_enableEditing = value;
			}
		}

		public UpperArmMimic UpperArmData;
		public ForearmMimic ForearmData;

		public Vector3 ControllerOffsetAmount;
		public Vector3 UpperArmOffsetAmount;
		public Vector3 shoulderOffsetAmount;

		public override void Setup(ArmSide WhichSide, GameObject ShoulderMountConnector, VRObjectMimic Tracker, VRObjectMimic Controller)
		{
			WhichArm = WhichSide;

			ShoulderMount = ShoulderMountConnector;

			//	Assign the upper arm tracker
			TrackerMount = Tracker;

			//	Assign the lower arm prefab attributes
			ControllerConnection = Controller;
			if (!ValidDataModelArms)
			{
				SetupArmWithNoVisuals(WhichSide, NonVisualPrefabs);
			}
			if (ValidDataModelArms)
			{
				SetupArmVisuals(WhichSide, BodyPartPrefabs);
			}
			SetArmColliderAreaFlags();
		}

		#region Arm Prefab Setup
		private void SetupArmWithNoVisuals(ArmSide WhichSide, BodyVisualPrefabData NonVisualPrefabs)
		{
			SetupUpperArm(WhichSide, NonVisualPrefabs.UpperArmPrefab);
			SetupForearm(WhichSide, NonVisualPrefabs.ForearmPrefab);
			SetupWristJoint(ControllerConnection, NonVisualPrefabs.JointPrefab);
			SetupShoulderJoint(NonVisualPrefabs.JointPrefab);
			ValidDataModelArms = true;

		}
		private void SetupArmVisuals(ArmSide WhichSide, BodyVisualPrefabData visualPrefabs)
		{
			AttachUpperArmVisual(WhichSide, visualPrefabs.UpperArmPrefab);
			AttachForearmVisual(WhichSide, visualPrefabs.ForearmPrefab);
			AttachWristJoint(ControllerConnection, visualPrefabs.JointPrefab);
			AttachShoulderJoint(visualPrefabs.JointPrefab);
		}

		private void SetupUpperArm(ArmSide WhichSide, GameObject UpperArmPrefab)
		{
			//Create the upper arm prefab.
			UpperArmData = GameObject.Instantiate<GameObject>(UpperArmPrefab).GetComponent<UpperArmMimic>();
			UpperArmData.AssignSide(WhichSide);
			UpperArmData.transform.SetParent(transform);
			UpperArmCollider = UpperArmData.UpperArmCollider;
			if (WhichSide == ArmSide.Right)
			{
				UpperArmData.Mirror();
			}
			elbowObject = UpperArmData.Elbow;
		}
		private void SetupForearm(ArmSide WhichSide, GameObject ForearmPrefab)
		{
			//Create the lower arm prefab.
			ForearmData = GameObject.Instantiate<GameObject>(ForearmPrefab).GetComponent<ForearmMimic>();
			ForearmData.AssignSide(WhichSide);
			ForearmData.transform.SetParent(transform);
			ForearmCollider = ForearmData.ForearmCollider;
			ForearmData.transform.localPosition = Vector3.zero;
			ForearmRepresentation = ForearmData.ForearmBody;
		}
		private void SetupWristJoint(VRObjectMimic Controller, GameObject JointPrefab)
		{
			WristObject = GameObject.Instantiate<GameObject>(JointPrefab);
			WristObject.transform.SetParent(Controller.transform);
			WristObject.transform.localPosition = ControllerOffsetAmount;
		}
		private void SetupShoulderJoint(GameObject JointPrefab)
		{
			ShoulderJoint = Instantiate(JointPrefab);
			ShoulderJoint.transform.SetParent(UpperArmData.UpperArmBody.transform);
			ShoulderJoint.transform.localPosition = Vector3.up * -.2f;
			ShoulderJoint.transform.localScale = Vector3.one * .13f;
		}

		private void AttachUpperArmVisual(ArmSide WhichSide, GameObject UpperArmPrefab)
		{
			//Create the upper arm prefab.
			UpperArmData.UpperArmVisual = GameObject.Instantiate<GameObject>(UpperArmPrefab);
			UpperArmData.UpperArmVisual.name = UpperArmPrefab.name + " [c]";
			UpperArmData.UpperArmVisual.transform.SetParent(UpperArmData.UpperArmBody.transform);
			UpperArmData.UpperArmVisual.transform.localPosition = Vector3.zero;

			if (WhichSide == ArmSide.Right)
			{
				UpperArmData.UpperArmVisual.transform.Rotate(new Vector3(0, 0, 180));
			}
		}
		private void AttachForearmVisual(ArmSide WhichSide, GameObject ForearmPrefab)
		{
			//Create the upper arm prefab.
			ForearmData.ForearmVisual = GameObject.Instantiate<GameObject>(ForearmPrefab);
			ForearmData.ForearmVisual.name = ForearmPrefab.name + " [c]";
			ForearmData.ForearmVisual.transform.SetParent(ForearmData.ForearmBody.transform);
			ForearmData.ForearmVisual.transform.localPosition = Vector3.zero;
		}
		private void AttachWristJoint(VRObjectMimic Controller, GameObject JointPrefab)
		{
			WristObjectVisual = GameObject.Instantiate<GameObject>(JointPrefab);
			WristObjectVisual.transform.SetParent(WristObject.transform);
			WristObjectVisual.transform.localPosition = Vector3.zero;
		}
		private void AttachShoulderJoint(GameObject JointPrefab)
		{
			ShoulderJointVisual = GameObject.Instantiate<GameObject>(JointPrefab);
			ShoulderJointVisual.transform.SetParent(ShoulderJoint.transform);
			ShoulderJointVisual.transform.localPosition = Vector3.zero;
			ShoulderJointVisual.transform.localScale = Vector3.one;
		}
		#endregion

		#region Visual Detachment
		public void DetachVisuals(bool DropInsteadOfDelete = true)
		{
			VisualDisposer disposer = new VisualDisposer();
			disposer.RecordVisual(UpperArmData.UpperArmVisual);
			disposer.RecordVisual(ForearmData.ForearmVisual);
			disposer.RecordVisual(WristObjectVisual);
			disposer.RecordVisual(ShoulderJointVisual);

			UnsetArmColliderAreaFlags();

			UpperArmData.UpperArmVisual = null;
			ForearmData.ForearmVisual = null;
			WristObjectVisual.transform.SetParent(null);
			WristObjectVisual = null;
			ShoulderJointVisual.transform.SetParent(null);
			ShoulderJointVisual = null;
			//Remove the hardlight colliders

			//Leave the visuals alive
			if (DropInsteadOfDelete)
				disposer.DropRecordedVisuals();
			else
				disposer.DeleteRecordedVisuals();
		}
		#endregion

		private void SetArmColliderAreaFlags()
		{
			//Set our colliders to use the correct side.
			ForearmCollider.regionID = WhichArm == ArmSide.Left ? AreaFlag.Forearm_Left : AreaFlag.Forearm_Right;
			UpperArmCollider.regionID = WhichArm == ArmSide.Left ? AreaFlag.Upper_Arm_Left : AreaFlag.Upper_Arm_Right;

			//Add the arms to the suit themselves
			bool result = HardlightSuit.Find().ModifyValidRegions(ForearmCollider.regionID, ForearmCollider.gameObject, ForearmCollider);
			if (!result)
				Debug.LogError("Unable to modify HardlightSuit's valid regions\n");
			result = HardlightSuit.Find().ModifyValidRegions(UpperArmCollider.regionID, UpperArmCollider.gameObject, UpperArmCollider);
			if (!result)
				Debug.LogError("Unable to modify HardlightSuit's valid regions\n");
		}

		private void UnsetArmColliderAreaFlags()
		{

		}

		//Index 
		void Start()
		{

		}

		void Update()
		{
			PositionUpperArm();

			if (ControllerConnection != null && elbowObject != null && WristObject != null)
			{
				if (ForearmData != null && UpperArmData != null)
				{
					CalculateAndAssignForearmPosition();

					ScaleForearmSize();

					HandleForearmOrientation();
				}
			}
		}

		private void PositionUpperArm()
		{
			if (UpperArmData != null && TrackerMount != null)
			{
				UpperArmData.transform.position = TrackerMount.transform.position;// + TrackerMount.transform.rotation * UpperArmVisual.offset * UpperArmVisual.transform.lossyScale;
				UpperArmData.transform.rotation = TrackerMount.transform.rotation;
			}
		}

		private void CalculateAndAssignForearmPosition()
		{
			elbowToWrist = WristObject.transform.position - elbowObject.transform.position;
			potentialForearmDistance = elbowToWrist.magnitude;
			float percentage = (potentialForearmDistance) * PercentagePlacement;

			ForearmData.transform.position = elbowObject.transform.position + elbowToWrist.normalized * percentage;
		}

		private void ScaleForearmSize()
		{
			Vector3 newScale = ForearmRepresentation.transform.localScale;
			newScale.y = potentialForearmDistance * ArmScale;
			ForearmRepresentation.transform.localScale = newScale;
		}

		private void HandleForearmOrientation()
		{
			//Debug.DrawLine(Vector3.zero, elbowToWrist, Color.black);
			Vector3 cross = Vector3.Cross(WristObject.transform.right, ControllerConnection.transform.up);
			Vector3 dir = elbowObject.transform.forward;
			ForearmData.transform.LookAt(WristObject.transform, dir);
		}

		void OnDrawGizmos()
		{
			Gizmos.color = GizmoColor;
			if (UpperArmData && UpperArmData.Elbow != null)
			{
				Gizmos.DrawLine(UpperArmData.Elbow.transform.position, WristObject.transform.position);
			}
		}
	}
}