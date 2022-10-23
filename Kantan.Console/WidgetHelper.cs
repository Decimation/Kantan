#if SPECTRE

global using Text = global::Spectre.Console.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;

namespace Kantan.Console;

public static class WidgetHelper
{
	public static Spectre.Console.Text T(this string s, Style st = null)
	{
		return new Spectre.Console.Text(s, st);
	}

	public static bool IsColumnEmpty(this Table t, int i)
	{
		var re1 = t.Rows.All((r) =>
		{
			return r[i] switch
			{
				Markup m => m.Length == 0,

				Spectre.Console.Text t => t.Length == 0,
				_                      => false
			};
		});

		return re1;
	}

	public static Table RemoveEmpty(this Table t)
	{
		for (int i = 0; i < t.Columns.Count; i++) {
			if (t.IsColumnEmpty(i)) {
				t = t.RemoveColumn(i);
			}
		}

		return t;
	}
	public static Table RemoveColumn(this Table t, int i)
	{
		Table t2 = t.Copy();

		for (int j = 0; j < t.Columns.Count; j++) {
			if (j == i) {
				continue;
			}

			t2.AddColumn(t.Columns[j]);
		}

		using var re = t.Rows.GetEnumerator();

		while (re.MoveNext()) {
			var r  = re.Current;
			var rr = r.ToList();
			rr.RemoveAt(i);

			// var re2=r.GetEnumerator();
			t2.AddRow(rr);
		}

		return t2;
	}

	public static Table Copy(this Table t)
	{
		var t2 = new Table()
		{
			Title         = t.Title,
			Alignment     = t.Alignment,
			Border        = t.Border,
			BorderStyle   = t.BorderStyle,
			Caption       = t.Caption,
			Expand        = t.Expand,
			ShowFooters   = t.ShowFooters,
			ShowHeaders   = t.ShowHeaders,
			UseSafeBorder = t.UseSafeBorder,
			Width         = t.Width,
		};

		return t2;
	}
}
#endif
