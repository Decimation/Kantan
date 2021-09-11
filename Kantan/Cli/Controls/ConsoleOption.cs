#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using Kantan.Model;
using Kantan.Text;
using Kantan.Utilities;

#pragma warning disable 8604

#pragma warning disable 8603

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Global

#pragma warning disable CS8618, CS8602

namespace Kantan.Cli.Controls
{
	public delegate object? ConsoleOptionFunction();

	/// <summary>
	///     Represents an interactive console/shell option
	/// </summary>
	public class ConsoleOption
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
		public virtual ConsoleOptionFunction Function
		{
			get => Functions[NC_FN_MAIN];
			set => Functions[NC_FN_MAIN] = value;
		}

		/// <summary>
		///     Information about this <see cref="ConsoleOption" />
		/// </summary>
		public virtual IOutline? Data { get; set; }

		public virtual Color? Color { get; set; }

		public virtual Color? ColorBG { get; set; }

		public Dictionary<ConsoleModifiers, ConsoleOptionFunction> Functions { get; init; } = new()
		{
			//[0] = () => { return null; },

		};

		#region From

		public static ConsoleOption[] FromArray<T>(T[] values) => FromArray(values, arg => arg.ToString());

		public static ConsoleOption[] FromArray<T>(IList<T> values, Func<T, string> getName)
		{
			var rg = new ConsoleOption[values.Count];

			for (int i = 0; i < rg.Length; i++) {
				var option = values[i];

				string name = getName(option);

				rg[i] = new ConsoleOption
				{
					Name     = name,
					Function = () => option
				};
			}

			return rg;
		}

		public static ConsoleOption[] FromEnum<TEnum>() where TEnum : Enum
		{
			var options = (TEnum[]) Enum.GetValues(typeof(TEnum));
			return FromArray(options, e => Enum.GetName(typeof(TEnum), e) ?? throw new InvalidOperationException());
		}

		#endregion

		internal string FormatOption()
		{
			var sb = new StringBuilder();

			string? name = Name;

			if (Color.HasValue) {
				name = name.AddColor(Color.Value);
			}

			if (ColorBG.HasValue) {
				name = name.AddColorBG(ColorBG.Value);
			}

			if (name != null) {
				sb.Append($"{name} ");
			}

			if (Data != null) {

				sb.AppendLine();

				sb.Append($"{Strings.Indent(Strings.OutlineString(Data))}");
			}

			if (!sb.ToString().EndsWith(Strings.Constants.NEW_LINE)) {
				sb.AppendLine();
			}


			return sb.ToString();
		}
	}
}