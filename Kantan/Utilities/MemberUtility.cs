using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kantan.Utilities;

public static class MemberUtility
{

	public const BindingFlags BF_INST_ALL = BindingFlags.NonPublic    |
	                                        BindingFlags.DeclaredOnly |
	                                        BF_INST_PUBLIC;

	public const BindingFlags BF_INST_PUBLIC = BindingFlags.Instance | BindingFlags.Public;

	/*
	public delegate string FormatMember<T>(T t, object inst, MemberInfo mi) where T:struct, Enum;

	public static string[] SelectF<T>(object o, T t, FormatMember<T> ff,
	                                  BindingFlags flags = BF_INST_PUBLIC)
		where T : struct, Enum
	{
		var select = Select(o, t, flags);
		var values = Enum.GetValues<T>();
		var rg     = new List<string>();

		foreach (var (k, vv) in select) {

		}
	}*/

	public static Dictionary<TEnum, MemberInfoValue> Select<TEnum>(object obj, TEnum flags,
	                                                               BindingFlags bindingFlags = BF_INST_PUBLIC)
		where TEnum : struct, Enum
	{
		var rg = new Dictionary<TEnum, MemberInfoValue>();

		var type = obj.GetType();

		foreach (var f in Enum.GetValues<TEnum>()) {
			if (f.Equals(default(TEnum))) {
				continue;
			}

			if (EnumHelper.And(flags, f).Equals(f)) {

				var fname = Enum.GetName(f);
				var prop  = type.GetProperty(fname, bindingFlags);

				MemberInfo memb = prop;

				object val;

				if (prop != null) {
					val = prop.GetValue(obj);

				}
				else {
					var field = type.GetField(fname, bindingFlags);
					memb = field;
					val  = field?.GetValue(obj);
				}

				rg.Add(f, new MemberInfoValue(val, memb));
			}
		}

		return rg;
	}

}

public sealed class MemberInfoValue
{

	public object Value { get; }

	public MemberInfo Member { get; }

	public MemberInfoValue(object value, MemberInfo member)
	{
		Value  = value;
		Member = member;
	}

}