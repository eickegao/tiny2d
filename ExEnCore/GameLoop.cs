using System;
using System.Diagnostics;

namespace ExEnCore
{
	/// <summary>
	/// Take a reliable (but not necessaraly exact, I'm looking at you, Silverlight) tick source
	/// and make it output exactly the desired target time.
	/// </summary>
	public class GameLoop
	{
		// Timing
		TimeSpan timePending = TimeSpan.Zero;

		// Outgoing events
		public delegate void PumpedTimeEvent(TimeSpan gameTime);
		public delegate void PumpedEvent();
		public PumpedTimeEvent Update;
		public PumpedTimeEvent Draw;
		public PumpedEvent ForcePump;
		public PumpedEvent UpdateInput;

		// Timing Targets
		TimeSpan targetElapsedTime;

		TimeSpan fudgeTimeLowerBound;
		TimeSpan fudgeTimeUpperBound;

		TimeSpan dropExcessTime = TimeSpan.FromTicks(2000000); // 0.2 seconds (12 frames @ 60fps)

		public TimeSpan TargetElapsedTime
		{
			get { return targetElapsedTime; }
			set
			{
				targetElapsedTime = value;
				// Fudge time at a fixed 5% (about +/- 3FPS at 60FPS)
				fudgeTimeLowerBound = targetElapsedTime - TimeSpan.FromTicks(targetElapsedTime.Ticks / 20);
				fudgeTimeUpperBound = targetElapsedTime + TimeSpan.FromTicks(targetElapsedTime.Ticks / 20);
			}
		}

		public bool IsFixedTimeStep { get; set; }


		public GameLoop()
		{
			IsFixedTimeStep = true;
			TargetElapsedTime = TimeSpan.FromTicks(200000); // 16.6667 milliseconds (60fps)
		}


		/// <summary>Perform an update.</summary>
		void DoUpdate(TimeSpan time)
		{
			if(Update != null)
				Update(time);
		}

		/// <summary>
		/// Tick the game loop. This is expected to be running at a fixed frame rate
		/// </summary>
		public void Tick(TimeSpan addTime)
		{	
			if(UpdateInput != null)
			{
				UpdateInput();
			}

			bool hasUpdated = false;

			// Drop any excess time (for example: due to pausing in the debugger)
			if(addTime > dropExcessTime)	
				return;

			IsRunningSlowly = false;

			if(!IsFixedTimeStep)
			{
				DoUpdate(addTime);
				hasUpdated = true;
				timePending = TimeSpan.Zero;
			}
			else
			{
				// Don't mess up a good thing, if we're currently updating at or close to the desired frame rate
				if(addTime >= fudgeTimeLowerBound && addTime <= fudgeTimeUpperBound)
				{
					DoUpdate(targetElapsedTime); // Just pretend it's the right frame rate :)
					hasUpdated = true;

					// And drift timePending back towards zero, if possible
					// Note: maths here is a bit lazy :)
					TimeSpan spare = addTime - targetElapsedTime;
					if(Math.Abs((timePending + spare).Ticks) < Math.Abs(timePending.Ticks))
						timePending += spare;
				}
				else // Got a weird chunk of time, so just do our best to keep the offset from real time close to zero
				{
					timePending += addTime;

					// If we will end updating more than once, indicate that we are running slowly
					if(timePending > TimeSpan.FromTicks(targetElapsedTime.Ticks + (targetElapsedTime.Ticks / 2)))
						IsRunningSlowly = true;

					while(timePending > TimeSpan.FromTicks(targetElapsedTime.Ticks / 2))
					{
						DoUpdate(targetElapsedTime);
						hasUpdated = true;
						timePending -= targetElapsedTime;
					}
				}
			}

			if(hasUpdated && Draw != null)
			{
				Draw(addTime);
			}
			else if(ForcePump != null)
			{
				ForcePump();
			}
		}


		public void Reset()
		{
			timePending = TimeSpan.Zero;
		}

		// Helper for Game (TODO: currently not used)
		public bool IsRunningSlowly { get; set; }
	}
}
