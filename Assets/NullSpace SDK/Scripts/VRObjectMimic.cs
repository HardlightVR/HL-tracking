using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	public class VRObjectMimic : MonoBehaviour
	{
		private static MimickedObjects _holder;
		public static MimickedObjects Holder
		{
			get
			{
				if (_holder == null)
				{
					_holder = Initialize();
				}
				return _holder;
			}
		}

		public GameObject ObjectToMimic;
		public enum DetectionState { Active, Idle }
		public enum MimickedObject { Camera, ControllerA, ControllerB }
		public MimickedObject MimickedObjectType;

		public Vector3 ScaleMultiplier;
		public Vector3 PositionOffset;

		void Init(GameObject NewMimicTarget)
		{
			ObjectToMimic = NewMimicTarget;

			transform.position = ObjectToMimic.transform.position + PositionOffset;
			transform.rotation = ObjectToMimic.transform.rotation;
			transform.localScale = ObjectToMimic.transform.localScale + ScaleMultiplier;
		}

		void Update()
		{
			transform.position = ObjectToMimic.transform.position + PositionOffset;
			transform.rotation = ObjectToMimic.transform.rotation;
			transform.localScale = ObjectToMimic.transform.localScale + ScaleMultiplier;
		}

		public static MimickedObjects Initialize()
		{
			//Find the headset and each of the controllers
			SteamVR_Camera camera = FindObjectOfType<SteamVR_Camera>();
			//var controllers = FindObjectsOfType<SteamVR_Controller>();

			MimickedObjects mimickingObjects = new MimickedObjects();

			GameObject parent = new GameObject();
			parent.name = "VR Mimic Objects";
			mimickingObjects.Root = parent;

			GameObject go = new GameObject();
			go.transform.SetParent(parent.transform);
			go.name = "Camera Mimic";
			mimickingObjects.Camera = go.AddComponent<VRObjectMimic>();
			mimickingObjects.Camera.Init(camera.gameObject);
			mimickingObjects.Camera.MimickedObjectType = MimickedObject.Camera;
			
			//mimickingObjects.ControllerA = controllers.First();
			//mimickingObjects.ControllerB = controllers.Last();

			_holder = mimickingObjects;

			return Holder;
		}
	}

	public class MimickedObjects
	{
		public GameObject Root;
		public VRObjectMimic Camera;
		//public VRObjectMimic ControllerA;
		//public VRObjectMimic ControllerB;
	}
}
