﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

#nullable disable
namespace Kantan.Utilities;

public static class ConfigurationHelper
{
	[CBN]
	public static T ReadSetting<T>(this Configuration configuration, [CallerMemberName] string key = null, [CBN] T def = default)
	{
		ArgumentNullException.ThrowIfNull(key, nameof(key));
		try {

			var appSettings = configuration.AppSettings.Settings;
			var result      = appSettings[key] ?? null;

			if (result == null) {
				configuration.AddUpdateSetting(key, def?.ToString());
				result = appSettings[key];
			}

			var value = result.Value;

			var type = typeof(T);

			if (type.IsEnum) {
				return (T) Enum.Parse(type, value);
			}
			if (type == typeof(bool)) {
				return (T) (object) bool.Parse(value);
			}

			return (T) (object) value;
		}
		catch (ConfigurationErrorsException) {
			return default;
		}
	}

	public static bool AddUpdateSetting(this Configuration configuration, string key, string value)
	{
		try {
			var settings = configuration.AppSettings.Settings;

			if (settings[key] == null) {
				settings.Add(key, value);
			}
			else {
				settings[key].Value = value;
			}

			configuration.Save(ConfigurationSaveMode.Modified);
			ConfigurationManager.RefreshSection(configuration.AppSettings.SectionInformation.Name);
			return true;
		}
		catch (ConfigurationErrorsException) {
			Debug.WriteLine("Error writing app settings");
			return false;
		}

	}
}