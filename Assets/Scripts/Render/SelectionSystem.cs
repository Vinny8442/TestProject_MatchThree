using System;
using System.Collections.Generic;
using Matching;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public class SelectionSystem : ComponentSystem
	{
		private Entity _selectedCell;
		private bool _hasSelectedCell;
		private EntityArchetype _updateNotificationArchetype;
		private EntityArchetype _switchRequestArchetype;

		protected override void OnStartRunning()
		{
			_switchRequestArchetype = EntityManager.CreateArchetype(typeof(SwitchRequest));
			_updateNotificationArchetype = EntityManager.CreateArchetype(typeof(CellUpdatedNotification));
		}

		protected override void OnUpdate()
		{
			Entities.ForEach((Entity entity, ref ResetSelection resetSelection) =>
			{
				if (_hasSelectedCell)
				{
					_hasSelectedCell = false;
					EntityManager.SetComponentData(_selectedCell, new CellSelectionFlag{Value = false});
					GenerateNotification(_selectedCell);
					PostUpdateCommands.DestroyEntity(entity);
				}
			});
			
			Entities.ForEach((Entity entity, ref CellClickedNotification notification) =>
			{
				if (_hasSelectedCell)
				{
					if (_selectedCell == notification.entity)
					{
						_hasSelectedCell = false;
						EntityManager.SetComponentData(_selectedCell, new CellSelectionFlag{Value = false});
						GenerateNotification(_selectedCell);
					}
					else
					{
						Entity switchRequest = EntityManager.CreateEntity(_switchRequestArchetype);
						EntityManager.SetComponentData(switchRequest, new SwitchRequest{cell1 = _selectedCell, cell2 = notification.entity});
					}
				}
				else
				{
					_hasSelectedCell = true;
					_selectedCell = notification.entity;
					EntityManager.SetComponentData(_selectedCell, new CellSelectionFlag{Value = true});
					GenerateNotification(_selectedCell);
				}

				PostUpdateCommands.DestroyEntity(entity);
			});
		}

		private void GenerateNotification(Entity entity)
		{
			var notificationEntity = EntityManager.CreateEntity(_updateNotificationArchetype);
			EntityManager.SetComponentData(notificationEntity, new CellUpdatedNotification{Entity = entity});
		}
	}
}