using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using DataTables.NetCore.Extensions;

namespace DataTables.NetCore
{
    public class DataTablesRequest<TEntity>
    {
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
        public IDataTablesColumnsCollection<TEntity> Columns { get; private set; } = new DataTablesColumnsList<TEntity>();

        /// <summary>
        /// Custom predicate to filter the queryable even when the <see cref="GlobalSearchValue"/> not specified.
        /// If custom filter predicate is specified, it is appended in the first place to the resulting queryable.
        /// </summary>
        public Expression<Func<TEntity, bool>> CustomFilterPredicate { get; set; } = null;

        /// <summary>
        /// Set this property to log incoming request parameters and resulting queries to the given delegate. 
        /// For example, to log to the console, set this property to <see cref="System.Console.Write(string)"/>.
        /// </summary>
        public Action<string> Log { get; set; } = null;

        /// <summary>
        /// Original request parameters collection
        /// </summary>
        public NameValueCollection OriginalRequest { get; private set; } = null;

        /// <summary>
        /// Gets original request parameter value by its name
        /// </summary>
        /// <param name="parameterName">Name of original request parameter</param>
        /// <returns>String value of original request parameter</returns>
        public string this[string parameterName]
        {
            get
            {
                return OriginalRequest[parameterName];
            }
        }

        /// <summary>
        /// Creates new <see cref="DataTablesRequest{T}"/> from <see cref="IDictionary{String, Object}"/>.
        /// This constructor is useful with it's needed to create <see cref="DataTablesRequest{T}"/> from the Nancy's <a href="https://github.com/NancyFx/Nancy/blob/master/src/Nancy/Request.cs">Request.Form</a> data.
        /// </summary>
        /// <param name="form">Request form data</param>
        public DataTablesRequest(IDictionary<string, object> form)
            : this(form.Aggregate(new NameValueCollection(), (k, v) => { k.Add(v.Key, v.Value.ToString()); return k; })) { }

        /// <summary>
        /// Creates new <see cref="DataTablesRequest{T}"/> from <see cref="Uri"/> instance.
        /// </summary>
        /// <param name="uri"><see cref="Uri"/> instance</param>
        public DataTablesRequest(Uri uri)
            : this(uri.Query) { }

        /// <summary>
        /// Creates new <see cref="DataTablesRequest{T}"/> from <see cref="DataTablesAjaxPostModel"/>.
        /// </summary>
        /// <param name="ajaxPostModel">Contains datatables parameters sent from client side when POST method is used.</param>
        public DataTablesRequest(DataTablesAjaxPostModel ajaxPostModel)
            : this(ajaxPostModel.ToNameValueCollection()) { }

        /// <summary>
        /// Creates new <see cref="DataTablesRequest{T}"/> from http query string.
        /// </summary>
        /// <param name="queryString"></param>
        public DataTablesRequest(string queryString)
            : this(HttpUtility.ParseQueryString(queryString)) { }

