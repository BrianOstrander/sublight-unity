using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class GroupRadialLimiter : MonoBehaviour
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		AnimationCurve distanceOpacity;
		[SerializeField]
		AnimationCurve normalDotOpacity;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		CanvasGroup target;

		void OnEnable()
		{
			target = GetComponent<CanvasGroup>();
		}

		void Update()
		{
			var distanceValue = distanceOpacity.Evaluate(Vector3.Distance(transform.position.NewY(0f), Vector3.zero));
			var pitchValue = normalDotOpacity.Evaluate(Mathf.Abs(Vector3.Dot(Vector3.up, App.V.CameraForward)));
			var alpha = distanceValue * pitchValue;;
			target.alpha = alpha;
			var alphaIsNotZero = !Mathf.Approximately(0f, alpha);
			target.blocksRaycasts = alphaIsNotZero;
			target.interactable = alphaIsNotZero;
		}
	}
}