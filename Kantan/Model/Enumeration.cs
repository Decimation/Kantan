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

		public static int GetNextId<TEnumeration>() where TEnumeration : Enumeration
		{
			var last = GetAll<TEnumeration>().OrderBy(x => x.Id).Last();

			return last.Id + 1;
		}

		public static IEnumerable<Enumeration> GetAll(Type t)
			=> GetAllFields(t).Select(x => x.GetValue(null)).Cast<Enumeration>();

		public static IEnumerable<TEnumeration> GetAll<TEnumeration>()
			where TEnumeration : Enumeration
			=> GetAll(typeof(TEnumeration)).Cast<TEnumeration>();

		public static IEnumerable<FieldInfo> GetAllFields(Type t)
			=> t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
			    .Where(r => r.FieldType == t);

		public static IEnumerable<FieldInfo> GetAllFields<TEnumeration>()
			where TEnumeration : Enumeration
			=> GetAllFields(typeof(TEnumeration));

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