        /// <summary>
        /// Creates new <see cref="DataTablesRequest{T}"/> from <see cref="NameValueCollection"/> instance.
        /// </summary>
        /// <param name="query"></param>
        public DataTablesRequest(NameValueCollection query)
        {
            if (query == null)
                throw new ArgumentNullException("Datatables query parameters collection is null.");

            if (!query.HasKeys())
                throw new ArgumentException("Datatables query has no keys.");

            OriginalRequest = new NameValueCollection(query);

            int start = int.TryParse(query["start"], out start) ? start : 0;
            int length = int.TryParse(query["length"], out length) ? length : 15;
            int draw = int.TryParse(query["draw"], out draw) ? draw : 0;

            string globalSearch = query["search[value]"];
            bool searchRegex = bool.TryParse(query["search[regex]"], out searchRegex) ? searchRegex : false;

            int pageNumber = start / length + 1;

            GlobalSearchValue = globalSearch;
            GlobalSearchRegex = searchRegex;
            PageNumber = pageNumber;
            PageSize = length;
            Draw = draw;

            // extract columns info
            string columnPattern = "columns\\[(\\d+)\\]\\[data\\]";
            var columnKeys = query.AllKeys.Where(k => k != null && Regex.IsMatch(k, columnPattern));
            foreach (var key in columnKeys)
            {
                var colIndex = Regex.Match(key, columnPattern).Groups[1].Value;
                bool orderable = bool.TryParse(query[$"columns[{colIndex}][orderable]"], out orderable) ? orderable : true;
                bool searchable = bool.TryParse(query[$"columns[{colIndex}][searchable]"], out searchable) ? searchable : true;
                bool colSearchRegex = bool.TryParse(query["search[regex]"], out colSearchRegex) ? colSearchRegex : false;
                bool colCISearch = bool.TryParse(query[$"columns[{colIndex}][cisearch]"], out colCISearch) ? colCISearch : false;
                bool colCIOrder = bool.TryParse(query[$"columns[{colIndex}][ciorder]"], out colCIOrder) ? colCIOrder : false;

                string data = query[$"columns[{colIndex}][data]"];
                string name = query[$"columns[{colIndex}][name]"];
                string searchValue = query[$"columns[{colIndex}][search][value]"];
                string propertyName = null;
                PropertyInfo propertyInfo = null;
                var type = typeof(TEntity);

                // take property name from `data`
                if (colIndex.ToString() != data)
                {
                    propertyInfo = type.GetPropertyByName(data);
                    if (propertyInfo != null)
                    {
                        propertyName = data;
                    }
                    else
                    {
                        throw new ArgumentException($"Could not find a property called \"{data}\" on type \"{type}\". Make sure you have specified correct value of \"columnDefs.data\" parameter in datatables options.");
                    }
                }

                // take property name from `name`
                if (propertyInfo == null && !string.IsNullOrWhiteSpace(name))
                {
                    propertyInfo = type.GetPropertyByName(name);
                    if (propertyInfo != null)
                    {
                        propertyName = name;
                    }
                    else
                    {
                        throw new ArgumentException($"Could not find a property called \"{name}\" on type \"{type}\". Make sure you have specified correct value of \"columnDefs.name\" parameter in datatables options.");
                    }
                }

                if (propertyName == null)
                {
                    throw new ArgumentException($"Unable to associate datatables column \"{colIndex}\" with model type \"{typeof(TEntity)}\". There are no matching public property found. Make sure you specified valid identifiers for \"columnDefs.data\" and/or \"columnDefs.name\" parameters in datatables options for the column \"{colIndex}\".");
                }

                var column = new DataTablesColumn<TEntity>()
                {
                    Index = int.Parse(colIndex),
                    PropertyName = propertyName,
                    SearchValue = searchValue,
                    SearchRegex = colSearchRegex,
                    IsSearchable = searchable,
                    IsOrderable = orderable,
                    SearchCaseInsensitive = colCISearch,
                    OrderingCaseInsensitive = colCIOrder
                };

                Columns.Add(column);
            }

            // extract sorting info
            string orderPattern = "order\\[(\\d)\\]\\[column\\]";
            var orderKeys = query.AllKeys.Where(k => k != null && Regex.IsMatch(k, orderPattern));
            foreach (var key in orderKeys)
            {
                var index = Regex.Match(key, orderPattern).Groups[1].Value;

                if (int.TryParse(index, out int sortingIndex) &&
                    int.TryParse(query[$"order[{index}][column]"], out int columnIndex))
                {
                    var column = Columns.FirstOrDefault(c => c.Index == columnIndex);
                    if (column != null)
                    {
                        column.OrderingIndex = sortingIndex;
                        column.OrderingDirection = query[$"order[{index}][dir]"] == "desc" ?
                            ListSortDirection.Descending : ListSortDirection.Ascending;
                    }
                }
            }
        }
    }
}
