using System;
using System.Linq;
using DataTables.NetCore.Abstract;
using DataTables.NetCore.Builder;
using DataTables.NetCore.Extensions;

namespace DataTables.NetCore
{
    /// <summary>
    /// Base class for DataTables. Offers a default implementations for most required methods.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
    /// <seealso cref="Abstract.IDataTable{TEntity, TEntityViewModel}" />
    public abstract class DataTable<TEntity, TEntityViewModel> : IDataTable<TEntity, TEntityViewModel>
    {
        protected readonly string _tableIdentifier;

        /// <summary>
        /// DataTable constructor. Gets and stores the table identifier.
        /// </summary>
        public DataTable()
        {
            _tableIdentifier = GetTableIdentifier();
        }

        public abstract IDataTablesColumnsCollection<TEntity, TEntityViewModel> Columns();
        public abstract IQueryable<TEntity> Query();

        public DataTablesResponse<TEntity, TEntityViewModel> RenderResponse(string query)
        {
            var data = RenderResults(query);

            return new DataTablesResponse<TEntity, TEntityViewModel>(data, Columns());
        }

        public IPagedList<TEntityViewModel> RenderResults(string query)
        {
            var request = new DataTablesRequest<TEntity, TEntityViewModel>(query, Columns());

            return Query().ToPagedList(request);
        }

        public string RenderScript(string url, string method = "get")
        {
            return DataTablesConfigurationBuilder.BuildDataTableConfigurationScript(Columns(), _tableIdentifier, url, method);
        }

        public string RenderHtml()
        {
            return $"<table id=\"{_tableIdentifier}\"></table>";
        }

        /// <summary>
        /// Returns the identifier for the table. Can be something static or a generated
        /// value that ensures uniqueness.
        /// </summary>
        protected string GetTableIdentifier()
        {
            return $"{GetType().Name}_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}";
        }
    }
}
