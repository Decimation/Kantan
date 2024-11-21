// Author: Deci | Project: Kantan.Net | Name: FirefoxCookie.cs
// Date: $File.CreatedYear-$File.CreatedMonth-22 @ 0:24:39

using System;
using System.Data;
using System.Net;
using Flurl.Http;

namespace Kantan.Net.Web;

public class FirefoxCookie : IBrowserCookie
{

	public Cookie AsCookie()
	{
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

	public FlurlCookie AsFlurlCookie()
	{
		var fk = new FlurlCookie(Name, Value, null, CreationTime)
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

	public long           Id;
	public string         Attribute;
	public string         Name;
	public string         Value;
	public string         Host;
	public string         Path;
	public DateTimeOffset Expiry;
	public DateTime       LastAccess;
	public DateTime       CreationTime;
	public bool           IsSecure;
	public bool           IsHttpOnly;

	public bool InBrowserElement;

	public SameSite SameSite;

	public bool RawSameSite;

	public int SchemeMap;

	public bool IsPartitionedAttributeSet;

	public FirefoxCookie(IDataRecord reader)
	{
		Id           = reader.GetInt64(0);
		Attribute    = reader.GetString(1);
		Name         = reader.GetString(2);
		Value        = reader.GetString(3);
		Host         = reader.GetString(4);
		Path         = reader.GetString(5);
		Expiry       = (DateTime.UnixEpoch + TimeSpan.FromSeconds(reader.GetInt64(6)));
		LastAccess   = (DateTime.UnixEpoch + TimeSpan.FromMicroseconds(reader.GetInt64(7)));
		CreationTime = DateTime.UnixEpoch + TimeSpan.FromMicroseconds(reader.GetInt64(8));
		IsSecure     = reader.GetBoolean(9);
		IsHttpOnly   = reader.GetBoolean(10);

		InBrowserElement          = reader.GetBoolean(11);
		SameSite                  = (SameSite) reader.GetInt32(12);
		RawSameSite               = reader.GetBoolean(13);
		SchemeMap                 = reader.GetInt32(14);
		IsPartitionedAttributeSet = reader.GetBoolean(15);
	}

}