using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DataTables.NetCore.Abstract;
using DataTables.NetCore.Configuration;
using DataTables.NetCore.Extensions;
using DataTables.NetCore.Util;

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
            _tableIdentifier = BuildTableIdentifier();
        }

        /// <summary>
        /// DataTable constructor. Uses the given table identifier.
        /// </summary>
        /// <param name="tableIdentifier"></param>
        public DataTable(string tableIdentifier)
        {
            _tableIdentifier = tableIdentifier;
        }

        public abstract IList<DataTablesColumn<TEntity, TEntityViewModel>> Columns();
        public abstract IQueryable<TEntity> Query();
        public abstract Expression<Func<TEntity, TEntityViewModel>> MappingFunction();

        public string GetTableIdentifier()
        {
            return _tableIdentifier;
        }

        public virtual DataTablesResponse<TEntity, TEntityViewModel> RenderResponse(string query)
        {
            var data = RenderResults(query);

            return new DataTablesResponse<TEntity, TEntityViewModel>(data, Columns());
        }

        public virtual IPagedList<TEntityViewModel> RenderResults(string query)
        {
            var request = new DataTablesRequest<TEntity, TEntityViewModel>(query, Columns(), MappingFunction());

            return Query().ToPagedList(request);
        }

        public virtual string RenderScript(string url, string method = "get")
        {
            return DataTablesConfigurationBuilder.BuildDataTableConfigurationScript(Columns(), GetTableIdentifier(), url, method, AdditionalDataTableOptions());
        }

        public virtual string RenderHtml()
        {
            var sb = new StringBuilder();

            sb.Append($"<table id=\"{GetTableIdentifier()}\">");

            sb.Append(RenderTableHeader());
            sb.Append(RenderTableBody());
            sb.Append(RenderTableFooter());

            sb.Append("</table>");

            return sb.ToString();
        }

        /// <summary>
        /// Additional data table options. The options are serialized as they are without changing PascalCase to
        /// camelCase or something similar.
        /// </summary>
        public virtual IDictionary<string, dynamic> AdditionalDataTableOptions()
        {
            return new Dictionary<string, dynamic>();
        }

        /// <summary>
        /// Returns a list of distinct column values that can be used for select filters.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <exception cref="ArgumentException">No column with public name <paramref name="columnName"/> available.</exception>
        public virtual IList<string> GetDistinctColumnValues(string columnName)
        {
            var column = Columns().FirstOrDefault(c => c.PublicName == columnName);
            if (column == null)
            {
                throw new ArgumentException($"No column with public name {columnName} available.");
            }

            return GetDistinctColumnValues(column);
        }

        /// <summary>
        /// Returns a list of distinct column values that can be used for select filters.
        /// </summary>
        /// <param name="column">The column.</param>
        public virtual IList<string> GetDistinctColumnValues(DataTablesColumn<TEntity, TEntityViewModel> column)
        {
            var parameterExp = ExpressionHelper.BuildParameterExpression<TEntity>();
            var propertyExp = ExpressionHelper.BuildPropertyExpression(parameterExp, column.PrivatePropertyName);
            var stringExp = Expression.Call(propertyExp, ExpressionHelper.Object_ToString);
            var lambda = Expression.Lambda<Func<TEntity, string>>(stringExp, parameterExp);

            return GetDistinctColumnValues(lambda);
        }

        /// <summary>
        /// Returns a list of distinct column values that can be used for select filters.
        /// </summary>
        /// <param name="property"></param>
        public virtual IList<string> GetDistinctColumnValues(Expression<Func<TEntity, string>> property)
        {
            return Query().Select(property).Distinct().ToList();
        }

        /// <summary>
        /// Renders the table header. Can be overwritten to change the rendering.
        /// </summary>
        protected virtual string RenderTableHeader()
        {
            var sb = new StringBuilder();

            sb.Append("<thead>");
            sb.Append("<tr>");

            foreach (var column in Columns())
            {
                sb.Append(RenderTableHeaderColumn(column));
            }

            sb.Append("</tr>");
            sb.Append("</thead>");

            return sb.ToString();
        }

        /// <summary>
        /// Renders a table header column. Can be overwritten to change the rendering.
        /// It is also possible to return a different template based on the given column.
        /// </summary>
        /// <param name="column">The column.</param>
        protected virtual string RenderTableHeaderColumn(DataTablesColumn<TEntity, TEntityViewModel> column)
        {
            return $"<th>{column.DisplayName}</th>";
        }

        /// <summary>
        /// Renders the table body. Can be overwritten to change the rendering.
        /// </summary>
        protected virtual string RenderTableBody()
        {
            return "<td></td>";
        }

        /// <summary>
        /// Renders the table footer. Can be overwritten to change the rendering.
        /// </summary>
        protected virtual string RenderTableFooter()
        {
            var sb = new StringBuilder();

            sb.Append("<tfoot>");
            sb.Append("<tr>");

            foreach (var column in Columns())
            {
                sb.Append(RenderTableFooterColumn(column));
            }

            sb.Append("</tr>");
            sb.Append("</tfoot>");

            return sb.ToString();
        }

        /// <summary>
        /// Renders the table footer column. Can be overwritten to change the rendering.
        /// It is also possible to return a different template based on the given column.
        /// </summary>
        /// <param name="column">The column.</param>
        protected virtual string RenderTableFooterColumn(DataTablesColumn<TEntity, TEntityViewModel> column)
        {
            return "<th></th>";
        }

        /// <summary>
        /// Returns the identifier for the table. Can be something static or a generated
        /// value that ensures uniqueness.
        /// </summary>
        protected virtual string BuildTableIdentifier()
        {
            return GetType().Name;
        }
    }
}
