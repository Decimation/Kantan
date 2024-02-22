using System;
using System.Data;

namespace Kantan.Model;

[Obsolete]
public interface IDataTable
{
	public DataTable ToTable();
}