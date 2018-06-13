using System;
using System.Linq;
using System.Collections.Generic;

namespace LunraGames.SpaceFarm 
{
	public class PresenterMediator 
	{
		struct RegistrationEntry
		{
			public RegistrationEntry(
				IPresenter presenter, 
				Func<TransitionStates> getState,
				Action<bool> closeView,
				Action unBind
			)
			{
				Presenter = presenter;
				GetState = getState;
				CloseView = closeView;
				UnBind = unBind;
			}

			public IPresenter Presenter;
			public Func<TransitionStates> GetState;
			public Action<bool> CloseView;
			public Action UnBind;
		}

		List<RegistrationEntry> registrations = new List<RegistrationEntry>();

		public void Initialize(Action<RequestStatus> done) 
		{
			done(RequestStatus.Success);
		}

		public void Register(IPresenter presenter, Func<TransitionStates> getState, Action<bool> closeView, Action unBind)
		{
			registrations.Add(new RegistrationEntry(presenter, getState, closeView, unBind));
		}

		public void UnRegister(IPresenter presenter, Action done = null)
		{
			var entry = registrations.First(p => p.Presenter == presenter);
			registrations.Remove(entry);
			switch (entry.GetState())
			{
				case TransitionStates.Closed:
					entry.UnBind();
					OnUnRegister(done);
					break;
				default:
					entry.CloseView(true);
					App.Heartbeat.Wait(() => OnUnRegisterClosed(done, entry), () => entry.GetState() == TransitionStates.Closed);
					break;
			}
		}

		public void UnRegisterAll(Action done = null, params IPresenter[] exceptions)
		{
			var waitingToClose = new List<RegistrationEntry>();
			foreach (var entry in registrations.Where(e => !exceptions.Contains(e.Presenter)).ToList())
			{
				registrations.Remove(entry);
				switch(entry.GetState())
				{
					case TransitionStates.Closed:
						entry.UnBind();
						break;
					default:
						entry.CloseView(true);
						waitingToClose.Add(entry);
						break;
				}
			}

			App.Heartbeat.Wait(() => OnUnRegisterClosed(done, waitingToClose.ToArray()), () => waitingToClose.TrueForAll(e => e.GetState() == TransitionStates.Closed));
		}

		void OnUnRegisterClosed(Action done, params RegistrationEntry[] remaining)
		{
			foreach (var entry in remaining) entry.UnBind();
			OnUnRegister(done);
		}

		void OnUnRegister(Action done)
		{
			if (done != null) done();
		}
	}
}
