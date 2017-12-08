//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: For controlling in-game objects with tracked devices.
//
//=============================================================================

using UnityEngine;
using Valve.VR;
using Hardlight.SDK;
using System.Collections;

public class SteamVR_TrackedObjectWithOffset : MonoBehaviour
{
	public enum EIndex
	{
		None = -1,
		Hmd = (int)OpenVR.k_unTrackedDeviceIndex_Hmd,
		Device1,
		Device2,
		Device3,
		Device4,
		Device5,
		Device6,
		Device7,
		Device8,
		Device9,
		Device10,
		Device11,
		Device12,
		Device13,
		Device14,
		Device15
	}

	public Vector3 TrackerOffset = new Vector3(0.0100224f, -2.07616526f, 0.4884118f);
	public Vector3 RollPitchYaw = new Vector3(10.854305f, 78.805113f, -91.8736f);
	//public float roll = 10.854305f;
	//public float pitch = 78.805113f;
	//public float yaw = 91.8736f;


	[SerializeField]
	private EIndex _index;
	public EIndex index
	{
		get { return _index; }
		set
		{
			Debug.Log("Adjusting Index" + name + "\n" + _index + "    " + value);
			_index = value;
		}
	}
	public Transform origin; // if not set, relative to parent
	public bool isValid = false;

	private void OnNewPoses(params object[] args)
	{
		if (index == EIndex.None)
			return;

		var i = (int)index;

		isValid = false;
		var poses = (Valve.VR.TrackedDevicePose_t[])args[0];

		if (poses.Length <= i)
			return;

		if (!poses[i].bDeviceIsConnected)
			return;

		if (!poses[i].bPoseIsValid)
			return;

		isValid = true;

		var pose = new SteamVR_Utils.RigidTransform(poses[i].mDeviceToAbsoluteTracking);

		Quaternion delta_rotation = Quaternion.Euler(RollPitchYaw.x, RollPitchYaw.y, RollPitchYaw.z);
		//Quaternion delta_rotation = Quaternion.Euler(roll, pitch, yaw);

		if (origin != null)
		{
			pose = new SteamVR_Utils.RigidTransform(origin) * pose;
			pose.pos.x *= origin.localScale.x;
			pose.pos.y *= origin.localScale.y;
			pose.pos.z *= origin.localScale.z;
			transform.position = pose.pos + TrackerOffset;
			transform.rotation = pose.rot * delta_rotation;
		}
		else
		{
			transform.localPosition = pose.pos + TrackerOffset;
			transform.localRotation = pose.rot * delta_rotation;
		}
	}

	void OnEnable()
	{
		var render = SteamVR_Render.instance;
		if (render == null)
		{
			enabled = false;
			return;
		}

		SteamVR_Utils.Event.Listen("new_poses", OnNewPoses);
	}

	private void Start()
	{
		//VRMimic.Instance.AddTrackedObject(gameObject);
	}

	void OnDisable()
	{
		SteamVR_Utils.Event.Remove("new_poses", OnNewPoses);
		isValid = false;
	}

	public void SetDeviceIndex(int index)
	{
		if (System.Enum.IsDefined(typeof(EIndex), index))
			this.index = (EIndex)index;
	}
}

