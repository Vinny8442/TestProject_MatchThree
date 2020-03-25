using System;
using System.Collections.Generic;
using DefaultNamespace;
using Unity.Entities;
using UnityEngine;

namespace Matching
{
	public interface IMatchDetector
	{
		MatchResult Check(CellsMap map, CellPosition basePosition);
	}

	public struct MatchResult
	{
		public int Length => Entities.Length;
		public Entity[] Entities;
	}
	
	
}