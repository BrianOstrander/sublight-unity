using System;

using UnityEngine;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
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

		protected virtual void OnShow() {}
	}

	public interface IEntryEncounterLogPresenter : IPresenter 
	{
		void Show(Transform root, Action done = null);
	}
}