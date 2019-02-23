using DataTables.NetStandard.Enhanced.Filters;

namespace DataTables.NetStandard.Enhanced
{
    public class EnhancedDataTablesColumn<TEntity, TEntityViewModel> : DataTablesColumn<TEntity, TEntityViewModel>
    {
        public IColumnFilter ColumnFilter { get; set; }
    }
}
