using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DataTables.NetStandard.Configuration;
using DataTables.NetStandard.Extensions;
using DataTables.NetStandard.Util;

namespace DataTables.NetStandard
{
    /// <summary>
    /// Base class for DataTables. Offers a default implementations for most required methods.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
    public abstract class DataTable<TEntity, TEntityViewModel>
    {
        protected string _tableIdentifier;

        /// <summary>
        /// DataTable constructor. Gets and stores the table identifier.
        /// </summary>
        public DataTable()
        {
            SetTableIdentifier(BuildTableIdentifier());
        }

        /// <summary>
        /// Column definitions for this DataTable.
        /// </summary>
        public abstract IList<DataTablesColumn<TEntity, TEntityViewModel>> Columns();

        /// <summary>
        /// Gets the query used to fetch data for the DataTable.
        /// </summary>
        public abstract IQueryable<TEntity> Query();

        /// <summary>
        /// A mapping function used to map query models to view models.
        /// </summary>
        /// <returns></returns>
        public abstract Expression<Func<TEntity, TEntityViewModel>> MappingFunction();

        /// <summary>
        /// Gets the table identifier.
        /// </summary>
        public string GetTableIdentifier()
        {
            return _tableIdentifier;
        }

        /// <summary>
        /// Sets the table identifier. Should only be used in the constructor.
        /// </summary>
        /// <param name="tableIdentifier">The table identifier.</param>
        protected void SetTableIdentifier(string tableIdentifier)
        {
            _tableIdentifier = tableIdentifier;
        }

        /// <summary>
        /// Renders the results based on the given <see cref="DataTablesRequest{TEntity, TEntityViewModel}"/>
        /// and builds a response that can be returned immediately.
        /// </summary>
        /// <param name="query">The query.</param>
        public virtual DataTablesResponse<TEntity, TEntityViewModel> RenderResponse(string query)
        {
            var request = BuildRequest(query);
            var data = RenderResults(request);

            return new DataTablesResponse<TEntity, TEntityViewModel>(data, Columns(), request.Draw);
        }

        /// <summary>
        /// Renders the results based on a <see cref="DataTablesRequest{TEntity, TEntityViewModel}"/>
        /// built from the given <paramref name="query"/>.
        /// </summary>
        /// <param name="query">The query.</param>
        public virtual IPagedList<TEntityViewModel> RenderResults(string query)
        {
            return Query().ToPagedList(BuildRequest(query));
        }

        /// <summary>
        /// Renders the results based on the given <see cref="DataTablesRequest{TEntity, TEntityViewModel}"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public virtual IPagedList<TEntityViewModel> RenderResults(DataTablesRequest<TEntity, TEntityViewModel> request)
        {
            return Query().ToPagedList(request);
        }

        /// <summary>
        /// Renders the script.
        /// </summary>
        /// <param name="url">The url of the data endpoint for the DataTable</param>
        /// <param name="method">The http method used for the data endpoint (get or post)</param>
        public virtual string RenderScript(string url, string method = "get")
        {
            return DataTablesConfigurationBuilder.BuildDataTableConfigurationScript(Columns(), GetTableIdentifier(), url, method, AdditionalDataTableOptions());
        }

        /// <summary>
        /// Renders the HTML.
        /// </summary>
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

        /// <summary>
        /// Builds a <see cref="DataTablesRequest{TEntity, TEntityViewModel}"/> based on the given
        /// <paramref name="query"/>, <see cref="Columns"/> and <see cref="MappingFunction"/>.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        protected virtual DataTablesRequest<TEntity, TEntityViewModel> BuildRequest(string query)
        {
            return new DataTablesRequest<TEntity, TEntityViewModel>(query, Columns(), MappingFunction());
        }
    }
}
