using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Kantan.Text;
using Kantan.Utilities;

namespace Kantan.Model;

public interface IMap
{
	public Dictionary<string, object> Data { get; }

	public static Dictionary<string, object> ToMap(object value)
	{
		var type = value.GetType();
		var f    = type.GetRuntimeFields();
		var p    = type.GetRuntimeProperties().Where(r=>r.Name != nameof(Data));

		var map = new Dictionary<string, object>();

		foreach (FieldInfo info in f) {
			map.Add($"{info.Name}", info.GetValue(value));
		}

		foreach (PropertyInfo info in p) {
			map.Add($"{info.Name}", info.GetValue(value));
		}

		return map;
	}
}