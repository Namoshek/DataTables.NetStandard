using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DataTables.NetCore.Extensions;

namespace DataTables.NetCore
{
    /// <summary>
    /// Defines interface for collection of <see cref="DataTablesColumn{TEntity, TEntityViewModel}"/> items with additional indexer methods
    /// to access an item by column name or model property name.
    /// </summary>
    /// <typeparam name="TEntity">Model type</typeparam>
    /// <typeparam name="TEntityViewModel">View model type</typeparam>
    public interface IDataTablesColumnsCollection<TEntity, TEntityViewModel> : ICollection<DataTablesColumn<TEntity, TEntityViewModel>>
    {
        /// <summary>
        /// Gets column by specified column name
        /// </summary>
        /// <param name="columnName">Column name that matches with the model property name</param>
        /// <returns><see cref="DataTablesColumn{TEntity, TEntityViewModel}"/> instance correspoding to specified model property</returns>
        DataTablesColumn<TEntity, TEntityViewModel> this[string columnName] { get; }

        /// <summary>
        /// Gets column by specified model property
        /// </summary>
        /// <param name="propertyExpression">Expression to locate the desired property</param>
        /// <returns><see cref="DataTablesColumn{TEntity, TEntityViewModel}"/> instance correspoding to specified model property</returns>
        DataTablesColumn<TEntity, TEntityViewModel> this[Expression<Func<TEntity, object>> propertyExpression] { get; }

        /// <summary>
        /// Gets column by its index
        /// </summary>
        /// <param name="columnIndex">Column index</param>
        /// <returns><see cref="DataTablesColumn{TEntity, TEntityViewModel}"/> instance correspoding to specified model property</returns>
        DataTablesColumn<TEntity, TEntityViewModel> this[int columnIndex] { get; }
    }

    /// <summary>
    /// Internal implementation of <see cref="IDataTablesColumnsCollection{TEntity, TEntityViewModel}"/>
    /// </summary>
    /// <typeparam name="TEntity">Model type</typeparam>
    public class DataTablesColumnsList<TEntity, TEntityViewModel> : List<DataTablesColumn<TEntity, TEntityViewModel>>, IDataTablesColumnsCollection<TEntity, TEntityViewModel>
    {
        public DataTablesColumnsList() : base() { }
        public DataTablesColumnsList(IEnumerable<DataTablesColumn<TEntity, TEntityViewModel>> items) : base(items) { }

        public DataTablesColumn<TEntity, TEntityViewModel> this[Expression<Func<TEntity, object>> propertyExpression]
        {
            get => this[propertyExpression.GetPropertyPath()];
        }

        public DataTablesColumn<TEntity, TEntityViewModel> this[string columnName]
        {
            get
            {
                var column = this.FirstOrDefault(c => c.PublicName == columnName);
                if (column == null)
                {
                    throw new ArgumentException($"Column \"{columnName}\" not found", nameof(columnName));
                }

                return column;
            }
        }

        public new DataTablesColumn<TEntity, TEntityViewModel> this[int columnIndex]
        {
            get
            {
                if (columnIndex < 0 || columnIndex > Count)
                {
                    throw new ArgumentException($"Column index \"{columnIndex}\" is out of range", nameof(columnIndex));
                }

                return base[columnIndex];
            }
        }
    }
}
