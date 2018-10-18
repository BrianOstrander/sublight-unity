using System;
using System.Linq;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LunraGames.SubLight.Views
{
	public class ToolbarView : View, IToolbarView
	{
		[Serializable]
		struct IconEntry
		{
			public SetFocusLayers Layer;
			public Sprite Icon;
		}

		struct ToolbarPosition
		{
			public Vector3 Position;
			public Vector3 Forward;
		}

		[SerializeField]
		Transform orbitOrigin;
		[SerializeField]
		float orbitRadius;
		[SerializeField]
		float buttonSeparation;
		[SerializeField]
		GameObject buttonArea;
		[SerializeField]
		ToolbarButtonLeaf buttonPrefab;

		[SerializeField]
		IconEntry[] iconEntries;

		[Header("Test")]
		[SerializeField]
		int previewCount;
		[SerializeField]
		float previewButtonDiameter;

		int currentSelection;

		Vector3 RangeCenter { get { return orbitOrigin.position + (orbitOrigin.forward * orbitRadius); } }

		ToolbarPosition[] GetPositions(int max)
		{
			max = Mathf.Max(1, max);

			var results = new ToolbarPosition[max];
			var center = RangeCenter;
			var totalRange = buttonSeparation * (max - 1f);
			var rangeOffset = totalRange * 0.5f;

			for (var i = 0; i < max; i++)
			{
				var finalAngle = Mathf.Deg2Rad * (90f + ((buttonSeparation * i) - rangeOffset));
				var forward = orbitOrigin.rotation * new Vector3(Mathf.Cos(finalAngle), 0f, Mathf.Sin(finalAngle));

				results[i] = new ToolbarPosition
				{
					Position = orbitOrigin.position + (forward * orbitRadius),
					Forward = forward
				};
			}

			return results;
		}

		public Sprite GetIcon(SetFocusLayers layer) { return iconEntries.FirstOrDefault(e => e.Layer == layer).Icon; }

		public ToolbarButtonBlock[] Buttons
		{
			set
			{
				currentSelection = -1;

				for (var i = 0; i < (value == null ? 0 : value.Length); i++)
				{
					if (value[i].IsSelected) currentSelection = i;
				}

				buttonArea.transform.ClearChildren<ToolbarButtonLeaf>();

				if (value == null) return;

				var positions = GetPositions(value.Length);

				for (var i = 0; i < positions.Length; i++)
				{
					var position = positions[i];
					var block = value[i];
					var button = buttonArea.InstantiateChild(buttonPrefab, setActive: true);
					button.transform.position = position.Position;
					button.transform.forward = position.Forward;

					button.ButtonImage.sprite = block.Icon;
				}
			}
		}

		public override void Reset()
		{
			base.Reset();

			Buttons = null;

			buttonPrefab.gameObject.SetActive(false);
		}

		void OnDrawGizmos()
		{
#if UNITY_EDITOR
			Gizmos.color = Color.yellow.NewA(0.2f);
			Handles.color = Color.yellow.NewA(0.2f);
			Gizmos.DrawLine(orbitOrigin.position, RangeCenter);
			Handles.DrawWireDisc(orbitOrigin.position, orbitOrigin.up, orbitRadius);

			Gizmos.color = Color.green;
			Handles.color = Color.green;
			var count = 0f;
			foreach (var position in GetPositions(previewCount))
			{
				if (count == 0)
				{
					Gizmos.DrawLine(orbitOrigin.position, position.Position);
					Handles.DrawWireDisc(position.Position, position.Forward, previewButtonDiameter * 0.05f);
				}
				Handles.DrawWireDisc(position.Position, position.Forward, previewButtonDiameter * 0.5f);
				count++;
			}
#endif
		}
	}

	public interface IToolbarView : IView
	{
		Sprite GetIcon(SetFocusLayers layer);
		ToolbarButtonBlock[] Buttons { set; }
	}
}