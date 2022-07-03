#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Kantan.Files;
using Kantan.Net.Utilities;
using Kantan.Text;
using Kantan.Utilities;

#endregion

namespace Kantan.Net.Content;

/// <remarks>
///     <a href="https://mimesniff.spec.whatwg.org/#handling-a-resource">5</a>
/// </remarks>
public sealed class HttpResourceHandle : ResourceHandle
{
	public bool CheckBugFlag { get; init; }

	public bool NoSniffFlag { get; init; }

	public IFlurlResponse Response { get; init; }
	

	#region Overrides of ResourceHandle

	

	#endregion

	public override string ToString()
	{
		return
			$"{Value} ({IsBinaryType}) :: supplied: {SuppliedType} | computed: {ComputedType} | resolved: {ResolvedTypes.QuickJoin()}";
	}

	public override void Dispose()
	{
		Response?.Dispose();
		Stream?.Dispose();
	}
}