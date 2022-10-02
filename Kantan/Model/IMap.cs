using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Kantan.Text;
using Kantan.Utilities;

namespace Kantan.Model;

public interface IMap
{
	public Dictionary<string, object> Data { get;  }
}