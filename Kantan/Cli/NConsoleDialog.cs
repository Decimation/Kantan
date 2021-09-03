using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Kantan.Cli
{
	public class NConsoleDialog
	{
		/// <summary>
		/// The index of an <see cref="NConsoleOption"/> corresponds to its key, which, when pressed,
		/// executes the <see cref="NConsoleOption.Function"/> with the appropriate modifiers
		/// </summary>
		/// <remarks><see cref="NConsole.MAX_DISPLAY_OPTIONS"/></remarks>
		public IList<NConsoleOption> Options { get; init; }

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
		/// <c>F*</c> keys
		/// </summary>
		public Dictionary<ConsoleKey, Action> Functions { get; set; } = new() { };

		public NConsoleDialog() { }

		public ConsoleOutputResult Read() => NConsole.ReadInput(this);

		public Task<ConsoleOutputResult> ReadAsync() => NConsole.ReadInputAsync(this);

		public override int GetHashCode()
		{
			var h = new HashCode();

			var hx = Options.Select(o => (o.GetHashCode()));

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