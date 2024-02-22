// Deci Kantan MemberExportAttribute.cs
// $File.CreatedYear-$File.CreatedMonth-22 @ 1:48

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kantan.Model.MemberIndex;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
public class MemberIndexAttribute : Attribute
{

    public bool Include { get; set; }

    public Type Formatter { get; set; }

    public MemberIndexAttribute(bool include = true, Type formatter = null)
    {
        Include = include;
        Formatter = formatter;
    }

}

public enum MemberIndexMode
{

    Inclusive,
    Exclusive

}

