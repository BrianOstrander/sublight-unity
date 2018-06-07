using System;

namespace LunraGames.SpaceFarm 
{
	public class ModelMediator
	{
		public void Initialize(Action<RequestStatus> done) 
		{
			done(RequestStatus.Success);
		}
	}
}
