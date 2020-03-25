﻿using System;
 using System.Collections.Generic;
 using DefaultNamespace;
 using DefaultNamespace.Game;
 using Notifications;
 using UnityEngine;
 using Random = UnityEngine.Random;

 namespace Unity.Entities
{
	public struct CollapseSystemMarker : IComponentData
	{
		public Entity Cell;
	}
	
	public class CellCollapseSystem : ComponentSystemWithHandlers
	{
		private static float CollapseTimeout = 0.2f;

		private EntityArchetype _cellDestroyArchetype;
		// private CompletableGroupHandler<TimerComponent, CollapseSystemMarker> _timersHandler;
		private bool _inProgress = false;
		// private Entity _gravityRequest;
		private List<CellPosition> _gravityChangerPlannedPositions = new List<CellPosition>();
		// private CompletableGroupHandler<GenerateGravityChangerRequest, CollapseSystemMarker> _gcHandler;

		protected override void OnStartRunning()
		{
			// _timersHandler = new CompletableGroupHandler<TimerComponent, CollapseSystemMarker>(EntityManager, Entities);
			// _timersHandler.OnCompleted += OnCollapseAnimationComplete;
			// _timersHandler.OnItemCompleted += OnCollapseTimerComplete;
			_cellDestroyArchetype = EntityManager.CreateArchetype(typeof(CellDestroyNotification));
		}

		protected override void OnUpdate()
		{
			float timeout = CollapseTimeout;
			Dictionary<Entity, bool> processedCells = null;
			
			ProcessGroup<TimerComponent, CollapseSystemMarker> timersHandler = null;
			Entities.WithAll<CollapseCellsRequest>().ForEach((Entity entity) =>
			{
				if (processedCells == null)
				{
					processedCells = new Dictionary<Entity, bool>();
				}

				if (timersHandler == null)
				{
					timersHandler = Keep(new ProcessGroup<TimerComponent, CollapseSystemMarker>(EntityManager, Entities));
					timersHandler.OnCompleted += OnCollapseAnimationComplete;

				}
				var cellsToCollapse = EntityManager.GetBuffer<CellToCollapse>(entity).ToArray();
				if (cellsToCollapse.Length >= 4)
				{
					_gravityChangerPlannedPositions.Add(EntityManager.GetComponentData<CellPosition>(cellsToCollapse[Random.Range(0, cellsToCollapse.Length)].Value));
				}

				bool invertGravity = false;
				for (int i = 0; i < cellsToCollapse.Length; i++)
				{
					var cellToCollapse = cellsToCollapse[i];
					if (processedCells.ContainsKey(cellToCollapse.Value)) continue;
					timersHandler.Add(new TimerComponent(timeout), new CollapseSystemMarker{Cell = cellToCollapse.Value});
					timersHandler.OnItemCompleted += OnCollapseTimerComplete;
					timeout += CollapseTimeout;
					processedCells[cellToCollapse.Value] = true;

					if (EntityManager.GetComponentData<CellContent>(cellToCollapse.Value).type == CellType.Special)
					{
						invertGravity = true;
					} 
				}

				if (invertGravity)
				{
					EntityManager.CreateEntity(new GravityInvert());
				}
				EntityManager.DestroyEntity(entity);
			});
			
			// TODO почему при перемещении наверх перестает сюда заходить 
			// _timersHandler.Update();
			// TODO  пусть система сама его держит, апдейтит и освобождает
			// _gcHandler?.Update();
			UpdateHolders();
		}
		
		private void RequestRefill()
		{
			Entity refillRequest = EntityManager.CreateEntity(typeof(RefillRequest));
			EntityManager.SetComponentData(refillRequest, new RefillRequest());
		}

		private void OnCollapseTimerComplete(Entity entity, ref TimerComponent timer, ref CollapseSystemMarker marker)
		{
			Entity destroyNotification = EntityManager.CreateEntity(_cellDestroyArchetype);
			EntityManager.SetComponentData(destroyNotification, new CellDestroyNotification{Entity = marker.Cell});
		}

		private void OnCollapseAnimationComplete(ProcessGroup<TimerComponent, CollapseSystemMarker> group)
		{
			if (_gravityChangerPlannedPositions.Count > 0)
			{
				var gcHandler = Keep(new ProcessGroup<GenerateGravityChangerRequest, CollapseSystemMarker>(EntityManager, Entities));
				foreach (var cellPosition in _gravityChangerPlannedPositions)
				{
					gcHandler.Add(new GenerateGravityChangerRequest {Position = cellPosition}, new CollapseSystemMarker());
				}

				gcHandler.OnCompleted += handler => { CreateGravityRequest(); };
				_gravityChangerPlannedPositions.Clear();
			}
			else
			{
				CreateGravityRequest();
			}
		}

		private void CreateGravityRequest()
		{
			Debug.Log($"-- Gravity Request {GameStateHelper.GetCounter(Entities)}");
			var groupHandler = Keep(new ProcessGroup<GravityRequest, CollapseSystemMarker>(EntityManager, Entities));
			groupHandler.Add(new GravityRequest(), new CollapseSystemMarker());
			groupHandler.OnCompleted +=
				handler =>
				{
					RequestRefill();
				};
		}
	}
}