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
		protected FlagsEnumeration(int id, string name) : base(id, name)
		{
		}
		
		public override int GetNextId()
		{
			var all = GetAll<FlagsEnumeration>();

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

		public FlagsEnumeration Xor(FlagsEnumeration f)
		{
			Id ^= f.Id;
			return this;
		}

		public bool HasFlag(FlagsEnumeration f)
		{
			return (Id & f.Id) != 0;
		}

		public abstract FlagsEnumeration Copy();

		public static FlagsEnumeration operator &(FlagsEnumeration f, FlagsEnumeration f2) => f.Copy().And(f2);

		public static FlagsEnumeration operator |(FlagsEnumeration f, FlagsEnumeration f2) => f.Copy().Or(f2);

		public static FlagsEnumeration operator ^(FlagsEnumeration f, FlagsEnumeration f2) => f.Copy().Xor(f2);

		public static FlagsEnumeration operator ~(FlagsEnumeration f) => f.Copy().Not();

		public override string ToString()
		{
			// var a     = GetAll<FlagsEnumeration>(GetType());
			// var a = GetType().GetRuntimeFields().Where(x => x.FieldType == GetType()).Select(x=>x.GetValue(null)).Cast<FlagsEnumeration>();

			// var s = a.Where(x => x.HasFlag(this)).Select(s => s.Name).QuickJoin();

			return $"{Name} ({Id}) ({Convert.ToString(Id, 2)})";
		}
	}
}