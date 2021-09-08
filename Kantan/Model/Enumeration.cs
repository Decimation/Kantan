using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Enum = System.Enum;

// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace Kantan.Model
{
	public abstract class Enumeration : IComparable
	{
		public string Name { get; protected set; }

		public int Id { get; protected set; }

		protected Enumeration(int id, string name)
		{
			Id   = id;
			Name = name;
		}

		public static int GetNextId<T>() where T : Enumeration
		{
			var all = GetAll<T>();

			var last = all.OrderBy(x => x.Id).Last();

			return last.Id + 1;
		}

		public static IEnumerable<T> GetAll<T>() where T : Enumeration
		{
			return GetAll<T>(typeof(T));
		}

		public static IEnumerable<T> GetAll<T>(Type t) where T : Enumeration
		{
			const BindingFlags flags = BindingFlags.Public |
			                           BindingFlags.Static |
			                           BindingFlags.DeclaredOnly;

			var fields = typeof(T).GetFields(flags).Where(r => r.FieldType == t);

			return fields.Select(f => f.GetValue(null)).Cast<T>();
		}

		public override string ToString() => $"{Name} ({Id})";

		public override bool Equals(object obj)
		{
			return obj is Enumeration otherValue && Equals(otherValue);
		}

		protected bool Equals(Enumeration other)
		{
			return Name == other.Name && Id == other.Id;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Name, Id);
		}

		public int CompareTo(object other) => Id.CompareTo(((Enumeration) other).Id);

		// Other utility methods ...
	}
}