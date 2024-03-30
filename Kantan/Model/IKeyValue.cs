// Read S Kantan IKeyValue.cs
// 2023-06-04 @ 6:47 PM

using System;
using System.Collections.Generic;

namespace Kantan.Model;

/*public interface IKeyValue : IKeyValue<string, object>
{

}*/
#if OBSOLETE

[Obsolete]
public interface IKeyValue<TKey, TValue>
{

	public IList<KeyValuePair<TKey, TValue>> GetKeyValues();

}
#endif
