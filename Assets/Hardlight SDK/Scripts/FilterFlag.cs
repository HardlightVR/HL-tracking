using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	[System.Serializable]
	public class FilterFlag
	{
		[Header("Removed Flags")]
		//[RegionFlag]
		public AreaFlag InactiveRegions = AreaFlag.None;

		public void DisableArea(AreaFlag AreaToFilterOut)
		{
			Debug.LogError("Disabled\n");
		//	InactiveRegions.AddFlag(AreaToFilterOut);
		}
		public void EnableArea(AreaFlag AreaToAllow)
		{
			InactiveRegions.RemoveArea(AreaToAllow);
		}

		public AreaFlag RemoveInactiveRegions(AreaFlag baseFlag)
		{
			return baseFlag.RemoveArea(InactiveRegions);
		}
	}
}