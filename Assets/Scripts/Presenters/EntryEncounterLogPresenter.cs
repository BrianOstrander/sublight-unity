using System;

using UnityEngine;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public abstract class EntryEncounterLogPresenter<M, V> : Presenter<V>, IEntryEncounterLogPresenter
		where M : EncounterLogModel
		where V : class, IEntryEncounterLogView
	{
		protected GameModel Model { get; private set; }
		protected M LogModel { get; private set; }

		public EntryEncounterLogPresenter(GameModel model, M logModel)
		{
			Model = model;
			LogModel = logModel;
		}

		public void Show(Transform root, Action done = null)
		{
			if (View.Visible) return;

			View.Reset();

			if (done != null) View.Shown += done;

			OnShow();

			ShowView(root);
		}

		public void Close()
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			CloseView(true);
		}

		protected virtual void OnShow() {}
	}

	public interface IEntryEncounterLogPresenter : IPresenter 
	{
		void Show(Transform root, Action done = null);
		void Close();
	}
}