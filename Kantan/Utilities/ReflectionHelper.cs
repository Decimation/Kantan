// Read S Kantan ReflectionHelper.cs
// 2023-07-05 @ 1:22 PM

using System.Reflection;
using Kantan.Text;

namespace Kantan.Utilities;

internal static class ReflectionHelper
{

	internal static bool IsBackingFieldOf(this FieldInfo f, PropertyInfo p)
	{
		var fn = f.Name.SubstringBetween("<", ">");
		var pfn = fn == p.Name;
		var bfn = f.Name.Contains("k__BackingField");

		return bfn && pfn;

	}
}