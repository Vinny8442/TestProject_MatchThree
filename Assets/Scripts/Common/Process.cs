using System;
using Unity.Entities;

namespace DefaultNamespace
{
	public class Process<TComponent, TMarker> : IProcess 
		where TComponent:struct, IProcessEntity, IComponentData
		where TMarker:struct, IComponentData
	{
		private EntityManager _em;
		private EntityQueryBuilder _entities;
		private bool _inProgress = false;
		private Entity _client;
		private EntityQueryBuilder _queryBuilder;
		
		public event CompletableCallback<Entity, TComponent, TMarker> OnCompleted;

		public Process(EntityManager em, EntityQueryBuilder entities, TComponent component, TMarker marker)
		{
			_entities = entities;
			_em = em;
			_queryBuilder = _entities.WithAllReadOnly<TComponent, TMarker>();
			
			_client = _em.CreateEntity(typeof(TComponent), typeof(TMarker));
			_em.SetComponentData(_client, component);
			_em.SetComponentData(_client, marker);
		}

		public event Action<IProcess> OnReadyToRelease;

		public bool Completed { get; private set; }

		public void Update()
		{
			if (_client == Entity.Null) return;
			
			_queryBuilder.ForEach((Entity entity, ref TComponent component, ref TMarker marker) =>
			{
				if (_client == entity)
				{
					if (component.Completed)
					{
						_client = Entity.Null;
						Completed = true;
						OnCompleted?.Invoke(entity, ref component, ref marker);
						OnReadyToRelease?.Invoke(this);
					}
				}
			});
		}
	}
}