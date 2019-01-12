using UnityEngine;
using UnityEngine.UI;

namespace LunraGames.SubLight
{
	public class OpacityByDirection : MonoBehaviour
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		Graphic[] graphics;
		[SerializeField]
		Transform origin;
		[SerializeField]
		FloatRange range;
		[SerializeField]
		CurveStyleBlock opacity = CurveStyleBlock.Default;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		//[SerializeField, Range(-1f, 1f)]
		//float lolDot;

		void Update()
		{
			//if (!App.V.CameraHasMoved) return;
			var currentOpacity = opacity.Evaluate(range.Progress(Vector3.Dot(App.V.CameraForward.FlattenY(), origin.up)));
			//var currentOpacity = opacity.Evaluate(range.Evaluate(lolDot));
			foreach (var graphic in graphics) graphic.color = graphic.color.NewA(currentOpacity);
		}

		void OnDrawGizmosSelected()
		{
			if (!Application.isPlaying) return;
			Gizmos.color = Color.red;
			var flatCameraDirection = App.V.CameraForward.FlattenY();
			Gizmos.DrawLine(origin.position, origin.position + flatCameraDirection);
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(origin.position, origin.position + origin.up);
		}
	}
}