using System;
using System.Collections.Generic;
using System.Text;

namespace Tiny2d
{
	public interface IDeepCopy
	{
		void CopyValuesTo(object target);
	}
}
