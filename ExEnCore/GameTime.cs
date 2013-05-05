using System;

namespace Microsoft.Xna.Framework
{
	public class GameTime
	{
		TimeSpan elapsedTime;
		public TimeSpan ElapsedGameTime { get { return elapsedTime; } }

		TimeSpan totalTime;
		public TimeSpan TotalGameTime { get { return totalTime; } }

		public GameTime()
		{
			elapsedTime = totalTime = TimeSpan.Zero;
		}


		public void Update(TimeSpan elapsed)
		{
			elapsedTime = elapsed;
			totalTime += elapsed;
		}

		public void Reset()
		{
			elapsedTime = TimeSpan.Zero;
			totalTime = TimeSpan.Zero;
		}
	}
}
