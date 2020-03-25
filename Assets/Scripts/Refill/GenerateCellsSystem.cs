using System;
using System.Collections.Generic;
using DefaultNamespace.Game;
using Matching;
using Unity.Entities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
	internal struct RefillTimerMarker : IComponentData { }

	public struct RefillRequest : IComponentData
	{
		public bool Value;
	}
	
	internal struct GenerateGravityChangerRequest : IComponentData, ICompletable
	{
		public CellPosition Position;
		public bool Completed { get; internal set; }
	}

	public class GenerateCellsSystem : ComponentSystem
	{
		private static float GenerateTimeout = 0.2f;
		
		private Dictionary<Entity, Action> _plannedActions = new Dictionary<Entity, Action>();
		private bool _inProgress = false;
		private GameStateHelper _helper;

		protected override void OnStartRunning()
		{
			_helper = new GameStateHelper(EntityManager, Entities);
		}

		protected override void OnUpdate()
		{
			GenerateGravityChangers();
			
			
			Entities.WithAllReadOnly<TimerComponent, RefillTimerMarker>().ForEach((Entity entity, ref TimerComponent timer) =>
			{
				if (timer.Completed)
				{
					if (_plannedActions.TryGetValue(entity, out var action))
					{
						_plannedActions.Remove(entity);
						action.Invoke();

						if (_inProgress && _plannedActions.Count == 0)
						{
							_inProgress = false;
							RequestGravity();
						}
					}
					// PostUpdateCommands.DestroyEntity(entity);
				}
			});
			
			FillTopLine();
			
		}

		private void FillTopLine()
		{
			bool active = false;
			Entities.ForEach((Entity entity, ref RefillRequest request) =>
			{
				active = true;
				EntityManager.DestroyEntity(entity);
			});

			if (!active)
			{
				return;
			}

			GameFieldSize fieldSize = _helper.GetSize();
			Color[] colors = _helper.GetColors();
			CellsMap map = new CellsMap(EntityManager);
			float delay = 0;
			int y = _helper.Gravity == GravityDirection.Down ? fieldSize.Height - 1 : 0;
			for (int i = 0; i < fieldSize.Width; i++)
			{
				if (!map.GetCell(i, y, out var cell))
				{
					_inProgress = true;
					delay += GenerateTimeout;
					AddCellAfterDelay(i, y, delay, colors);
				}
			}
		}

		private void GenerateGravityChangers()
		{
			Entities.ForEach((Entity entity, ref GenerateGravityChangerRequest gcRequest) =>
			{
				if (!gcRequest.Completed)
				{
					AddCellAt(gcRequest.Position.x, gcRequest.Position.y, _helper.GetColors(), CellType.Special);
					gcRequest.Completed = true;
					Debug.Log($"-- gcr completed {GameStateHelper.GetCounter(Entities)}");
				}
				else
				{
					Debug.Log($"-- gcr Destr {GameStateHelper.GetCounter(Entities)}");
					PostUpdateCommands.DestroyEntity(entity);
				}
			});
		}

		private void RequestGravity()
		{
			Entity entity = EntityManager.CreateEntity(typeof(GravityRequest));
			EntityManager.SetComponentData(entity, new GravityRequest());
		}

		private void AddCellAfterDelay(int x, int y, float delay, Color[] colors)
		{
			Entity entity = EntityManager.CreateEntity(typeof(TimerComponent), typeof(RefillTimerMarker));
			EntityManager.AddComponentData(entity, new TimerComponent(delay));
			_plannedActions.Add(entity, () => { AddCellAt(x, y, colors);});
		}

		private void AddCellAt(int x, int y, Color[] colors, CellType type = CellType.Simple)
		{
			Entity entity = EntityManager.CreateEntity(TMPArchetypeLibrary.CellsArchetype);
			EntityManager.SetComponentData(entity, new CellPosition{x = x, y = y});
			EntityManager.SetComponentData(entity, new CellContent
			{
				type = type,
				Color = colors[Random.Range(0, colors.Length)]
			});
		}
	}
	
}