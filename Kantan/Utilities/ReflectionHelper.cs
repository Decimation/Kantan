using System.Reflection;
using System.Runtime.CompilerServices;

namespace Kantan.Utilities
{
	public static class ReflectionHelper
	{
		public static T Clone<T>(T t) where T : class
		{
			var method = t.GetType().GetMethod(nameof(MemberwiseClone), BindingFlags.Instance |
			                                                            BindingFlags.NonPublic);

			if (method is not { }) {
				return default(T);
			}

			var o     = method.Invoke(t, null);
			var clone = Unsafe.As<T>(o);

			return clone;
		}
	}
}