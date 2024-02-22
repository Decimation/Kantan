// Deci Kantan MemberIndexTypeAttribute.cs
// $File.CreatedYear-$File.CreatedMonth-22 @ 2:29

using System;
using System.Reflection;

namespace Kantan.Model.MemberIndex;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class MemberIndexTypeAttribute : Attribute
{

	public static BindingFlags DefaultFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
	                                          BindingFlags.DeclaredOnly;

	public MemberIndexMode Mode { get; set; } = MemberIndexMode.Inclusive;

	public BindingFlags Flags { get; set; } = DefaultFlags;

	public Type Formatter { get; set; }

}