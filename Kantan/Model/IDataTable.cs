using System.Data;

namespace Kantan.Model;

public interface IDataTable
{
	public DataTable ToTable();
}