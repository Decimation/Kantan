using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Enum = System.Enum;

// ReSharper disable NonReadonlyMemberInGetHashCode

// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace Kantan.Model;

public abstract class Enumeration : Enumeration<int>
{

	protected Enumeration(int id, string name) : base(id, name) { }

}

public abstract class Enumeration<T> : IComparable where T : INumber<T>, IAdditionOperators<T, T, T>, IComparable<T>
{

	public string Name { get; protected set; }

	public T Id { get; protected set; }

	protected Enumeration(T id, string name)
	{
		Id   = id;
		Name = name;
	}

	public static T GetNextId<TEnumeration>() where TEnumeration : Enumeration<T>
	{
		var last = GetAll<TEnumeration>().Where(x => x != null).OrderBy(x => x.Id).Last();

		return last.Id + T.One;
	}

	#region 

	public static IEnumerable<Enumeration<T>> GetAll(Type t)
		=> GetAllFields(t).Select(x => x.GetValue(null)).Cast<Enumeration<T>>();

	public static IEnumerable<TEnumeration> GetAll<TEnumeration>()
		where TEnumeration : Enumeration<T>
		=> GetAll(typeof(TEnumeration)).Cast<TEnumeration>();

	public static IEnumerable<FieldInfo> GetAllFields(Type t)
	{
		return t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
			.Where(r => r.FieldType == t);
	}

	public static IEnumerable<FieldInfo> GetAllFields<TEnumeration>()
		where TEnumeration : Enumeration<T>
		=> GetAllFields(typeof(TEnumeration));

	#endregion

	public override bool Equals(object obj)
	{
		return obj is Enumeration<T> otherValue && Equals(otherValue);
	}

	protected bool Equals(Enumeration<T> other)
	{
		return Name == other.Name && Id == other.Id;
	}

	public int CompareTo(object other)
		=> Id.CompareTo(((Enumeration<T>) other).Id);

	public override int GetHashCode()
	{
		return HashCode.Combine(Name, Id);
	}

	public override string ToString()
		=> $"{Name} ({Id})";

}