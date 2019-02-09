using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using LunraGames;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LunraGames.SubLight.Views
{
	public class RadialOptionsView : View, IRadialOptionsView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		Transform optionsAnchor;
		[SerializeField]
		float optionsRadiusMultiplier;

		[SerializeField]
		AnimationCurve optionsRadius;
		[SerializeField]
		AnimationCurve optionsLoadingAlpha;
		[SerializeField]
		AnimationCurve optionsButtonAlpha;

		[SerializeField]
		float optionsTopDeadSpace;
		[SerializeField]
		float optionsSpacing;

		[SerializeField]
		GameObject buttonAreaLeft;
		[SerializeField]
		GameObject buttonAreaRight;

		[SerializeField]
		ButtonRadialOptionsLeaf buttonPrefabLeft;
		[SerializeField]
		ButtonRadialOptionsLeaf buttonPrefabRight;

		[Header("Test")]
		[SerializeField]
		int previewCount;

		public LabelButtonBlock[] ButtonsLeft { set { SetButtons(value, true); } }
		public LabelButtonBlock[] ButtonsRight { set { SetButtons(value, false); } }

		ButtonRadialOptionsLeaf[] buttonInstancesLeft = new ButtonRadialOptionsLeaf[0];
		ButtonRadialOptionsLeaf[] buttonInstancesRight = new ButtonRadialOptionsLeaf[0];

		// TODO REMOVE THIS!
		[SerializeField]
		MeshRenderer tempIris;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		void SetButtons(LabelButtonBlock[] blocks, bool left)
		{
			var buttonArea = left ? buttonAreaLeft : buttonAreaRight;

			buttonArea.transform.ClearChildren<LabelButtonLeaf>();

			if (left) buttonInstancesLeft = new ButtonRadialOptionsLeaf[0];
			else buttonInstancesRight = new ButtonRadialOptionsLeaf[0];

			if (blocks == null) return;

			var buttonInstances = new List<ButtonRadialOptionsLeaf>();

			var buttonPrefab = left ? buttonPrefabLeft : buttonPrefabRight;

			foreach (var block in blocks)
			{
				var instance = buttonArea.InstantiateChild(buttonPrefab, setActive: true);

				instance.ButtonLabel.text = block.Text ?? string.Empty;
				instance.Button.OnClick.AddListener(new UnityAction(block.Click ?? ActionExtensions.Empty));
				instance.Button.interactable = block.Interactable;

				buttonInstances.Add(instance);
			}

			if (left) buttonInstancesLeft = buttonInstances.ToArray();
			else buttonInstancesRight = buttonInstances.ToArray();
		}

		float GetOptionsRadius(float scalar)
		{
			return optionsRadius.Evaluate(scalar) * optionsRadiusMultiplier;
		}

		Vector3 GetPosition(float scalar, int index, bool left)
		{
			var deadAngle = Mathf.Deg2Rad * (90f - optionsTopDeadSpace);

			var totalAngle = deadAngle - (index * optionsSpacing * Mathf.Deg2Rad);

			var result = new Vector3(Mathf.Cos(totalAngle), Mathf.Sin(totalAngle), 0f);
			if (!left) result = result.NewX(result.x * -1f);

			result = optionsAnchor.rotation * result;

			var currentRadius = GetOptionsRadius(scalar);
			result = optionsAnchor.position + (result * currentRadius);

			return result;
		}

		public override void Reset()
		{
			base.Reset();

			// TODO REMOVE THIS!
			tempIris.gameObject.SetActive(false);

			buttonPrefabLeft.gameObject.SetActive(false);
			buttonPrefabRight.gameObject.SetActive(false);

			ButtonsLeft = null;
			ButtonsRight = null;
		}

		protected override void OnShowing(float scalar)
		{
			base.OnShowing(scalar);

			UpdateButtons(scalar, buttonInstancesLeft, true);
			UpdateButtons(scalar, buttonInstancesRight, false);
		}

		protected override void OnClosing(float scalar)
		{
			base.OnClosing(scalar);

			var shiftedScalar = 1f + scalar;

			UpdateButtons(shiftedScalar, buttonInstancesLeft, true);
			UpdateButtons(shiftedScalar, buttonInstancesRight, false);
		}

		void UpdateButtons(float scalar, ButtonRadialOptionsLeaf[] buttons, bool left)
		{
			var buttonAlpha = optionsButtonAlpha.Evaluate(scalar);
			var loadingAlpha = optionsLoadingAlpha.Evaluate(scalar);

			for (var i = 0; i < buttons.Length; i++)
			{
				var button = buttons[i];
				button.transform.position = GetPosition(scalar, i, left);
				button.ButtonGroup.alpha = buttonAlpha;
				button.LoadingGroup.alpha = loadingAlpha;
			}
		}

		protected override void OnShown()
		{
			base.OnShown();
			
			// TODO REMOVE THIS!
			tempIris.gameObject.SetActive(true);
			tempIris.material.SetColor("_LipColor", new Color(1f, 0.2129f, 0.1745f));
		}


		void OnDrawGizmos()
		{
#if UNITY_EDITOR
			Gizmos.color = Color.green;
			Gizmos.DrawLine(optionsAnchor.position, optionsAnchor.position + (optionsAnchor.forward * 1f));
			Handles.color = Color.green;
			Handles.DrawWireDisc(optionsAnchor.position, optionsAnchor.forward, optionsRadiusMultiplier);

			Gizmos.DrawLine(optionsAnchor.position, GetPosition(1f, 0, true));
			Gizmos.DrawLine(optionsAnchor.position, GetPosition(1f, 0, false));

			for (var c = 0; c < 2; c++)
			{
				var isLeft = c == 0;
				for (var i = 0; i < previewCount / 2; i++)
				{
					var pos = GetPosition(1f, i, isLeft);
					Gizmos.DrawWireSphere(pos, 0.1f);
				}	
			}
#endif
		}
	}

	public interface IRadialOptionsView : IView
	{
		LabelButtonBlock[] ButtonsLeft { set; }
		LabelButtonBlock[] ButtonsRight { set; }
	}
}