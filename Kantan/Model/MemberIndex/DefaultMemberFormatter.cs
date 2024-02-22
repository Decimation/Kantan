// Deci Kantan MemberFormatter.cs
// $File.CreatedYear-$File.CreatedMonth-22 @ 2:30

using System.Collections.Generic;
using System.Reflection;

namespace Kantan.Model.MemberIndex;

public interface IMemberIndexFormatter
{

	public KeyValuePair<string, string> Format(object o, MemberInfo m);

}

public class DefaultMemberIndexFormatter : IMemberIndexFormatter
{

	private DefaultMemberIndexFormatter() { }

	public static readonly IMemberIndexFormatter Instance = new DefaultMemberIndexFormatter();

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