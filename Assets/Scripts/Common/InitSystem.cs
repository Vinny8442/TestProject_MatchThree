using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	public class InitSystem : ComponentSystem
	{
		private Entity _gameEntity;

		protected override void OnStartRunning()
		{
			_gameEntity = EntityManager.CreateEntity(typeof(GameComponent));

			EntityManager.AddComponent(_gameEntity, typeof(GameFieldSize));
			EntityManager.SetComponentData(_gameEntity, new GameFieldSize{Width = 10, Height = 10});

			var colors = new[] {Color.blue, Color.cyan, Color.green, Color.magenta, Color.red, Color.yellow};
			var colorsBuffer = EntityManager.AddBuffer<GameColor>(_gameEntity);
			foreach (Color color in colors)
			{
				colorsBuffer.Add(new GameColor{Color = color});
			}

			EntityManager.AddComponent<GameGravityDirection>(_gameEntity);
			EntityManager.SetComponentData(_gameEntity, new GameGravityDirection{Value = GravityDirection.Down});
		}

		protected override void OnUpdate()
		{
		}
	}

	public struct GameComponent : IComponentData
	{
		public bool IsInited;
	}

	public struct GameFieldSize : IComponentData
	{
		public int Width;
		public int Height;
	}

	public struct GameColor : IBufferElementData
	{
		public Color Color;
	}

	public struct GameGravityDirection : IComponentData
	{
		public GravityDirection Value;
	}
}