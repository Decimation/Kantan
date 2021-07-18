using System.Collections.Generic;

// ReSharper disable UnusedMember.Global

namespace Kantan.Model
{
	public interface IViewable
	{
		public Dictionary<string, object> View { get; }
	}
}