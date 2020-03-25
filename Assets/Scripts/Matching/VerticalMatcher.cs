using System;
using System.Collections.Generic;
using DefaultNamespace;
using Unity.Entities;
using UnityEngine;

namespace Matching
{
	public class VerticalMatcher : IMatchDetector
	{

		public MatchResult Check(CellsMap map, CellPosition basePosition)
		{
			List<Entity> entities = new List<Entity> {map.GetCell(basePosition.x, basePosition.y)};

			Color baseColor = map.GetColor(basePosition.x, basePosition.y);
			int y = basePosition.y - 1;
			bool continueFlag = true;
			while (continueFlag)
			{
				if (map.GetColor(basePosition.x, y, out var color))
				{
					if (color == baseColor)
					{
						entities.Add(map.GetCell(basePosition.x, y));
						y--;
					}
					else
					{
						continueFlag = false;
					}
				}
				else
				{
					continueFlag = false;
				}
			}

			y = basePosition.y + 1;
			continueFlag = true;
			while (continueFlag)
			{
				if (map.GetColor(basePosition.x, y, out var color))
				{
					if (color == baseColor)
					{
						entities.Add(map.GetCell(basePosition.x, y));
						y++;
					}
					else
					{
						continueFlag = false;
					}
				}
				else
				{
					continueFlag = false;
				}
			}
			return new MatchResult{Entities = entities.ToArray()};
		}
	}
}