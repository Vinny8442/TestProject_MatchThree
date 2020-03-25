using System.Collections.Generic;
using Unity.Entities;

namespace DefaultNamespace
{
	public abstract class ComponentSystemWithHandlers : ComponentSystem
	{
		private readonly List<IProcess> _processes = new List<IProcess>();
		private readonly List<IPromise> _promises = new List<IPromise>();
		
		public T Keep<T>(T process) where T : class, IProcess
		{
			if (!_processes.Contains(process))
			{
				process.OnReadyToRelease += ReleaseHolder;
				_processes.Add(process);
			}

			return process;
		}

		public IPromise Keep2(IProcess process)
		{
			var result = new Promise(process);
			_promises.Add(result);
			return result;
		}

		private void ReleaseHolder(IProcess handler)
		{
			handler.OnReadyToRelease -= ReleaseHolder;
			if (_processes.Contains(handler))
			{
				_processes.Remove(handler);
			}
		}

		protected void UpdateHolders()
		{
			foreach (var handler in _processes.ToArray())
			{
				handler.Update();
			}

			foreach (IPromise promise in _promises.ToArray())
			{
				promise.Update();
			}
		}
	}
}