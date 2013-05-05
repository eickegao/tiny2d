using System;
using System.Runtime.CompilerServices;
using MonoTouch.UIKit;

namespace Microsoft.Xna.Framework
{
	public static class PowerStatus
	{		
		public static Microsoft.Xna.Framework.BatteryChargeStatus BatteryChargeStatus
		{
			get
			{
				Enable();
				switch (UIDevice.CurrentDevice.BatteryState)
				{
					case UIDeviceBatteryState.Charging:
						return BatteryChargeStatus.Charging;
					case UIDeviceBatteryState.Full:
						return BatteryChargeStatus.High;
					case UIDeviceBatteryState.Unknown:
						return BatteryChargeStatus.Unknown;
					default:
						if (BatteryLifePercent >= 60.0f)
							return BatteryChargeStatus.High;
						if (BatteryLifePercent >= 20.0f)
							return BatteryChargeStatus.Low;
						return BatteryChargeStatus.Critical;
				}
			}
		}
		
		private static void Enable()
		{
			if (!UIDevice.CurrentDevice.BatteryMonitoringEnabled) 
				UIDevice.CurrentDevice.BatteryMonitoringEnabled = true;
		}

		public static TimeSpan? BatteryFullLifetime
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public static float? BatteryLifePercent
		{
			get
			{
				Enable();
				return UIDevice.CurrentDevice.BatteryLevel * 100;
			}
		}

		public static TimeSpan? BatteryLifeRemaining
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public static Microsoft.Xna.Framework.PowerLineStatus PowerLineStatus
		{
			get
			{
				Enable();
				if (UIDevice.CurrentDevice.BatteryState == UIDeviceBatteryState.Unplugged)
				{
					return PowerLineStatus.Offline;
				}
				return PowerLineStatus.Online;
			}
		}
	}
}

