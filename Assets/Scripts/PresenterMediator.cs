using System;

namespace LunraGames.SpaceFarm 
{
	public class PresenterMediator 
	{
		// TODO: Move this to App.
		public AudioService Audio;

		public void Initialize(Action<RequestStatus> done) 
		{
			done(RequestStatus.Success);
		}
	}
}
