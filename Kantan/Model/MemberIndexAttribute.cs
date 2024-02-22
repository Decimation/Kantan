// Deci Kantan MemberExportAttribute.cs
// $File.CreatedYear-$File.CreatedMonth-22 @ 1:48

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kantan.Model;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class MemberIndexTypeAttribute : Attribute
{

	public static BindingFlags DefaultFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
	                                          BindingFlags.DeclaredOnly;

	public MemberIndexMode Mode { get; set; } = MemberIndexMode.Inclusive;

	public BindingFlags Flags { get; set; } = DefaultFlags;

	public Type Formatter { get; set; }

}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
public class MemberIndexAttribute : Attribute
{

	public bool Include { get; set; }

	public Type Formatter { get; set; }

	public MemberIndexAttribute(bool include = true, Type formatter = null)
	{
		Include   = include;
		Formatter = formatter;
	}

}

public enum MemberIndexMode
{

	Inclusive,
	Exclusive

}

public interface IMemberFormatter
{

	public KeyValuePair<string, string> Format(object o, MemberInfo m);

}

public class MemberFormatter : IMemberFormatter
{

	public static readonly IMemberFormatter Instance = new MemberFormatter();

	public KeyValuePair<string, string> Format(object o, MemberInfo m)
	{
		string name;
		object val;

		if (m is FieldInfo fi) {
			val = fi.GetValue(o);
		}
		else if (m is PropertyInfo pi) {
			val = pi.GetValue(o);
		}
		else {
			val = null;
		}

		string kvval = val?.ToString();

		name = m.Name;

		return new KeyValuePair<string, string>(name, kvval);
	}

}

public class MemberIndexFormat
{

	public static List<KeyValuePair<string, string>> Format(object o)
	{
		var type = o.GetType();
		var attr = type.GetCustomAttribute<MemberIndexTypeAttribute>();

		if (attr == null) {
			throw new ArgumentException();
		}

		Type             sfType = attr.Formatter;
		IMemberFormatter sf;

		if (sfType != null && Activator.CreateInstance(sfType) is IMemberFormatter sf2) {
			sf = sf2;
		}
		else {
			sf = MemberFormatter.Instance;
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
			IMemberFormatter f = sf;

			if (mia is { Formatter: { } mf }) {
				var mfi = Activator.CreateInstance(mf);

				if (mfi is IMemberFormatter mmf) {
					f = mmf;
				}
			}

			var kvval = f.Format(o, info);

			kvp.Add(kvval);

		}

		return kvp;
	}

}