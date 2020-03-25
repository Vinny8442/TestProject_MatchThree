using Unity.Entities;

namespace DefaultNamespace
{
	public class CompletableHandlerSingle<TComponent, TMarker> 
		where TComponent:struct, ICompletable, IComponentData
		where TMarker:struct, IComponentData
	{
		private EntityManager _em;
		private EntityQueryBuilder _entities;
		private bool _inProgress = false;
		private Entity _client;
		private EntityQueryBuilder _queryBuilder;
		
		public event CompletableCallback<Entity, TComponent, TMarker> OnCompleted;

		public CompletableHandlerSingle(EntityManager em, EntityQueryBuilder entities)
		{
			_entities = entities;
			_em = em;
			_queryBuilder = _entities.WithAllReadOnly<TComponent, TMarker>();
		}

		public void Init(TComponent component, TMarker marker)
		{
			_client = _em.CreateEntity(typeof(TComponent), typeof(TMarker));
			_em.SetComponentData(_client, component);
			_em.SetComponentData(_client, marker);
		}
		
		public void Update()
		{
			if (_client == Entity.Null) return;
			
			_queryBuilder.ForEach((Entity entity, ref TComponent component, ref TMarker marker) =>
			{
				if (_client == entity)
				{
					if (component.Completed)
					{
						OnCompleted(entity, ref component, ref marker);
						_client = Entity.Null;
					}
				}
			});
		}
	}
}