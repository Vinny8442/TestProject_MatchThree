using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public struct CellWorldPosition : IComponentData
	{
		public CellWorldPosition(Vector2 value, Entity cell)
		{
			x = value.x;
			y = value.y;
			Cell = cell;
		}
		public float x;
		public float y;
		public Entity Cell { get; }
	}
}