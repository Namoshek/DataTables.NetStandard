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
        /// Note: If a custom <see cref="GlobalFilterPredicate"/> is used, implementing the regex logic has to be done individually as well.
        /// 
        /// Note: If this option is set to false, then requests asking for regex evaluation will be handled as if they were non-regex requests.
        ///       The reason for this is security as we only allow regex evaluation if the server-side agrees to it.
        ///       
        /// Note: As Linq queries with regex logic cannot be translated to native SQL queries, all queries with regex logic
        ///       will be evaluated in-memory, which is highly inefficient with large data sets. Be careful using this option.
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
        public IList<DataTablesColumn<TEntity, TEntityViewModel>> Columns { get; private set; } = new List<DataTablesColumn<TEntity, TEntityViewModel>>();

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
        public IList<DataTablesColumn<TEntity, TEntityViewModel>> OriginalColumns { get; protected set; } = null;

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
        public DataTablesRequest(IDictionary<string, object> form, IList<DataTablesColumn<TEntity, TEntityViewModel>> columns)
            : this(form.Aggregate(new NameValueCollection(), (k, v) => { k.Add(v.Key, v.Value.ToString()); return k; }), columns) { }

        /// <summary>
        /// Creates new <see cref="DataTablesRequest{T}"/> from <see cref="DataTablesAjaxPostModel"/>.
        /// </summary>
        /// <param name="ajaxPostModel">Contains datatables parameters sent from client side when POST method is used.</param>
        public DataTablesRequest(DataTablesAjaxPostModel ajaxPostModel, IList<DataTablesColumn<TEntity, TEntityViewModel>> columns)
            : this(ajaxPostModel.ToNameValueCollection(), columns) { }

        /// <summary>
        /// Creates new <see cref="DataTablesRequest{T}"/> from <see cref="Uri"/> instance.
        /// </summary>
        /// <param name="uri"><see cref="Uri"/> instance</param>
        public DataTablesRequest(Uri uri, IList<DataTablesColumn<TEntity, TEntityViewModel>> columns)
            : this(uri.Query, columns) { }

        /// <summary>
        /// Creates new <see cref="DataTablesRequest{T}"/> from http query string.
        /// </summary>
        /// <param name="queryString"></param>
        public DataTablesRequest(string queryString, IList<DataTablesColumn<TEntity, TEntityViewModel>> columns)
            : this(HttpUtility.ParseQueryString(queryString), columns) { }

        /// <summary>
        /// Creates new <see cref="DataTablesRequest{T}"/> from <see cref="NameValueCollection"/> instance.
        /// </summary>
        /// <param name="query"></param>
        public DataTablesRequest(NameValueCollection query, IList<DataTablesColumn<TEntity, TEntityViewModel>> columns)
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

                // Retrieve the unique column key (and name)
                string data = query[$"columns[{colIndex}][data]"] ?? query[$"columns[{colIndex}][name]"];

                // Attempt to find a column with the given data
                var column = OriginalColumns.FirstOrDefault(c => c.PublicName == data);
                if (column != null)
                {
                    // We clone all columns we find because we don't want to change our original columns
                    column = (DataTablesColumn<TEntity, TEntityViewModel>)column.Clone();

                    // Parsing the index like this is safe because of the regex we used before
                    column.Index = int.Parse(colIndex);

                    column.SearchValue = query[$"columns[{colIndex}][search][value]"];

                    if (bool.TryParse(query[$"columns[{colIndex}][search][regex]"], out bool regexSearch))
                    {
                        // We only allow regex search if it is enabled for the column on the server-side
                        column.SearchRegex = (column.SearchRegex && regexSearch);
                    }

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
                // Retrieve the numeric column index
                var index = Regex.Match(key, ColumnOrderingPattern).Groups[1].Value;

                // Parse the actual column index used to sort
                if (int.TryParse(index, out int sortingIndex) &&
                    int.TryParse(query[$"order[{index}][column]"], out int columnIndex))
                {
                    // Attempt to find a column with the given index
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
