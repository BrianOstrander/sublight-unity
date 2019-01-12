using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class WaypointView : UniverseScaleView, IWaypointView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		TextMeshProUGUI nameLabel;
		[SerializeField]
		TextMeshProUGUI distanceLabel;
		[SerializeField]
		TextMeshProUGUI unitLabel;
		[SerializeField]
		float lipRadius;
		[SerializeField]
		GameObject hintArea;
		[SerializeField]
		CanvasGroup group;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		string waypointName;

		public bool HintVisible { set { hintArea.SetActive(value); } }

		public void SetDetails(string name, string distance, string units)
		{
			waypointName = name ?? "< Null >";

			nameLabel.text = name ?? string.Empty;
			distanceLabel.text = distance ?? string.Empty;
			unitLabel.text = units ?? string.Empty;
		}

		protected override void OnPosition(Vector3 position, Vector3 rawPosition)
		{

		}

		protected override void OnOpacityStack(float opacity)
		{
			group.alpha = opacity;
		}

		public override void Reset()
		{
			base.Reset();

			SetDetails(null, null, null);
			HintVisible = false;
		}

		#region Events
		public void OnEnter()
		{

		}

		public void OnExit()
		{

		}

		public void OnClick()
		{

		}
		#endregion

		void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(PositionArea.position, 0.05f);

#if UNITY_EDITOR
			Handles.color = Color.yellow;
			Handles.Label(PositionArea.position, new GUIContent(waypointName));
			Handles.color = Color.red;
			Handles.DrawWireDisc(Root.position, Vector3.up, lipRadius);
#endif

		}
	}

	public interface IWaypointView : IUniverseScaleView
	{
		bool HintVisible { set; }

		void SetDetails(string name, string distance, string units);
	}
}