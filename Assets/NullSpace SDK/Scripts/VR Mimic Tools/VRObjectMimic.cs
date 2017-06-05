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

		private bool initialized = false;

		void Init(GameObject NewMimicTarget)
		{
			if (!initialized)
			{
				ObjectToMimic = NewMimicTarget;

				transform.position = ObjectToMimic.transform.position + PositionOffset;
				transform.rotation = ObjectToMimic.transform.rotation;
				transform.localScale = ObjectToMimic.transform.localScale + ScaleMultiplier;
				initialized = true;
			}
		}

		void Start()
		{
			Init(null);
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

			VRMimic parent = VRMimic.Instance;
			parent.name = "VR Mimic Objects";
			mimickingObjects.Root = parent.gameObject;

			GameObject go = new GameObject();
			go.transform.SetParent(parent.transform);
			go.name = "Camera Mimic";
			mimickingObjects.VRCamera = go.AddComponent<VRObjectMimic>();
			mimickingObjects.VRCamera.Init(camera.gameObject);
			mimickingObjects.VRCamera.MimickedObjectType = MimickedObject.Camera;
			
			//mimickingObjects.ControllerA = controllers.First();
			//mimickingObjects.ControllerB = controllers.Last();

			_holder = mimickingObjects;

			return Holder;
		}
	}

	public class MimickedObjects
	{
		public GameObject Root;
		public VRObjectMimic VRCamera;
		//public VRObjectMimic ControllerA;
		//public VRObjectMimic ControllerB;
	}
}
