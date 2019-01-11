using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class GraphicRadialLimiter : MonoBehaviour
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		AnimationCurve distanceOpacity;
		[SerializeField]
		AnimationCurve normalDotOpacity;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		Graphic target;

		void OnEnable()
		{
			target = GetComponent<TextMeshProUGUI>();
		}

		void Update()
		{
			var distanceValue = distanceOpacity.Evaluate(Vector3.Distance(transform.position.NewY(0f), Vector3.zero));
			var pitchValue = normalDotOpacity.Evaluate(Mathf.Abs(Vector3.Dot(Vector3.up, App.V.CameraForward)));
			target.color = target.color.NewA(distanceValue * pitchValue);
		}
	}
}