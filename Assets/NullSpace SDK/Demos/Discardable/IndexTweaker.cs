using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IndexTweaker : MonoBehaviour
{
	public List<SteamVR_TrackedObject> ele = new List<SteamVR_TrackedObject>();
	public List<IndexSelector> myElements = new List<IndexSelector>();

	void Awake()
	{
		for (int i = 0; i < ele.Count; i++)
		{
			myElements.Add(new IndexSelector(ele[i]));
		}
	}

	void Update()
	{

	}

	void OnGUI()
	{
		for (int i = 0; i < myElements.Count; i++)
		{
			myElements[i].OnGUI(i);
		}
	}

	void OnDrawGizmos()
	{
		for (int i = 0; i < myElements.Count; i++)
		{
			myElements[i].Draw(myElements[i].tracked.transform.position);
		}
	}

	public class IndexSelector
	{
		private string origName;
		public SteamVR_TrackedObject tracked;
		public IndexSelector(SteamVR_TrackedObject obj)
		{
			tracked = obj;
			origName = tracked.name;
			tracked.name = origName + " - [" + tracked.index.ToString() + "]";
		}
		public void SetIndex(SteamVR_TrackedObject.EIndex targIndex)
		{
			tracked.gameObject.SetActive(true);
			tracked.index = targIndex;
			tracked.name = origName + " - [" + tracked.index.ToString() + "]";
		}
		public void Draw(Vector3 centerPoint)
		{
			//Gizmos.DrawSphere(centerPoint, .2f);
		}
		public void OnGUI(int index)
		{
			float height = 40;
			float width = 200;
			float smallWidth = 20;
			string text = tracked.name + "\n" + tracked.index;
			Rect rect = new Rect(0, height * index, width, height);
#if UNITY_EDITOR
			if (GUI.Button(new Rect(smallWidth, rect.y, width - 2 * smallWidth, height), text))
			{
				UnityEditor.Selection.activeGameObject = tracked.gameObject;
			}
#else
			GUI.Box(rect, text);
#endif
			if (GUI.Button(new Rect(0, rect.y, smallWidth, height), "-"))
			{
				var reduce = Mathf.Clamp(((int)tracked.index) - 1, 0, 18);
				SetIndex((SteamVR_TrackedObject.EIndex)reduce);
			}

			if (GUI.Button(new Rect(width - smallWidth, rect.y, smallWidth, height), "+"))
			{
				var reduce = Mathf.Clamp(((int)tracked.index) + 1, 0, 18);
				SetIndex((SteamVR_TrackedObject.EIndex)reduce);
			}
		}
	}
}
