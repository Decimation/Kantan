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

		

		protected Enumeration(int id,string name)
		{
			Id   = id;
			Name = name;

			Update();
			
		}

		private void Update()
		{
			var fields = GetAllFields();

			foreach (var info in fields) {
				var     p     = info.FieldType.GetProperty("Name");
				var o = p.GetValue(this);

				if (o is null) {
					p.SetValue(this,info.Name);
					Debug.WriteLine($"{info.Name}");
				}
			}
		}

		public static MemberInfo memberof<T>(Expression<Func<T>> expression)
		{
			var body = (MemberExpression)expression.Body;
			return body.Member;
		}

		public static FieldInfo fieldof<T>(Expression<Func<T>> expression)
		{
			return (FieldInfo)memberof(expression);
		}

		public static PropertyInfo propertyof<T>(Expression<Func<T>> expression)
		{
			return (PropertyInfo)memberof(expression);
		}
		public virtual int GetNextId()
		{
			var all = GetAll<Enumeration>();

			var last = all.OrderBy(x => x.Id).Last();

			return last.Id + 1;
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


		public IEnumerable<TEnumeration> GetAll<TEnumeration>()
			where TEnumeration : Enumeration
			=> GetAllFields().Select(x => x.GetValue(this)).Cast<TEnumeration>();


		protected IEnumerable<FieldInfo> GetAllFields()
			=> GetType().GetRuntimeFields().Where(x => x.FieldType == GetType() && x.IsStatic && x.IsInitOnly);
		
	}
}