// Author: Deci | Project: Kantan.Net | Name: FirefoxCookie.cs
// Date: $File.CreatedYear-$File.CreatedMonth-22 @ 0:24:39

using System;
using System.Data;
using System.Net;
using Flurl.Http;
using JetBrains.Annotations;
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace Kantan.Net.Web;

public class FirefoxCookie : ICookie
{

	public long Id { get; }

	public string Attribute1 { get; }

	public string Name { get; }

	public string Value { get; }

	public bool IsSecure { get; }

	public bool IsHttpOnly { get; }

	public string Host { get; }

	public string Path { get; }

	public DateTimeOffset Expiry { get; }

	public DateTime LastAccess { get; }

	public DateTime CreationTime { get; }

	public bool InBrowserElement { get; }

	public SameSite SameSite { get; }

	public bool RawSameSite { get; }

	public int SchemeMap { get; }

	public bool IsPartitionedAttributeSet { get; }

/*
 *
 *
 *
 *
 * Name = 'id' AND "Data Type" = 'INTEGER'
   OR Name = 'originAttributes' AND "Data Type" = 'TEXT'
   OR Name = 'name' AND "Data Type" = 'TEXT'
   OR Name = 'value' AND "Data Type" = 'TEXT'
   OR Name = 'host' AND "Data Type" = 'TEXT'
   OR Name = 'path' AND "Data Type" = 'TEXT'
   OR Name = 'expiry' AND "Data Type" = 'INTEGER'
   OR Name = 'lastAccessed' AND "Data Type" = 'INTEGER'
   OR Name = 'creationTime' AND "Data Type" = 'INTEGER'
   OR Name = 'isSecure' AND "Data Type" = 'INTEGER'
   OR Name = 'isHttpOnly' AND "Data Type" = 'INTEGER'
   OR Name = 'inBrowserElement' AND "Data Type" = 'INTEGER'
   OR Name = 'sameSite' AND "Data Type" = 'INTEGER'
   OR Name = 'schemeMap' AND "Data Type" = 'INTEGER'
   OR Name = 'isPartitionedAttributeSet' AND "Data Type" = 'INTEGER'
 *
 */
	public FirefoxCookie(IDataRecord reader)
	{
		Id           = reader.GetInt64(0);
		Attribute1   = reader.GetString(1);
		Name         = reader.GetString(2);
		Value        = reader.GetString(3);
		Host         = reader.GetString(4);
		Path         = reader.GetString(5);
		Expiry       = (DateTime.UnixEpoch + TimeSpan.FromSeconds(reader.GetInt64(6)));
		LastAccess   = (DateTime.UnixEpoch + TimeSpan.FromMicroseconds(reader.GetInt64(7)));
		CreationTime = DateTime.UnixEpoch + TimeSpan.FromMicroseconds(reader.GetInt64(8));
		IsSecure     = reader.GetBoolean(9);
		IsHttpOnly   = reader.GetBoolean(10);

		InBrowserElement = reader.GetBoolean(11);
		SameSite         = (SameSite) reader.GetInt32(12);
		/*RawSameSite               = reader.GetBoolean(13);
		SchemeMap                 = reader.GetInt32(14);
		IsPartitionedAttributeSet = reader.GetBoolean(15);*/
		// RawSameSite               = reader.GetBoolean(13);
		SchemeMap                 = reader.GetInt32(13);
		IsPartitionedAttributeSet = reader.GetBoolean(14);
	}

	public Cookie AsCookie()
	{

		ArgumentException.ThrowIfNullOrWhiteSpace(Name);

		var cc = new Cookie()
		{
			HttpOnly = IsHttpOnly,
			Name     = Name,
			Domain   = Host,
			Value    = Value,
			Path     = Path,
			Secure   = IsSecure,
			Expires  = Expiry.DateTime,
		};

		return cc;
	}


	public FlurlCookie AsFlurlCookie([CBN] string originUrl = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(Name);

		var fk = new FlurlCookie(Name, Value, originUrl, CreationTime)
		{
			Domain   = Host,
			HttpOnly = IsHttpOnly,
			Secure   = IsSecure,
			SameSite = SameSite,
			Path     = Path,
			Expires  = Expiry,
		};

		return fk;
	}

}