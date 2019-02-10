using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web;

namespace DataTables.NetCore
{
    public class DataTablesRequest<TEntity, TEntityViewModel>
    {
        protected const string ColumnPattern = "columns\\[(\\d+)\\]\\[data\\]";
        protected const string ColumnOrderingPattern = "order\\[(\\d)\\]\\[column\\]";

        /// <summary>
        /// 1-based page number (used for pagination of results).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Count of records per page. Negative value means pagination is off.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Draw counter. This is used by DataTables to ensure that the Ajax returns from server-side processing requests are drawn in sequence by DataTables.
        /// </summary>
        public int Draw { get; set; }

        /// <summary>
        /// Global search value. To be applied to all columns which have searchable as true.
        /// Null if no search criteria provided.
        /// </summary>
        public string GlobalSearchValue { get; set; }

        /// <summary>
        /// True if the <see cref="GlobalSearchValue"/> should be treated as a regular expression for advanced searching, false otherwise.
        /// This feature is not implemented yet.
        /// </summary>
        public bool GlobalSearchRegex { get; private set; }

        /// <summary>
        /// Collection of DataTables column info.
        /// Each column can be acessed via indexer by corresponding property name or by property selector. 
        /// </summary>
        /// <example>
        /// Example for an entity Student that has public property FirstName.
        /// <code>
        /// // Get DataTables request from Http query parameters
        /// var request = new DataTablesRequest&lt;Student&gt;(url); 
        /// 
        /// // Access by property name
        /// var column = request.Columns["FirstName"];
        /// 
        /// // Access by property selector
        /// var column = request.Columns[s => s.FirstName];
        /// </code>
        /// </example>
        public IDataTablesColumnsCollection<TEntity, TEntityViewModel> Columns { get; private set; } = new DataTablesColumnsList<TEntity, TEntityViewModel>();

        /// <summary>
        /// Custom predicate to filter the queryable even when the <see cref="GlobalSearchValue"/> not specified.
        /// If custom filter predicate is specified, it is appended in the first place to the resulting queryable.
        /// </summary>
        public Expression<Func<TEntity, bool>> GlobalFilterPredicate { get; set; } = null;

        /// <summary>
        /// Set this property to log incoming request parameters and resulting queries to the given delegate. 
        /// For example, to log to the console, set this property to <see cref="Console.Write(string)"/>.
        /// </summary>
        public Action<string> Log { get; set; } = null;

        /// <summary>
        /// Original request parameters collection.
        /// </summary>
        public NameValueCollection OriginalRequest { get; protected set; } = null;

        /// <summary>
        /// Original set of column definitions for the underlying DataTables.
        /// </summary>
        public IDataTablesColumnsCollection<TEntity, TEntityViewModel> OriginalColumns { get; protected set; } = null;

        /// <summary>
        /// Gets original request parameter value by its name
        /// </summary>
        /// <param name="parameterName">Name of original request parameter</param>
        /// <returns>String value of original request parameter</returns>
        public string this[string parameterName]
        {
            get => OriginalRequest[parameterName];
        }

        /// <summary>
        /// Creates new <see cref="DataTablesRequest{T}"/> from <see cref="IDictionary{String, Object}"/>.
        /// This constructor is useful with it's needed to create <see cref="DataTablesRequest{T}"/> from the Nancy's <a href="https://github.com/NancyFx/Nancy/blob/master/src/Nancy/Request.cs">Request.Form</a> data.
        /// </summary>
        /// <param name="form">Request form data</param>
        public DataTablesRequest(IDictionary<string, object> form, IDataTablesColumnsCollection<TEntity, TEntityViewModel> columns)
            : this(form.Aggregate(new NameValueCollection(), (k, v) => { k.Add(v.Key, v.Value.ToString()); return k; }), columns) { }

        /// <summary>
        /// Creates new <see cref="DataTablesRequest{T}"/> from <see cref="DataTablesAjaxPostModel"/>.
        /// </summary>
        /// <param name="ajaxPostModel">Contains datatables parameters sent from client side when POST method is used.</param>
        public DataTablesRequest(DataTablesAjaxPostModel ajaxPostModel, IDataTablesColumnsCollection<TEntity, TEntityViewModel> columns)
            : this(ajaxPostModel.ToNameValueCollection(), columns) { }

