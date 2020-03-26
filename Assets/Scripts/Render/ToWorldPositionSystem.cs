using System;
using System.Collections.Generic;
using Notifications;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public struct ToWorldPositionMarker : IComponentData
	{
		public Entity Entity;
	}
	
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	[UpdateBefore(typeof(RenderSystem))]
	public class ToWorldPositionSystem : ComponentSystem
	{
		private WorldPositionConverter _converter;

		protected override void OnStartRunning()
		{
			_converter = GameStateHelper.CreateWorldPositionConverter(Entities);
		}

		protected override void OnUpdate()
		{
			Dictionary<Entity, bool> destroyedCells = new Dictionary<Entity, bool>();
			Entities.WithAllReadOnly<CellDestroyNotification>().ForEach((Entity entity, ref CellDestroyNotification notification) =>
				{
					destroyedCells[notification.Entity] = true;
				});
			
			Dictionary<Entity, bool> updatedCells = new Dictionary<Entity, bool>();
			Entities.WithAllReadOnly<CellUpdatedNotification>().ForEach((Entity entity, ref CellUpdatedNotification notification) =>
				{
					updatedCells[notification.Entity] = true;
				});
			
			Entities.WithAllReadOnly<CellPosition, WorldPositionLink>().ForEach(
				(Entity cell, ref CellPosition position, ref WorldPositionLink link) =>
				{
					if (link.WorldPositionEntity == Entity.Null)
					{
						Entity worldPositionEntity = EntityManager.CreateEntity(typeof(CellWorldPosition));
						EntityManager.SetComponentData(worldPositionEntity, new CellWorldPosition(ToWorldPosition(position), cell));
						link.WorldPositionEntity = worldPositionEntity;
						// EntityManager.SetComponentData(cell, new WorldPositionLink{WorldPositionEntity = worldPositionEntity});
					}

					if (updatedCells.ContainsKey(cell))
					{
						EntityManager.SetComponentData(link.WorldPositionEntity, new CellWorldPosition(ToWorldPosition(position), cell));
					}

					if (destroyedCells.ContainsKey(cell))
					{
						EntityManager.DestroyEntity(link.WorldPositionEntity);
					}
				});
			
			Entities.WithAllReadOnly<AnimationComponent, ToWorldPositionMarker>().ForEach((Entity entity, ref AnimationComponent animation, ref ToWorldPositionMarker marker) =>
			{
				if (!destroyedCells.ContainsKey(marker.Entity))
				{
					Entity cell = marker.Entity;
					Entity worldPositionEntity = EntityManager.GetComponentData<WorldPositionLink>(cell).WorldPositionEntity;
					EntityManager.SetComponentData(worldPositionEntity, new CellWorldPosition(ToWorldPosition(animation.Current), cell));
				}
			});
		}

		private Vector2 ToWorldPosition(Vector2 input)
		{
			return _converter.LogicToWorld(input);
		}
	}
}