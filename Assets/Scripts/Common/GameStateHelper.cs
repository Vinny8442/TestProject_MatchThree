using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public struct GameStateHelper
	{
		private EntityManager _em;
		private EntityQueryBuilder _entities;
		private Entity _game;
		private GravityDirection _gravity;

		public GameStateHelper(EntityManager em, EntityQueryBuilder entities)
		{
			_entities = entities;
			_em = em;
			Entity game = Entity.Null;
			_entities.WithAllReadOnly<GameComponent>().ForEach(entity => { game = entity; });
			_game = game;
			_gravity = _em.GetComponentData<GameGravityDirection>(_game).Value;
		}

		public Color[] GetColors()
		{
			var colors = _em.GetBuffer<GameColor>(_game);
			Color[] result = new Color[colors.Length];
			for (int i = 0; i < colors.Length; i++)
			{
				result[i] = colors[i].Color;
			}
			return result;
		}

		public GameFieldSize GetSize()
		{
			return _em.GetComponentData<GameFieldSize>(_game);
		}

		public GravityDirection Gravity
		{
			get => _em.GetComponentData<GameGravityDirection>(_game).Value;

			private set
			{
				_gravity = value;
				_em.SetComponentData(_game, new GameGravityDirection{Value = value});
			}
		}

		public void InvertGravity()
		{
			if (Gravity == GravityDirection.Down) Gravity = GravityDirection.Up;
			else Gravity = GravityDirection.Down;
		}

		public static long GetCounter(EntityQueryBuilder entities)
		{
			long result = 0;
			entities.WithAllReadOnly<FrameCounter>().ForEach((ref FrameCounter counter) => { result = counter.Value; });
			return result;
		}
	}
}