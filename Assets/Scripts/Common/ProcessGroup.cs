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
		void Update();
	}

	public class Promise : IPromise
	{
		private Func<IProcess> _after1;
		private Action _after2;
		private Promise _next;
		private IProcess _process;
		private bool _executed;

		public Promise(IProcess process)
		{
			_process = process;
			if (_process.Completed)
			{
				OnProcessComplete();
			}
			else
			{
				_process.OnReadyToRelease += p => OnProcessComplete();
			}
		}

		private void OnProcessComplete()
		{
			_next.Start();
		}

		private void Start()
		{
			if (_after1 != null)
			{
				_process = _after1.Invoke();
				if (_process.Completed)
				{
					OnProcessComplete();
				}
				else
				{
					_process.OnReadyToRelease += p => OnProcessComplete();
				}
			}
			else
			{
				_after2?.Invoke();
			}
		}

		public Promise()
		{
			
		}

		public Promise Then(Func<IProcess> after)
		{
			_after1 = after;
			_next = new Promise();
			return _next;
		}

		public void Then(Action after)
		{
			_after2 = after;
		}

		public void Update()
		{
			_process?.Update();
		}
	}
	
	public class ProcessGroup<TComponent, TMarker> : IProcess 
		where TComponent:struct, ICompletable, IComponentData
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

		
		// public ICompletableHandler Then(Func<ICompletableHandler> after)
		// {
		// 	new AfterHolder(this)
		// 	OnCompleted += handler => { after.Invoke(); };
		// 	return next;
		// }

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
}