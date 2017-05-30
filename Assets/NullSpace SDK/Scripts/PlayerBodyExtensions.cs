using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NullSpace.SDK;

namespace NullSpace.SDK
{
	//ATTENTION:
	// This is a stub function that exists so that PlayerBody.HitExplosion can be left existing.
	// I created a dummy Explosion class here that doesn't contain the bulk of our Explosion code.



	public class Explosion
	{
		public class ExplosionInfo
		{
			public Vector3 explosionCenter;
			public float dist;
		}
	}
	public static class PlayerBodyExtensions
	{
		public static void HitExplosion(this PlayerBody body, Explosion.ExplosionInfo info)
		{
			AreaFlag loc = body.FindNearestFlag(info.explosionCenter);
			if (loc != AreaFlag.None)
			{
				int depth = 8;
				int repeats = 3;
				float strength = 1.0f;
				float delay = .15f;
				if (info.dist > 0)
				{
					//The depth is greater if the distance to the explosion is less.
					depth = (int)(8 / info.dist);
				}
				if (info.dist > 0)
				{
					//Adjust the strength based on the explosion strength
					strength = 2 / info.dist;
				}
				repeats = Mathf.RoundToInt(Mathf.Clamp(7 / info.dist, 0, 7));
				if (repeats > 4)
				{
					//Reduce the delay if it was a big explosion.
					delay = .1f;
				}

				//We create an emanating effect with the effect and duration.
				ImpulseGenerator.Impulse impulse = ImpulseGenerator.BeginEmanatingEffect(loc, depth)
						.WithDuration(.15f)
						.WithEffect(Effect.Click, .00f, strength)
						.WithEffect(Effect.Buzz, .15f, strength);

				body.StartCoroutine(body.RepeatedEmanations(impulse, delay, repeats));
			}
			else
			{
				Debug.LogWarning("Invalid Hit at " + info.explosionCenter + "\n");
			}
		}
	}
}