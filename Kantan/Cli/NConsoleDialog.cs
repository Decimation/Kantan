using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Kantan.Cli
{
	public class NConsoleDialog
	{
		public IList<NConsoleOption> Options { get; init; }

		public bool SelectMultiple { get; init; }

		public string Header { get; set; }

		[CanBeNull]
		public string Status { get; set; }

		[CanBeNull]
		public string Description { get; set; }


		

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
			return h.ToHashCode();
		}
	}
}