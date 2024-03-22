// Read S Kantan IMemberNames.cs
// 2023-07-05 @ 12:52 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kantan.Utilities;

namespace Kantan.Model;

[Obsolete]
public interface IMemberTransformer
{
	public static BindingFlags DefaultFlags { get; set; } = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
	                                                        BindingFlags.DeclaredOnly;
	
	public static IEnumerable<T> TransformMembers<T>(object obj, Func<MemberInfo, bool> pred,
	                                                 Func<MemberInfo, T> transformer,
	                                                 BindingFlags? bf = default)
	{

		bf ??= DefaultFlags;

		var mem = obj.GetType().GetMembers(bf.Value);

		var mp = mem.Where(m => m.MemberType == MemberTypes.Property)
			.OfType<PropertyInfo>();
		var mf = mem.Where(m => m.MemberType == MemberTypes.Field)
			.OfType<FieldInfo>();

		mf = mf.Where(f => !mp.Any(f.IsBackingFieldOf));

		var mem2 = mf.OfType<MemberInfo>().Union(mp).Where(pred);

		var val  = mem2.Select(transformer);

		return val;
	}

	public IEnumerable<T> TransformMembers<T>(Func<MemberInfo, bool> pred, Func<MemberInfo, T> transformer,
	                                          BindingFlags? bf = default)
	{
		return TransformMembers(this, pred, transformer, bf);
	}
}