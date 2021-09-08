// ReSharper disable UnusedMember.Global

namespace Kantan.Text
{
	public static partial class Strings
	{
		public static class Constants
		{
			#region Join

			/// <summary>
			///     Scope resolution operator
			/// </summary>
			public const string JOIN_SCOPE = "::";
			public const string JOIN_SPACE       = " ";
			public const string JOIN_COMMA       = ", ";

			#endregion

			#region Punctuation

			public const char EXCLAMATION      = '!';
			public const char SPACE            = ' ';
			public const char ASTERISK         = '*';
			public const char PERIOD           = '.';

			#endregion

			#region Arrow

			public const char ARROW_LEFT_RIGHT = '\u2194';
			public const char ARROW_RIGHT      = '\u2192';
			public const char ARROW_DOWN       = '\u2193';
			public const char ARROW_LEFT       = '\u2190';
			public const char ARROW_UP         = '\u2191';
			public const char ARROW_UP_DOWN    = '\u2195';

			#endregion

			#region Control

			public const char NULL_CHAR = '\0';
			public const char NEW_LINE  = '\n';

			#endregion

			public const char BALLOT_X         = '\u2717';
			public const char HEAVY_CHECK_MARK = '\u2714';
			public const char LOZENGE          = '\u25ca';
			public const char RAD_SIGN         = '\u221A';
			public const char RELOAD           = '\u21bb';
			public const char HEAVY_BALLOT_X   = '\u2718';
			public const char CHECK_MARK       = '\u2713';
			public const char MUL_SIGN         = '\u00D7';
			public const char MUL_SIGN2        = '\u2715';
			public const char SUN              = '\u263c';

			public const string CHEVRON          = ">>";
			public const string ELLIPSES         = "...";

			/// <summary>
			/// Constant <see cref="string.Empty"/>
			/// </summary>
			public const string Empty = "";

			public const  string Alphanumeric          = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

			#region Box

			public const  string UpperLeftCorner       = "\u250c";
			public const  string BottomLeftCorner      = "\u2514";
			public const  string HorizontalMidVertical = "\u251c";
			public const  string Vertical              = "\u2502";
			public const  string Horizontal            = "\u2500";

			#endregion

			public static string Separator   { get; set; } = new('-', 20);
			public static string Indentation { get; set; } = new(' ', 5);
		}
	}
}