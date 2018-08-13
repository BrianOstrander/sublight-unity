using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LunraGames.SubLight
{
	public class SplineHelper : MonoBehaviour
	{
		void OnDrawGizmos()
		{
			// This doesn't need to be defined out, but it does when it eventually uses Handles.
			Gizmos.color = Color.cyan;
#if UNITY_EDITOR
			Transform lastChild = null;
			for (var i = 0; i < transform.childCount; i++)
			{
				var child = transform.GetChild(i);
				if (lastChild != null)
				{
					Gizmos.DrawLine(lastChild.position, child.position);
				}
				lastChild = child;
			}

#endif
		}
	
	}
}