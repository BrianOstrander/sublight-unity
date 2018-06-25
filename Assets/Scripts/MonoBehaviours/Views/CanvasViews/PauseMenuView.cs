using System;

using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public class PauseMenuView : CanvasView, IPauseMenuView
	{
		[SerializeField]
		CanvasGroup group;
		[SerializeField]
		XButton saveButton;

		public override bool Interactable
		{ 
			get { return base.Interactable; }

			set
			{
				base.Interactable = value;
				group.interactable = value;
			}
		}
		public bool CanSave { set { saveButton.interactable = value; } }
		public Action BackClick { set; private get; }
		public Action SaveClick { set; private get; }
		public Action MainMenuClick { set; private get; }

		public override void Reset()
		{
			base.Reset();

			Interactable = true;
			CanSave = true;
			BackClick = ActionExtensions.Empty;
			SaveClick = ActionExtensions.Empty;
			MainMenuClick = ActionExtensions.Empty;
		}

		#region Events
		public void OnBackClick() { BackClick(); }
		public void OnSaveClick() { SaveClick(); }
		public void OnMainMenuClick() { MainMenuClick(); }
		#endregion
	}

	public interface IPauseMenuView : ICanvasView
	{
		bool CanSave { set; }
		Action BackClick { set; }
		Action SaveClick { set; }
		Action MainMenuClick { set; }
	}
}