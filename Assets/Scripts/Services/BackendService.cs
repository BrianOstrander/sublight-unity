using System;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public interface IBackendService
	{
		
	}

	public abstract class BackendService : IBackendService
	{

		Action GetRequest<M>(
			string id,
			Action<RequestStatus, M> done,
			Action<string, Action<RequestStatus, M>> request
		)
			where M : class, IModel
		{
			var canceled = false;
			var isDone = false;
			var onCancel = new Action(
				() => { 
					canceled = true;
					if (isDone) return;
					isDone = true;
					done(RequestStatus.Cancel, null);
				}
			);
			request(
				id,
				(status, model) =>
				{
					if (isDone) return;
					isDone = true;
					if (canceled) done(RequestStatus.Cancel, null);
					else done(status, model);
				}
			);
			return onCancel;
		}

		Action SetRequest<M>(
			M model,
			Action<RequestStatus> done,
			Action<M, Action<RequestStatus>> request
		)
			where M : class, IModel
		{
			var canceled = false;
			var isDone = false;
			var onCancel = new Action(
				() => { 
					canceled = true;
					if (isDone) return;
					isDone = true;
					done(RequestStatus.Cancel);
				}
			);
			request(
				model,
				status =>
				{
					if (isDone) return;
					isDone = true;
					if (canceled) done(RequestStatus.Cancel);
					else done(status);
				}
			);
			return onCancel;
		}

		Action UpdateRequest<M>(
			M model,
			Action<RequestStatus> done,
			Action<M, Action<RequestStatus>> request
		)
			where M : class, IModel
		{
			var canceled = false;
			var isDone = false;
			var onCancel = new Action(
				() => { 
					canceled = true;
					if (isDone) return;
					isDone = true;
					done(RequestStatus.Cancel);
				}
			);
			request(
				model,
				status =>
				{
					if (isDone) return;
					isDone = true;
					if (canceled) done(RequestStatus.Cancel);
					else done(status);
				}
			);
			return onCancel;
		}

		#region Virtual Methods

		#endregion
	}
}