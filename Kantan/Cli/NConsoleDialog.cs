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

		public Action[] Functions { get; set; }

		public NConsoleDialog()
		{
			Functions = new Action[12];
		}

		public HashSet<object> Read() => NConsole.ReadOptions(this);

		public Task<HashSet<object>> ReadAsync() => NConsole.ReadOptionsAsync(this);

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