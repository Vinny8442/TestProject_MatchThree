using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

namespace DefaultNamespace
{
	public abstract class ComponentSystemWithExtras : ComponentSystem
	{
		private readonly List<IProcess> _processes = new List<IProcess>();
		private readonly List<IPromise> _promises = new List<IPromise>();
		private readonly Dictionary<Entity, int> _destroyEntities = new Dictionary<Entity, int>();
		
		public T HoldProcess<T>(T process) where T : class, IProcess
		{
			if (!_processes.Contains(process))
			{
				process.OnReadyToRelease += RemoveProcess;
				_processes.Add(process);
			}

			return process;
		}

		public IPromise HoldPromise(IProcess process)
		{
			var result = new Promise(process);
			result.ChainCompleted += () => RemovePromise(result);
			_promises.Add(result);
			return result;
		}

		private void RemovePromise(IPromise promise)
		{
			_promises.Remove(promise);
		}

		private void RemoveProcess(IProcess handler)
		{
			handler.OnReadyToRelease -= RemoveProcess;
			if (_processes.Contains(handler))
			{
				_processes.Remove(handler);
			}
		}

		protected void UpdateProcesses()
		{
			foreach (var handler in _processes.ToArray())
			{
				handler.Update();
			}

			foreach (IPromise promise in _promises.ToArray())
			{
				promise.Update();
			}

			List<Entity> toRemove = new List<Entity>();
			foreach (var entity in _destroyEntities.Keys.ToArray())
			{
				_destroyEntities[entity] = _destroyEntities[entity] - 1;
				if (_destroyEntities[entity] == 0)
				{
					toRemove.Add(entity);
				}
			}

			foreach (Entity entity in toRemove)
			{
				_destroyEntities.Remove(entity);
				EntityManager.DestroyEntity(entity);
			}
		}

		protected void DestroyEntityAfterFrame(Entity entity, int frames)
		{
			if (!_destroyEntities.ContainsKey(entity))
			{
				_destroyEntities.Add(entity, frames);
			}
		}
	}
}