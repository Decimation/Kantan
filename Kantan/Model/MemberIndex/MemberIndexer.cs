// Deci Kantan MemberIndexFormat.cs
// $File.CreatedYear-$File.CreatedMonth-22 @ 2:29

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Kantan.Model.MemberIndex;

public static class MemberIndexer
{

	public static List<KeyValuePair<string, string>> Format(object o)
	{
		var type = o.GetType();
		var attr = type.GetCustomAttribute<MemberIndexTypeAttribute>();

		if (attr == null) {
			throw new ArgumentException();
		}

		Type             sfType = attr.Formatter;
		IMemberIndexFormatter sf;

		if (sfType != null && Activator.CreateInstance(sfType) is IMemberIndexFormatter sf2) {
			sf = sf2;
		}
		else {
			sf = DefaultMemberIndexFormatter.Instance;
		}

		var memb = type.GetMembers(MemberIndexTypeAttribute.DefaultFlags);
		var kvp  = new List<KeyValuePair<string, string>>();

		foreach (var info in memb) {
			var mia = info.GetCustomAttribute<MemberIndexAttribute>();

			if (mia == null) {
				if (attr.Mode == MemberIndexMode.Inclusive) { }

				if (attr.Mode == MemberIndexMode.Exclusive) {
					continue;
				}
				else {
					// continue;
				}
			}
			else {
				if (!mia.Include) {
					continue;
				}
			}

			IMemberIndexFormatter f = sf;

			if (mia is { Formatter: { } mf }) {
				var mfi = Activator.CreateInstance(mf);

				if (mfi is IMemberIndexFormatter mmf) {
					f = mmf;
				}
			}

			var kvval = f.Format(o, info);

			kvp.Add(kvval);

		}

		return kvp;
	}

}