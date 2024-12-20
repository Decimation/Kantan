using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Kantan.Utilities;

namespace Kantan.Model;

[Obsolete]
public interface IMap
{

	public Dictionary<string, object> Data { get; }

	/*public static IEnumerable<KeyValuePair<string, object>> ToKeyValues(
		object value, Predicate<MemberInfo> membPredicate = null, Func<MemberInfo, object, object> fieldConv = null)
	{
		var map = ToMap(value);

		return map.ToArray();
	}*/

	/// <summary>
	/// Converts an object <paramref name="value"/> to a <see cref="Dictionary{TKey,TValue}"/>
	/// </summary>
	/// <param name="value">Object</param>
	/// <param name="membPredicate"><see cref="MemberInfo"/> predicate</param>
	/// <param name="fieldConv">Field conversion functor</param>
	public static Dictionary<string, object> ToMap(object value, Predicate<MemberInfo> membPredicate = null,
	                                               Func<MemberInfo, object, object> fieldConv = null)
	{
		Predicate<MemberInfo> fn = (MemberInfo mi) => mi.Name != nameof(Data);

		// membPredicate ??= fn;
		Predicate<MemberInfo> mbp = mi => { return fn(mi) && (membPredicate?.Invoke(mi) ?? true); };

		fieldConv ??= (info, inst) =>
		{
			return info switch
			{
				FieldInfo f    => f.GetValue(inst),
				PropertyInfo p => p.GetValue(inst),
				_              => inst
			};
		};

		var type = value.GetType();

		var p = type.GetRuntimeProperties().Where(pi => mbp(pi))
			.Where(r => r.Name != nameof(Data));

		var f = type.GetRuntimeFields().Where(fi => mbp(fi))
			.Where(f => !p.Any(pp => pp.Name.Contains(f.Name, StringComparison.InvariantCultureIgnoreCase)));

		var m = f.OfType<MemberInfo>().Union(p)
			.Where(m => !m.DeclaringType.Namespace.StartsWith("System"));

		return m.ToDictionary(info => $"{info.Name}", info => fieldConv(info, value));
	}

}