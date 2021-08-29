#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using Kantan.Model;
#pragma warning disable 8603

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Global


#pragma warning disable CS8618, CS8602

namespace Kantan.Cli
{
	public delegate object? NConsoleFunction();


	/// <summary>
	///     Represents an interactive console/shell option
	/// </summary>
	public class NConsoleOption
	{
		public const ConsoleModifiers NC_FN_MAIN  = 0;
		public const ConsoleModifiers NC_FN_ALT   = ConsoleModifiers.Alt;
		public const ConsoleModifiers NC_FN_CTRL  = ConsoleModifiers.Control;
		public const ConsoleModifiers NC_FN_SHIFT = ConsoleModifiers.Shift;
		public const ConsoleModifiers NC_FN_COMBO = NC_FN_CTRL | NC_FN_ALT;


		/// <summary>
		///     Display name
		/// </summary>
		[MaybeNull]
		public virtual string Name { get; set; }

		/// <summary>
		///     Function to execute when selected
		/// </summary>
		public virtual NConsoleFunction Function
		{
			get => Functions[NC_FN_MAIN];
			set => Functions[NC_FN_MAIN] = value;
		}

		/// <summary>
		///     Information about this <see cref="NConsoleOption" />
		/// </summary>
		public virtual IOutline? Data { get; set; }


		public virtual Color? Color { get; set; }

		public Dictionary<ConsoleModifiers, NConsoleFunction> Functions { get; init; } = new()
		{
			//[0] = () => { return null; },

		};

		#region From

		public static List<NConsoleOption> FromList<T>(IList<T> values) =>
			FromArray(values, arg => arg.ToString()).ToList();


		public static NConsoleOption[] FromArray<T>(T[] values) => FromArray(values, arg => arg.ToString());

		public static NConsoleOption[] FromArray<T>(IList<T> values, Func<T, string> getName)
		{
			var rg = new NConsoleOption[values.Count];

			for (int i = 0; i < rg.Length; i++) {
				var option = values[i];

				string name = getName(option);

				rg[i] = new NConsoleOption
				{
					Name     = name,
					Function = () => option
				};
			}

			return rg;
		}

		public static NConsoleOption[] FromEnum<TEnum>() where TEnum : Enum
		{
			var options = (TEnum[]) Enum.GetValues(typeof(TEnum));
			return FromArray(options, e => Enum.GetName(typeof(TEnum), e) ?? throw new InvalidOperationException());
		}

		#endregion
	}
}