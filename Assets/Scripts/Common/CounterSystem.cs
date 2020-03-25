using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public struct FrameCounter : IComponentData
	{
		public long Value;
	}
	public class CounterSystem : ComponentSystem
	{
		protected override void OnStartRunning()
		{
			EntityManager.CreateEntity(new FrameCounter());
		}

		protected override void OnUpdate()
		{
			Entities.ForEach((ref FrameCounter counter) =>
			{
				counter.Value++;
				// Debug.Log($"{counter.Value} : {(int)(1f / Time.DeltaTime)} FPS");
			});
		}
	}
}