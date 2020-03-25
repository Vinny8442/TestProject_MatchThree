using Unity.Entities;

namespace DefaultNamespace
{
	public class TimerSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach((Entity entity, ref TimerComponent timer) =>
			{
				timer.AddDeltaTime(Time.DeltaTime);
				if (timer.IsReadyToClear)
				{
					EntityManager.DestroyEntity(entity);
				}
			});
		}
	}
}