using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	public static class CameraExtension
	{
		public static void HideLayer(this Camera camera, int layerToHide)
		{
			//Define the layermask without the given layer. (usually the Haptics layer)
			camera.cullingMask = camera.cullingMask - (1 << layerToHide);
		}
	}
}