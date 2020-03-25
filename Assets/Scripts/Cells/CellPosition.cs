using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public struct CellPosition : IComponentData
	{
		public int x;
		public int y;

		public static implicit operator Vector2(CellPosition value)
		{
			return new Vector2(value.x, value.y);
		}

		public static implicit operator CellPosition(Vector2 vector)
		{
			return new CellPosition{x = (int)vector.x, y = (int)vector.y};
		}
	}
}