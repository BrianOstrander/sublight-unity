using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace LunraGames.SubLight
{
	public class HoloHelper : MonoBehaviour
	{
		[SerializeField]
		AnimationCurve radiusCurve;
		[SerializeField]
		Vector2 widthHeight;
		[SerializeField, Range(2f, 16f)]
		int segments = 2;
		[SerializeField, Range(0f, 1f)]
		float alpha = 1f;

		void OnDrawGizmos()
		{
#if UNITY_EDITOR
			if (!DevPrefs.ShowHoloHelper) return;
			Handles.color = Color.green.NewA(alpha);
			for (var i = 0; i < segments; i++)
			{
				var progress = i / (float)(segments - 1);
				var origin = transform.position + (Vector3.up * progress * widthHeight.y);
				var radius = radiusCurve.Evaluate(progress) * widthHeight.x;
				Handles.DrawWireDisc(origin, Vector3.up, radius);
			}
#endif
		}
	}
}