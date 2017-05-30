using UnityEngine;
using System.Collections;
using NullSpace.SDK;

namespace NullSpace.SDK
{
	public class HapticLocation : MonoBehaviour
	{
		//RegionFlag is a special type of attribute which gives better inspector assignment behavior to a HapticLocation. 
		//For more info look at Scripts/RegionFlawDrawer.cs
		[RegionFlag]
		public AreaFlag MyLocation;
	}
}