        /// <summary>
        /// Creates new <see cref="DataTablesRequest{T}"/> from <see cref="Uri"/> instance.
        /// </summary>
        /// <param name="uri"><see cref="Uri"/> instance</param>
        public DataTablesRequest(Uri uri, IDataTablesColumnsCollection<TEntity, TEntityViewModel> columns)
            : this(uri.Query, columns) { }

        /// <summary>
        /// Creates new <see cref="DataTablesRequest{T}"/> from http query string.
        /// </summary>
        /// <param name="queryString"></param>
        public DataTablesRequest(string queryString, IDataTablesColumnsCollection<TEntity, TEntityViewModel> columns)
            : this(HttpUtility.ParseQueryString(queryString), columns) { }

        /// <summary>
        /// Creates new <see cref="DataTablesRequest{T}"/> from <see cref="NameValueCollection"/> instance.
        /// </summary>
        /// <param name="query"></param>
        public DataTablesRequest(NameValueCollection query, IDataTablesColumnsCollection<TEntity, TEntityViewModel> columns)
        {
            if (query == null)
            {
                throw new ArgumentNullException("Datatables query parameters collection is null.");
            }

            if (!query.HasKeys())
            {
                throw new ArgumentException("Datatables query has no keys.");
            }

            OriginalRequest = new NameValueCollection(query);
            OriginalColumns = columns;

            ParseGlobalConfigurationFromQuery(query);
            ParseColumnConfigurationFromQuery(query);
            ParseColumnOrderingConfigurationFromQuery(query);
        }

        /// <summary>
        /// Parses the global configuration from query.
        /// </summary>
        /// <param name="query">The query.</param>
        protected void ParseGlobalConfigurationFromQuery(NameValueCollection query)
        {
            int start = int.TryParse(query["start"], out start) ? start : 0;
            PageSize = int.TryParse(query["length"], out int length) ? length : 15;
            PageNumber = start / PageSize + 1;

            Draw = int.TryParse(query["draw"], out int draw) ? draw : 0;

            GlobalSearchValue = query["search[value]"];
            GlobalSearchRegex = bool.TryParse(query["search[regex]"], out bool searchRegex) ? searchRegex : false;
        }

        /// <summary>
        /// Parses the column configuration from query.
        /// </summary>
        /// <param name="query">The query.</param>
        protected void ParseColumnConfigurationFromQuery(NameValueCollection query)
        {
            var columnKeys = query.AllKeys.Where(k => k != null && Regex.IsMatch(k, ColumnPattern));
            foreach (var key in columnKeys)
            {
                // Retrieve the numeric column index
                var colIndex = Regex.Match(key, ColumnPattern).Groups[1].Value;

                // Retrieve the unique column key and name
                string data = query[$"columns[{colIndex}][data]"];
                string name = query[$"columns[{colIndex}][name]"];

                // Attempt to find a column with the given data
                var column = OriginalColumns.FirstOrDefault(c => c.PublicName == data);
                if (column != null)
                {
                    column.Index = int.Parse(colIndex);
                    column.SearchValue = query[$"columns[{colIndex}][search][value]"];
                    Columns.Add(column);
                }
            }
        }

        /// <summary>
        /// Parses the column ordering configuration from query.
        /// </summary>
        /// <param name="query">The query.</param>
        protected void ParseColumnOrderingConfigurationFromQuery(NameValueCollection query)
        {
            var orderKeys = query.AllKeys.Where(k => k != null && Regex.IsMatch(k, ColumnOrderingPattern));
            foreach (var key in orderKeys)
            {
                var index = Regex.Match(key, ColumnOrderingPattern).Groups[1].Value;

                if (int.TryParse(index, out int sortingIndex) &&
                    int.TryParse(query[$"order[{index}][column]"], out int columnIndex))
                {
                    var column = Columns.FirstOrDefault(c => c.Index == columnIndex);
                    if (column != null)
                    {
                        column.OrderingIndex = sortingIndex;
                        column.OrderingDirection = query[$"order[{index}][dir]"] == "desc"
                            ? ListSortDirection.Descending 
                            : ListSortDirection.Ascending;
                    }
                }
            }
        }
    }
}
