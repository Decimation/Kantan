using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Enum = System.Enum;

// ReSharper disable NonReadonlyMemberInGetHashCode

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

		public virtual int GetNextId()
		{

			var last = GetAll<Enumeration>().OrderBy(x => x.Id).Last();

			return last.Id + 1;
		}

		public static IEnumerable<TEnumeration> GetAll<TEnumeration>()
			where TEnumeration : Enumeration
			=> GetAllFields<TEnumeration>().Select(x => x.GetValue(null)).Cast<TEnumeration>();

		public static IEnumerable<FieldInfo> GetAllFields<T>()
			=> typeof(T).GetRuntimeFields().Where(x => x.FieldType == typeof(T) /* && x.IsStatic && x.IsInitOnly*/);

		public override bool Equals(object obj)
		{
			return obj is Enumeration otherValue && Equals(otherValue);
		}

		protected bool Equals(Enumeration other)
		{
			return Name == other.Name && Id == other.Id;
		}

		public int CompareTo(object other) => Id.CompareTo(((Enumeration) other).Id);

		public override int GetHashCode()
		{
			return HashCode.Combine(Name, Id);
		}

		public override string ToString() => $"{Name} ({Id})";
	}
}