using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	public class RequestArmMimic : MonoBehaviour
	{
		public VRObjectMimic.TypeOfMimickedObject controllerMimicType = VRObjectMimic.TypeOfMimickedObject.ControllerA;
		public GameObject Controller;
		public GameObject Tracker;
		public ArmMimic.ArmSide MySide = ArmMimic.ArmSide.Left;

		void Start()
		{
			if (Controller != null && Tracker != null)
			{
				//VRObjectMimic controllerMimic = VRMimic.Instance.FindMimicOfObject(Controller);
				VRObjectMimic controllerMimic = VRMimic.Instance.AddTrackedObject(Controller, controllerMimicType);

				if (controllerMimic == null)
				{
					//Setup controller tracker

				}
				else
				{
					controllerMimic.Init(Controller);
				}

				VRObjectMimic mimic = VRMimic.Instance.AddTrackedObject(Tracker);
				if (mimic == null)
				{

				}
				else
				{
					mimic.Init(Tracker);
				}

				ArmMimic newArm = VRMimic.Instance.ActiveBodyMimic.CreateArm(MySide, mimic, controllerMimic);
				Debug.Log("Created new arm.\n\t[Click to select it]", newArm);
			}
		}

		void Update()
		{

		}
	}
}