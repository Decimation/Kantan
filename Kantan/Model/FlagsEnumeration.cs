using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Kantan.Text;

// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace Kantan.Model
{
	public abstract class FlagsEnumeration : Enumeration
	{
		protected FlagsEnumeration(int id, string name) : base(id, name) { }

		public int GetNextFlagId()
		{
			var all = GetAll<FlagsEnumeration>(GetType());

			// a =a.OrderBy(x => BitOperations.TrailingZeroCount((uint) x.Id));

			all = all.OrderByDescending(x => x.Id);

			return all.First().Id << 1;
		}

		public FlagsEnumeration Or(FlagsEnumeration f)
		{
			Id |= f.Id;
			return this;
		}

		public FlagsEnumeration And(FlagsEnumeration f)
		{
			Id &= f.Id;
			return this;
		}

		public FlagsEnumeration Not()
		{
			Id = ~Id;
			return this;
		}

		public bool HasFlag(FlagsEnumeration f)
		{
			return (Id & f.Id) != 0;
		}

		public abstract FlagsEnumeration Copy();

		public static FlagsEnumeration operator &(FlagsEnumeration f, FlagsEnumeration f2) => f.Copy().And(f2);

		public static FlagsEnumeration operator |(FlagsEnumeration f, FlagsEnumeration f2) => f.Copy().Or(f2);

		public static   FlagsEnumeration operator ~(FlagsEnumeration f) => f.Copy().Not();
		public override string ToString()                               => $"{Name} ({Id})";
	}
}