using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	public class InitSystem : ComponentSystem
	{
		private Entity _gameEntity;
		private static string GameSettingLinkage = "Config/GameSettings";

		protected override void OnStartRunning()
		{
			GameSettings config = Resources.Load<GameSettings>(GameSettingLinkage);
			_gameEntity = EntityManager.CreateEntity(typeof(GameComponent));

			EntityManager.AddComponent(_gameEntity, typeof(GameFieldSize));
			EntityManager.SetComponentData(_gameEntity, new GameFieldSize{Width = config.Width, Height = config.Height});

			var colorsBuffer = EntityManager.AddBuffer<GameColor>(_gameEntity);
			foreach (Color color in config.Colors)
			{
				colorsBuffer.Add(new GameColor{Color = color});
			}

			EntityManager.AddComponent<GameGravityDirection>(_gameEntity);
			EntityManager.SetComponentData(_gameEntity, new GameGravityDirection{Value = GravityDirection.Down});
			EntityManager.SetComponentData(_gameEntity, new GameComponent{IsInited = true});
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