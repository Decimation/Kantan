using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

// ReSharper disable EmptyConstructor

// ReSharper disable UnusedMember.Global

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Kantan.Cli.Controls
{
	/// <summary>
	///     Displayed in following sequence:
	///     <list type="number">
	///         <item>
	///             <description>
	///                 <see cref="Header" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="Subtitle" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="Options" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="Status" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="Description" />
	///             </description>
	///         </item>
	///     </list>
	/// </summary>
	public class ConsoleDialog
	{
		public ConsoleDialog() { }

		/*
		 * Header
		 * Subtitle
		 * Options
		 * Status
		 * Description
		 */


		/// <summary>
		///     The index of an <see cref="ConsoleOption" /> corresponds to its key, which, when pressed,
		///     executes the <see cref="ConsoleOption.Function" /> with the appropriate modifiers
		/// </summary>
		/// <remarks>
		///     <see cref="ConsoleManager.MAX_DISPLAY_OPTIONS" />
		/// </remarks>
		public IList<ConsoleOption> Options { get; init; }

		public bool SelectMultiple { get; init; }

		[CanBeNull]
		public string Header { get; set; }

		[CanBeNull]
		public string Status { get; set; }

		[CanBeNull]
		public string Description { get; set; }

		[CanBeNull]
		public string Subtitle { get; set; }

		/// <summary>
		///     <c>F*</c> keys
		/// </summary>
		public Dictionary<ConsoleKey, Action> Functions { get; set; } = new();

		public ConsoleOutputResult Read() => ConsoleManager.ReadInput(this);

		public Task<ConsoleOutputResult> ReadAsync() => ConsoleManager.ReadInputAsync(this);

		public override int GetHashCode()
		{
			var h = new HashCode();

			IEnumerable<int> hx = Options.Select(o => o.GetHashCode());

			foreach (int i in hx) {
				h.Add(i);
			}

			h.Add(Status?.GetHashCode());
			h.Add(Description?.GetHashCode());
			h.Add(Header?.GetHashCode());
			h.Add(Subtitle?.GetHashCode());

			return h.ToHashCode();
		}
	}
}