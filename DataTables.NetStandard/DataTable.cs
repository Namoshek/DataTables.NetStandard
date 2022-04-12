using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DataTables.NetStandard.Extensions;
using DataTables.NetStandard.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
        protected DataTablesConfiguration _configuration;
        protected bool _isConfigured = false;

        /// <summary>
        /// DataTable constructor. Gets and stores the table identifier.
        /// </summary>
        public DataTable()
        {
            SetTableIdentifier(BuildTableIdentifier());

            _configuration = new DataTablesConfiguration();
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
            Configure();

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
            Configure();

            return Query().ToPagedList(BuildRequest(query));
        }

        /// <summary>
        /// Renders the results based on the given <see cref="DataTablesRequest{TEntity, TEntityViewModel}"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        public virtual IPagedList<TEntityViewModel> RenderResults(DataTablesRequest<TEntity, TEntityViewModel> request)
        {
            Configure();

            return Query().ToPagedList(request);
        }

        /// <summary>
        /// Renders the script.
        /// </summary>
        /// <param name="url">The url of the data endpoint for the DataTable</param>
        /// <param name="method">The http method used for the data endpoint (get or post)</param>
        public virtual string RenderScript(string url, string method = "get")
        {
            Configure();

            return BuildConfigurationScript(GetTableIdentifier(), url, method);
        }

        /// <summary>
        /// Renders the HTML.
        /// </summary>
        public virtual string RenderHtml()
        {
            Configure();

            var sb = new StringBuilder();

            sb.Append($"<table id=\"{GetTableIdentifier()}\">");

            sb.Append(RenderTableHeader());
            sb.Append(RenderTableBody());
            sb.Append(RenderTableFooter());

            sb.Append("</table>");

            return sb.ToString();
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
        /// Builds the configuration script for the DataTable instance.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="url">The URL.</param>
        /// <param name="method">The method.</param>
        public string BuildConfigurationScript(string tableName, string url, string method)
        {
            var configuration = (DataTablesConfiguration)_configuration.Clone();
            configuration.Ajax = new DataTablesConfiguration.AjaxConfiguration
            {
                Url = url,
                Method = method
            };

            var config = JsonConvert.SerializeObject(configuration, GetSerializerSettings());

            return $"var dt_{tableName} = $('#{tableName}').DataTable({config});";
        }

        /// <summary>
        /// Allows to configure the DataTable instance.
        /// </summary>
        protected virtual void Configure()
        {
            if (_isConfigured)
            {
                return;
            }

            var columns = Columns();

            ConfigureColumns(_configuration, columns);
            ConfigureColumnOrdering(_configuration, columns);
            ConfigureAdditionalOptions(_configuration, columns);

            _isConfigured = true;
        }

        /// <summary>
        /// Configures the columns in the DataTable settings which are serialized when rendering the table script.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="columns"></param>
        protected virtual void ConfigureColumns(
            DataTablesConfiguration configuration,
            IList<DataTablesColumn<TEntity, TEntityViewModel>> columns)
        {
            foreach (var column in columns)
            {
                configuration.Columns.Add(new DataTablesConfiguration.DataTablesConfigurationColumn
                {
                    Data = column.PublicName,
                    Name = column.PublicName,
                    Title = column.DisplayName ?? column.PublicName.FirstCharToUpper(),
                    Searchable = column.IsSearchable,
                    Orderable = column.IsOrderable,
                    AdditionalOptions = column.AdditionalOptions.DeepClone()
                });
            }
        }

        /// <summary>
        /// Configures the column ordering in the DataTable settings which are serialized when rendering the table script.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="columns"></param>
        protected virtual void ConfigureColumnOrdering(
            DataTablesConfiguration configuration,
            IList<DataTablesColumn<TEntity, TEntityViewModel>> columns)
        {
            var orderedColumns = columns.Where(c => c.OrderingIndex > -1).OrderBy(c => c.OrderingIndex);
            foreach (var column in orderedColumns)
            {
                configuration.Order.Add(new List<string>
                {
                    columns.IndexOf(column).ToString(),
                    column.OrderingDirection == System.ComponentModel.ListSortDirection.Descending ? "desc" : "asc"
                });
            }
        }

        /// <summary>
        /// Allows to configure additional table options.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="columns"></param>
        protected virtual void ConfigureAdditionalOptions(
            DataTablesConfiguration configuration,
            IList<DataTablesColumn<TEntity, TEntityViewModel>> columns)
        {
            // We don't configure anything here, but we provide a default implementation.
        }

        /// <summary>
        /// Gets the serializer settings used to serialize the DataTables configuration.
        /// </summary>
        protected static JsonSerializerSettings GetSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
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
        protected virtual DataTablesRequest<TEntity, TEntityViewModel> BuildRequest(string query)
        {
            return new DataTablesRequest<TEntity, TEntityViewModel>(query, Columns(), MappingFunction());
        }
    }
}
