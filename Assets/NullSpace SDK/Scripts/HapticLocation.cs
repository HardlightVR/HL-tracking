using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	public class HapticLocation : MonoBehaviour
	{
		public GameObject correspondingObject;
		[RegionFlag]
		public AreaFlag Where;

		public Transform objTransform
		{
			get { return correspondingObject.transform; }
		}

		public HapticLocation(GameObject obj, AreaFlag Where)
		{
			correspondingObject = obj;
			this.Where = Where;
		}
	}
}