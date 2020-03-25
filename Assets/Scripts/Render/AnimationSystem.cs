using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	[UpdateBefore(typeof(ToWorldPositionSystem))]
	public class AnimationSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach((Entity entity, ref AnimationComponent animation) =>
			{
				animation.AddTime(Time.DeltaTime);
				if (animation.IsReadyToDestroy)
				{
					PostUpdateCommands.DestroyEntity(entity);
				}
			});
		}
	}
	
	
	public struct AnimationComponent : IComponentData, ICompletable
	{
		private Vector2 _fromPosition;
		private Vector2 _toPosition;
		private float _duration;
		private float _timePassed;
		private int _counter;

		public bool IsReadyToDestroy => _counter >= 2;
		public bool Completed => _timePassed > _duration;
		public Vector2 Current { get; private set; }

		public AnimationComponent(Vector2 from, Vector2 to, float duration)
		{
			_fromPosition = from;
			_toPosition = to;
			_timePassed = 0;
			_duration = duration;
			Current = _fromPosition;
			_counter = 0;
		}

		public void AddTime(float deltaTime)
		{
			if (Completed)
			{
				_counter++;
				return;
			}
			
			_timePassed += deltaTime;
			Current = new Vector2
			{
				x = Mathf.Lerp(_fromPosition.x, _toPosition.x, _timePassed / _duration),
				y = Mathf.Lerp(_fromPosition.y, _toPosition.y, _timePassed / _duration),
			};
		}
	}
}