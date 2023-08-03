// Read S Kantan IKeyValue.cs
// 2023-06-04 @ 6:47 PM

using System.Collections.Generic;

namespace Kantan.Model;

public interface IKeyValue
{
	public KeyValuePair<string, object>[] KeyValues { get; }
}