using System.Collections.Generic;
using Matching;
using Notifications;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace.Game
{
	internal struct GravityAnimationMarker : IComponentData
	{
		public Entity Cell;
	}

	public struct GravityInvert : IComponentData
	{
		public bool Value;
	}
	
	public struct GravityRequest : IComponentData, IProcessEntity
	{
		public int Status;

		public bool Completed => Status >= 2;
	}
	
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	[UpdateAfter(typeof(CellDestroySystem))]
	public class GravitySystem : ComponentSystemWithExtras
	{
		private GameStateHelper _helper;
		private readonly Dictionary<ProcessGroup<AnimationComponent, GravityAnimationMarker>, Entity> _requestByAnimation = new Dictionary<ProcessGroup<AnimationComponent, GravityAnimationMarker>, Entity>();  

		protected override void OnStartRunning()
		{
			_helper = new GameStateHelper(EntityManager, Entities);
			// _animationHandler = new CompletableHandlersGroup<AnimationComponent, GravityAnimationMarker>(EntityManager, Entities);
			// _animationHandler.OnCompleted += OnAnimationCompleted;
		}

		protected override void OnUpdate()
		{
			foreach (var animationGroup in _requestByAnimation.Keys)
			{
				animationGroup.Update();
			}
			
			InvertGravityIfNecessary();
			
			if (CheckAndUpdateRequests(out var newRequest))
			{
				AnimateCells(CalculateCellsOffsets(_helper.GetSize()), newRequest);
			}
		}

		private void InvertGravityIfNecessary()
		{
			Entities.WithAllReadOnly<GravityInvert>().ForEach(entity =>
			{
				_helper.InvertGravity();
				EntityManager.DestroyEntity(entity);
			});
		}

		private bool CheckAndUpdateRequests(out Entity newRequest)
		{
			bool result = false;
			Entity newRequestInner = Entity.Null;
			Entities.WithAll<GravityRequest>().ForEach((Entity entity, ref GravityRequest request) =>
			{
				if (request.Status == 0)
				{
					request.Status++;
					newRequestInner = entity;
					result = true;
				}
				else if (request.Status == 2)
				{
					request.Status++;
				}
				else if (request.Status >= 3)
				{
					EntityManager.DestroyEntity(entity);
				}
			});
			newRequest = newRequestInner;
			return result;
		}

		private void AnimateCells(Dictionary<Entity, int> offsettedCells, Entity requestEntity)
		{
			if (offsettedCells.Count > 0)
			{
				var animationGroup = new ProcessGroup<AnimationComponent, GravityAnimationMarker>(EntityManager, Entities);
				animationGroup.OnCompleted += OnAnimationCompleted;
				animationGroup.OnItemCompleted += OnCellAnimationCompleted;
				foreach (var cell in offsettedCells.Keys)
				{
					CellPosition position = EntityManager.GetComponentData<CellPosition>(cell);
					int offset = offsettedCells[cell];
					Entity animationEntity = animationGroup.Add(
						new AnimationComponent(position, new Vector2(position.x, position.y + offset), 0.2f),
						new GravityAnimationMarker{Cell = cell});
					EntityManager.AddComponent<ToWorldPositionMarker>(animationEntity);
					EntityManager.SetComponentData(animationEntity, new ToWorldPositionMarker{Entity = cell});
					position.y += offset;
					EntityManager.SetComponentData(cell, position);
				}
				_requestByAnimation.Add(animationGroup, requestEntity);
			}
			else
			{
				EntityManager.SetComponentData(requestEntity, new GravityRequest{Status = 2});
			}
		}

		private void OnCellAnimationCompleted(Entity entity, ref AnimationComponent completable, ref GravityAnimationMarker marker)
		{
			EntityManager.SetComponentData(marker.Cell, (CellPosition) completable.Current);
		}

		private void OnAnimationCompleted(ProcessGroup<AnimationComponent, GravityAnimationMarker> animation)
		{
			if (_requestByAnimation.TryGetValue(animation, out var requestEntity))
			{
				EntityManager.SetComponentData(requestEntity, new GravityRequest{Status = 2});
			}
		}

		private Dictionary<Entity, int> CalculateCellsOffsets(GameFieldSize gameSettings)
		{
			GravityDirection gravityDirection = _helper.Gravity;
			CellsMap map = new CellsMap(EntityManager);
			Dictionary<Entity, int> offsettedCells = new Dictionary<Entity, int>();
			var i = new IntIterator(new IteratorSettings(0, gameSettings.Width - 1, 1));
			while (i.MoveNext())
			{
				var j = new IntIterator(new IteratorSettings(0, gameSettings.Height - 1, -(int)gravityDirection));
				while (j.MoveNext())
				{
					if (map.GetCell(i, j, out var cell)) continue;
					
					var k = new IntIterator(new IteratorSettings(
						j - (int) gravityDirection, 
						IteratorSettings.GetTo(0, gameSettings.Height - 1, -(int)gravityDirection), 
						-(int) gravityDirection));
					while (k.MoveNext())
					{
						if (!map.GetCell(i, k, out var affectedCell)) continue;
						
						if (!offsettedCells.TryGetValue(affectedCell, out var o))
						{
							offsettedCells[affectedCell] = 0;
						}

						offsettedCells[affectedCell] += (int)gravityDirection;
					}
				}
			}

			return offsettedCells;
		}

	}
}