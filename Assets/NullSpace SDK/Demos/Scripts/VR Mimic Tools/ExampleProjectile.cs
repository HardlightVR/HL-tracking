using UnityEngine;
using System.Collections;
using NullSpace.SDK.FileUtilities;

namespace NullSpace.SDK.Demos
{
	public class ExampleProjectile : MonoBehaviour
	{
		public enum CollisionType { Hit, HitImpulse, BigImpact, RepeatedImpact }
		public CollisionType typeOfCollision = CollisionType.Hit;
		public float ImpactArea = 1.5f;
		public bool CanCollide = true;
		public bool DestroyAfterCollision = true;
		public GameObject rootObject;

		public void OnTriggerEnter(Collider col)
		{
			if (CanCollide)
			{
				Collide(col, transform.position);
			}
		}

		public void Collide(Collider col, Vector3 where)
		{
			#region Hit Player
			//Layer 31 is the default haptics layer.
			if (col.gameObject.layer == NSManager.HAPTIC_LAYER)
			{
				HardlightSuit body = col.gameObject.GetComponent<HardlightSuit>();
				HapticSequence seq = body.GetSequence("pulse");
				if (body != null)
				{
					switch (typeOfCollision)
					{
						case CollisionType.Hit:
							body.Hit(transform.position, "double_click");
							break;
						case CollisionType.HitImpulse:
							body.HitImpulse(transform.position, seq, .2f, 2, 3);
							break;
						case CollisionType.BigImpact:
							AreaFlag Where = body.FindAllFlagsWithinRange(transform.position, ImpactArea, true);
							body.GetSequence("buzz").Play(Where);
							break;
						case CollisionType.RepeatedImpact:
							seq = body.GetSequence("pain_short");
							body.HitImpulse(transform.position, seq, .2f, 2, 3, .15f, 1.0f);
							break;
					}
					if (DestroyAfterCollision)
					{
						CanCollide = false;
						if (rootObject != null)
						{
							Destroy(rootObject, 1.5f);
						}
						else
						{
							Destroy(gameObject, 1.5f);
						}
					}
				}
			}
			#endregion
		}
	}
}