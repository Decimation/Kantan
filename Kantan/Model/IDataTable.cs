using System;
using System.Data;

namespace Kantan.Model;
#if OBSOLETE

[Obsolete]
public interface IDataTable
{
	public DataTable ToTable();
}
#endif