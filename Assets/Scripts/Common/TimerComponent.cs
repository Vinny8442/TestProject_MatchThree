using Unity.Entities;

namespace DefaultNamespace
{
	public struct TimerComponent : IComponentData, ICompletable
	{
		public bool IsReadyToClear;
		public float TimeLeft;
		public bool Completed { get; private set; }
		private int _counter;

		public TimerComponent(float timeout)
		{
			TimeLeft = timeout;
			IsReadyToClear = false;
			Completed = false;
			_counter = 0;
		}

		public void AddDeltaTime(float deltaTime)
		{
			if (TimeLeft > 0)
			{
				TimeLeft -= deltaTime;
			}
			// if (TimeLeft <= 0)
			else {
				Completed = true;
				_counter++;
			}

			if (_counter >= 2)
			{
				IsReadyToClear = true;
			}
		}

	}
}