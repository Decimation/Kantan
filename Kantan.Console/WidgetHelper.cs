global using SText = Spectre.Console.Text;
using Spectre.Console;
using Spectre.Console.Rendering;

// // global using Text = global::Spectre.Console.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Kantan.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using String = System.String;

namespace Kantan.Console;

public static class WidgetHelper
{

	extension(Table t)
	{

		public bool IsColumnEmpty(int i)
		{
			var re1 = t.Rows.All((r) =>
			{
				return r[i] switch
				{
					Markup m => m.Length == 0,
					SText t  => t.Length == 0,
					_        => false
				};
			});

			return re1;
		}

		public Table RemoveEmpty()
		{
			for (int i = 0; i < t.Columns.Count; i++) {
				if (t.IsColumnEmpty(i)) {
					t = t.RemoveColumn(i);
				}
			}

			return t;
		}

		public Table RemoveColumn(int i)
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

		public Table Copy()
		{
			var t2 = new Table()
			{
				Title             = t.Title,
				Alignment         = t.Alignment,
				Border            = t.Border,
				BorderStyle       = t.BorderStyle,
				Caption           = t.Caption,
				Expand            = t.Expand,
				ShowFooters       = t.ShowFooters,
				ShowHeaders       = t.ShowHeaders,
				UseSafeBorder     = t.UseSafeBorder,
				Width             = t.Width,
				ShowRowSeparators = t.ShowRowSeparators
			};

			return t2;
		}

	}

	public static Table ToSpcTable(this DataTable dt)
	{
		var t = new Table();

		foreach (DataColumn row in dt.Columns) {
			t.AddColumn(new TableColumn(row.ColumnName));
		}

		Func<object, IRenderable> selector = RenderableUtility.AsRenderable;

		foreach (DataRow row in dt.Rows) {
			var obj = row.ItemArray.Select(selector);

			t.AddRow(obj);
		}

		return t;
	}

	internal static Grid AddRowsByChunk(this Grid g, int cnt, params IEnumerable<IRenderable> items)
	{
		var chunks = items.Chunk(cnt);

		foreach (IRenderable[] chunk in chunks) {
			g.AddRow(chunk);
		}

		return g;
	}

	internal static Grid MapToGrid<TKey, TValue>(IDictionary<TKey, TValue>       dictionary,
	                                             [CBN] Func<TKey, IRenderable>   keyFunc = null,
	                                             [CBN] Func<TValue, IRenderable> valFunc = null)
	{
		var grd = new Grid();
		grd.AddColumns(2);

		keyFunc ??= static k =>
		{
			//
			var s = k.ToString();
			ArgumentNullException.ThrowIfNull(s);
			return new SText(s);
		};

		valFunc ??= RenderableUtility.AsRenderable;

		foreach (var (k, v) in dictionary) {
			grd.AddRow(keyFunc(k), valFunc(v));
		}

		return grd;
	}

}