using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable ConditionIsAlwaysTrueOrFalse
#pragma warning disable CA2255
namespace Kantan
{
	public static class KantanInit
	{

		[ModuleInitializer]
		public static void Init()
		{
			Trace.WriteLine($"Kantan: init");

			bool useWC;
#if USE_WC
			useWC = true;
#else
			useWC = false;
#endif
			Debug.WriteLine($"USE_WC: {useWC}");
		}
	}
}
