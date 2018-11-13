using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class GroupRadialLimiter : MonoBehaviour
	{
		[SerializeField]
		AnimationCurve distanceOpacity;
		[SerializeField]
		AnimationCurve normalDotOpacity;

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