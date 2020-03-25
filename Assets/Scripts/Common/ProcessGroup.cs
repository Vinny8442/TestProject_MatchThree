using System;
using System.Collections.Generic;
using Unity.Entities;

namespace DefaultNamespace
{
	public interface IProcess
	{
		event Action<IProcess> OnReadyToRelease;
		bool Completed { get; }
		void Update();
	}

	public interface IPromise
	{
		// void Update();
		IPromise Then(Func<IPromise> promise);
		IPromise Then(Func<IProcess> process);
		void Then(Action after);
		void Update();
	}

	public class Promise : IPromise
	{
		public event Action Completed;
		public event Action ChainCompleted;
		
		private readonly Func<IProcess> _processFunc;
		private readonly Func<IPromise> _promiseFunc;
		private readonly Action _voidFunc;
		
		private IProcess _process;
		private Promise _innerPromise;
		private Promise _nextPromise;

		public Promise(IProcess process)
		{
			_process = process;
			if (_process.Completed)
			{
				FullFilled();
			}
			else
			{
				_process.OnReadyToRelease += p => FullFilled();
			}
		}
		
		private Promise(Func<IProcess> processFunc)
		{
			_processFunc = processFunc;
		}

		private Promise(Func<IPromise> promiseFunc)
		{
			_promiseFunc = promiseFunc;
		}

		private Promise(Action voidFunc)
		{
			_voidFunc = voidFunc;
		}

		public void Start()
		{
			if (_processFunc != null)
			{
				_process = _processFunc.Invoke();
				if (_process.Completed)
				{
					FullFilled();
				}
				else
				{
					_process.OnReadyToRelease += p => FullFilled();
				}
			}
			else if (_promiseFunc != null)
			{
				_innerPromise = (Promise) _promiseFunc.Invoke();
				_innerPromise.Completed += FullFilled;
			}
			else if (_voidFunc != null)
			{
				_voidFunc.Invoke();
				FullFilled();
			}
		}

		public IPromise Then(Func<IProcess> processFunc)
		{
			_nextPromise = new Promise(processFunc);
			return _nextPromise;
		}
		
		public IPromise Then(Func<IPromise> promiseFunc)
		{
			_nextPromise = new Promise(promiseFunc);
			return _nextPromise;
		}

		public void Then(Action action)
		{
			_nextPromise = new Promise(action);
		}

		private void FullFilled()
		{
			Completed?.Invoke();
			if (_nextPromise != null)
			{
				_nextPromise.Start();
				_nextPromise.ChainCompleted += OnNextChainCompleted;
			}
			else
			{
				OnNextChainCompleted();
			}
		}

		private void OnNextChainCompleted()
		{
			ChainCompleted?.Invoke();
		}

		public void Update()
		{
			_process?.Update();
			_nextPromise?.Update();
		}
	}
	
	public class ProcessGroup<TComponent, TMarker> : IProcess 
		where TComponent:struct, IProcessEntity, IComponentData
		where TMarker:struct, IComponentData
	{
		private EntityManager _em;
		private EntityQueryBuilder _entities;
		private Dictionary<Entity, bool> _items;
		private bool _inProgress = false;
		private EntityQueryBuilder _queryBuilder;

		public event CompletableCallback<Entity, TComponent, TMarker> OnItemCompleted;
		public event Action<ProcessGroup<TComponent, TMarker>> OnCompleted;
		public event Action<IProcess> OnReadyToRelease;

		public ProcessGroup(EntityManager em, EntityQueryBuilder entities)
		{
			_entities = entities;
			_em = em;
			_items = new Dictionary<Entity, bool>();
			_queryBuilder = _entities.WithAllReadOnly<TComponent, TMarker>();
		}

		public void Add(Entity entity)
		{
			_inProgress = true;
			_items.Add(entity, true);
		}

		public Entity Add(TComponent component, TMarker marker, params IComponentData[] components)
		{
			Entity entity = _em.CreateEntity(typeof(TComponent), typeof(TMarker));
			_em.SetComponentData(entity, component);
			_em.SetComponentData(entity, marker);
			foreach (IComponentData otherComponent in components)
			{
				Type type = otherComponent.GetType();
				_em.AddComponent(entity, type);
				_em.SetComponentData(entity, otherComponent);
			}
			Add(entity);
			return entity;
		}

		public bool Completed { get; private set; }

		public void Update()
		{
			if (!_inProgress) return;
			
			_queryBuilder.ForEach((Entity entity, ref TComponent component, ref TMarker marker) =>
			{
				if (_items.ContainsKey(entity))
				{
					if (component.Completed)
					{
						OnItemCompleted?.Invoke(entity, ref component, ref marker);
						_items.Remove(entity);
					}
				}
			});
			if (_inProgress && _items.Count == 0)
			{
				_inProgress = false;
				Completed = true;
				OnCompleted?.Invoke(this);
				OnReadyToRelease?.Invoke(this);
			}
		}

		public void Clear()
		{
			_items.Clear();
		}

	}
	
	public class EmptyProcess : IProcess
	{
		public event Action<IProcess> OnReadyToRelease;
		public bool Completed { get; private set; }
		public void Update()
		{
			Completed = true;
			OnReadyToRelease?.Invoke(this);
		}
	}
}