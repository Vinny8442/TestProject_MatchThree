using System.ComponentModel;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public struct CellContent : IComponentData
	{
		public CellType type;
		public Color Color;
	}
}