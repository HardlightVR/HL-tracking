using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VisualDisposer  
{
	public List<GameObject> visualsToDispose;

	public VisualDisposer()
	{
		visualsToDispose = new List<GameObject>();
	}

	public void RecordVisual(GameObject visualToDispose)
	{
		if (visualToDispose != null)
		{
			visualsToDispose.Add(visualToDispose);
		}
	}

	public void DeleteRecordedVisuals()
	{
		for (int i = visualsToDispose.Count - 1; i >= 0; i--)
		{
			GameObject.Destroy(visualsToDispose[i]);
		}
	}
	public void DropRecordedVisuals(bool randomForce = true)
	{
		for (int i = 0; i < visualsToDispose.Count; i++)
		{
			visualsToDispose[i].transform.SetParent(null);

			var rb = visualsToDispose[i].GetComponent<Rigidbody>();
			if (!rb)
			{
				visualsToDispose[i].AddComponent<Rigidbody>();
			}
			if (rb)
			{
				rb.useGravity = true;
				rb.isKinematic = false;
				if (randomForce)
				{
					rb.AddForce((Random.onUnitSphere) * Random.Range(5, 15), ForceMode.Impulse);
				}
			}
		}
	}
}