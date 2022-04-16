using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Kantan.Utilities;

public static class ResourceHelper
{
	public static ResourceManager GetManager(Assembly assembly, string p = "EmbeddedResources")
	{
		string name = null;

		foreach (string v in assembly.GetManifestResourceNames()) {
			string value = assembly.GetName().Name;

			if (v.Contains(value!) || v.Contains(p)) {
				name = v;
				break;
			}
		}

		if (name == null) {
			return null;
		}

		name = name[..name.LastIndexOf('.')];

		var resourceManager = new ResourceManager(name, assembly);

		return resourceManager;
	}

	public static Dictionary<string[], string> ReadMap(string s)
	{
		var a = s.Split('\n')
		         .Select(x => x.Split('='))
		         .ToDictionary(x => x[0].Split(','), x => x[1]);

		return a;
	}
}