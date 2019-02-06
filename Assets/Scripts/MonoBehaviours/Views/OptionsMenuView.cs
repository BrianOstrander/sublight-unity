using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class OptionsMenuView : View, IOptionsMenuView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		GameObject entryArea;

		[SerializeField]
		RectTransform entryBackground;

		[SerializeField]
		CanvasGroup rootGroup;
		[SerializeField]
		CanvasGroup entryGroup;
		[SerializeField]
		CanvasGroup entryBackgroundGroup;

		[SerializeField]
		OptionsMenuEntryLeaf[] entryPrefabs;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public IOptionsMenuEntry[] Entries
		{
			set
			{
				entryArea.transform.ClearChildren<OptionsMenuEntryLeaf>();

				if (value == null) return;

				foreach (var entry in value)
				{
					var entryType = entry.GetType();
					if (entryType == typeof(GenericOptionsMenuEntry)) InstantiateEntry<GenericOptionsMenuEntry, OptionsMenuEntryLeaf>(entry, ApplyEntry);
					else if (entryType == typeof(LabelOptionsMenuEntry)) InstantiateEntry<LabelOptionsMenuEntry, LabelOptionsMenuEntryLeaf>(entry, ApplyEntry);
					else if (entryType == typeof(ButtonOptionsMenuEntry)) InstantiateEntry<ButtonOptionsMenuEntry, ButtonOptionsMenuEntryLeaf>(entry, ApplyEntry);
					else
					{
						Debug.LogError("Unrecognized entry type: " + entryType.FullName);
					}
				}
			}
		}

		void InstantiateEntry<E, L>(IOptionsMenuEntry entry, Action<E, L> done)
			where E : OptionsMenuEntry<L>
			where L : OptionsMenuEntryLeaf
		{
			var prefab = entryPrefabs.FirstOrDefault(p => p.GetType() == typeof(L) && p.Style == entry.Style);

			if (prefab == null)
			{
				Debug.LogError("No prefab exists of type " + typeof(L).FullName + " with style " + entry.Style);
				return;
			}
			done(entry as E, entryArea.InstantiateChild(prefab as L, setActive: true));
		}

		void ApplyEntry(
			GenericOptionsMenuEntry entry,
			OptionsMenuEntryLeaf instance
		)
		{
			// Nothing to do here.
		}

		void ApplyEntry(
			LabelOptionsMenuEntry entry,
			LabelOptionsMenuEntryLeaf instance
		)
		{
			instance.Label.text = entry.Message;
		}

		void ApplyEntry(
			ButtonOptionsMenuEntry entry,
			ButtonOptionsMenuEntryLeaf instance
		)
		{
			instance.Label.text = entry.Message;
			instance.Button.OnClick.AddListener(new UnityEngine.Events.UnityAction(() => OnClick(entry)));

			switch (entry.InteractionState)
			{
				case ButtonOptionsMenuEntry.InteractionStates.Interactable:
					// todo... theming
					instance.Button.interactable = true;
					break;
				case ButtonOptionsMenuEntry.InteractionStates.LooksNotInteractable:
					// todo... theming
					instance.Button.interactable = true;
					break;
				case ButtonOptionsMenuEntry.InteractionStates.NotInteractable:
					// todo... theming
					instance.Button.interactable = false;
					break;
				default:
					Debug.LogError("Unrecognized InteractionState: " + entry.InteractionState);
					break;
			}
		}

		public override void Reset()
		{
			base.Reset();

			foreach (var prefab in entryPrefabs) prefab.gameObject.SetActive(false);

			Entries = null;
		}

		#region Events
		void OnClick(ButtonOptionsMenuEntry entry)
		{
			Debug.LogWarning("Todo, this logic!");
		}
		#endregion
	}

	public interface IOptionsMenuView : IView
	{
		IOptionsMenuEntry[] Entries { set; }
	}

	public enum OptionsMenuStyles
	{
		Unknown = 0,
		Title = 10,
		Header = 20,
		Divider = 30,
		Button = 40
	}

	public interface IOptionsMenuEntry
	{
		OptionsMenuStyles Style { get; }
		Type PrefabType { get; }
	}

	public abstract class OptionsMenuEntry<T> : IOptionsMenuEntry
		where T : OptionsMenuEntryLeaf
	{
		public abstract OptionsMenuStyles Style { get; }
		public Type PrefabType { get { return typeof(T); } }
	}

	public class GenericOptionsMenuEntry : OptionsMenuEntry<OptionsMenuEntryLeaf>
	{
		public static GenericOptionsMenuEntry CreateDivider() { return new GenericOptionsMenuEntry(OptionsMenuStyles.Divider); }

		OptionsMenuStyles style;
		public override OptionsMenuStyles Style { get { return style; } }

		GenericOptionsMenuEntry(OptionsMenuStyles style)
		{
			this.style = style;
		}
	}

	public class LabelOptionsMenuEntry : OptionsMenuEntry<LabelOptionsMenuEntryLeaf>
	{
		public static LabelOptionsMenuEntry CreateTitle(string message) { return new LabelOptionsMenuEntry(OptionsMenuStyles.Title, message); }
		public static LabelOptionsMenuEntry CreateHeader(string message) { return new LabelOptionsMenuEntry(OptionsMenuStyles.Header, message); }

		OptionsMenuStyles style;
		public override OptionsMenuStyles Style { get { return style; } }

		public string Message;

		LabelOptionsMenuEntry(OptionsMenuStyles style, string message)
		{
			this.style = style;
			Message = message;
		}
	}

	public class ButtonOptionsMenuEntry : OptionsMenuEntry<ButtonOptionsMenuEntryLeaf>
	{
		public enum InteractionStates
		{
			Unknown = 0,
			Interactable = 10,
			NotInteractable = 20,
			/// <summary>
			/// The button can be interacted with, but it looks not interactable.
			/// </summary>
			/// <remarks>
			/// This is useful for showing a popup of why it's not interactable.
			/// </remarks>
			LooksNotInteractable = 30
		}

		public static ButtonOptionsMenuEntry CreateButton(
			string message,
			Action click,
			InteractionStates interactionState = InteractionStates.Interactable
		)
		{
			return new ButtonOptionsMenuEntry(
				OptionsMenuStyles.Button,
				message,
				click,
				interactionState
			);
		}

		OptionsMenuStyles style;
		public override OptionsMenuStyles Style { get { return style; } }

		public string Message;
		public Action Click;
		public InteractionStates InteractionState;

		ButtonOptionsMenuEntry(
			OptionsMenuStyles style,
			string message,
			Action click,
			InteractionStates interactionState
		)
		{
			this.style = style;
			Message = message;
			Click = click;
			InteractionState = interactionState;
		}
	}
}