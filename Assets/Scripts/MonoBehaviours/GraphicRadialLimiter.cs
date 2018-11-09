using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class GraphicRadialLimiter : MonoBehaviour
	{
		[SerializeField]
		AnimationCurve opacity;

		Graphic target;

		void OnEnable()
		{
			target = GetComponent<TextMeshProUGUI>();
		}

		void Update()
		{
			target.color = target.color.NewA(opacity.Evaluate(Vector3.Distance(transform.position.NewY(0f), Vector3.zero)));
		}
	}
}