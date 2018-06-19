using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public class DestructionSystemView : View, IDestructionSystemView
	{
		public float Radius { set; private get; }
		public UniversePosition UniversePosition { set; get; }
		public Action<bool> Highlight { set; private get; }
		public Action Click { set; private get; }

		public override void Reset()
		{
			base.Reset();

			Radius = 0f;
			UniversePosition = UniversePosition.Zero;
			Highlight = ActionExtensions.GetEmpty<bool>();
			Click = ActionExtensions.Empty;
		}

		#region Events
		public void OnEnter() { Highlight(true); }
		public void OnExit() { Highlight(false); }
		public void OnClick() { Click(); }
		#endregion

		void OnDrawGizmos()
		{
			if (!Application.isPlaying) return;
#if UNITY_EDITOR
			Handles.color = Color.cyan;
			Handles.DrawWireDisc(transform.position, Vector3.up, UniversePosition.ToUnityDistance(Radius));
#endif
		}
	}

	public interface IDestructionSystemView : IGridTransform
	{
		float Radius { set; }
		Action<bool> Highlight { set; }
		Action Click { set; }
	}
}