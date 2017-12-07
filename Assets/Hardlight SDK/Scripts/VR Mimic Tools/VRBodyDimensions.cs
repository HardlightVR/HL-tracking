using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Hardlight/VR Body Dimension")]
public class VRBodyDimensions : ScriptableObject
{
	public bool UpdateEveryFrame;
	[Header("Head Offset")]
	[Range(-2, 2)]
	public float NeckSize = .1f;
	[Range(-2, 2)]
	public float ForwardAmount = -.4f;

	[Header("Arm Dimensions")]
	[Range(.2f, .65f)]
	public float ShoulderWidth = .25f;
	//[Range(.1f, 1.5f)]
	//public float UpperArmLength = .45f;

	[Range(.1f, .8f)]
	public float TorsoHeight = .4f;

	[Header("Arm Shoulder Vertical Offset")]
	[Range(.2f, .75f)]
	public float VerticalShoulderOffset = .5f;

	[Header("Upper Torso Dimensions")]
	[Range(.1f, .75f)]
	public float UpperTorsoWidth = .35f;
	[Range(.1f, .75f)]
	public float UpperTorsoHeight = .4f;
	[Range(.05f, 1f)]
	public float UpperTorsoDepth = .15f;

	[Header("Lower Torso Dimensions")]
	[Range(.1f, .5f)]
	public float LowerTorsoWidth = .3f;
	[Range(.1f, .5f)]
	public float LowerTorsoHeight = .3f;
	[Range(.05f, 1f)]
	public float LowerTorsoDepth = .1f;

	public Vector3 UpperTorsoDimensions
	{
		get
		{
			return new Vector3(UpperTorsoWidth, UpperTorsoDepth, UpperTorsoHeight);
		}
	}
	public Vector3 LowerTorsoDimensions
	{
		get
		{
			return new Vector3(LowerTorsoWidth, LowerTorsoDepth, LowerTorsoHeight);
		}
	}
}