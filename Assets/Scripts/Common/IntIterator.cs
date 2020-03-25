using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
	public struct IntIterator : IEnumerator<int>
	{
		private readonly int _step;
		private readonly int _first;
		private readonly int _last;
		public IntIterator(IteratorSettings settings)
		{
			_first = settings.First;
			_last = settings.Last;
			_step = settings.Step;

			Current = _first - _step;
		}

		public bool MoveNext()
		{
			Current += _step;
			if (_step > 0 && Current > _last)
			{
				return false;
			}

			if (_step < 0 && Current < _last)
			{
				return false;
			}

			return true;

		}

		public void Reset()
		{
			Current = _first - _step;
		}

		public int Current { get; private set; }

		public static implicit operator int(IntIterator iterator)
		{
			return iterator.Current;
		}

		object IEnumerator.Current => Current;

		public void Dispose()
		{
		}
	}

	public struct IteratorSettings
	{
		public int First { get; private set; }
		public int Last { get; private set; }
		public int Step { get; private set; }
		
		public IteratorSettings(int limit1, int limit2, int step)
		{
			if (step == 0)
			{
				throw new Exception("Step can't be equal to 0");
			}
			Step = step;
			if (Step > 0)
			{
				First = Mathf.Min(limit1, limit2);
				Last = Mathf.Max(limit1, limit2);
			}
			else
			{
				First = Mathf.Max(limit1, limit2);
				Last = Mathf.Min(limit1, limit2);
			}
		}

		public IteratorSettings(int start, int finish)
		{
			First = start;
			Last = finish;

			if (start > finish)
			{
				Step = -1;
			}
			else
			{
				Step = 1;
			}
		}

		public static int GetFrom(int limit1, int limit2, int step)
		{
			if (step > 0)
			{
				return Mathf.Min(limit1, limit2);
			}

			return Mathf.Max(limit1, limit2);
		}

		public static int GetTo(int limit1, int limit2, int step)
		{
			if (step > 0)
			{
				return Mathf.Max(limit1, limit2);
			}

			return Mathf.Min(limit1, limit2);
		}
		
	}
}