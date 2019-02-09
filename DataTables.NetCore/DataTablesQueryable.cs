﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataTables.NetCore
{
    /// <summary>
    /// Extended version of standard <see cref="IQueryable{TEntity}"/> interface with
    /// additional property to access <see cref="DataTablesRequest{TEntity, TEntityViewModel}"/>.
    /// </summary>
    /// <typeparam name="TEntity">Data type.</typeparam>
    public interface IDataTablesQueryable<TEntity, TEntityViewModel> : IQueryable<TEntity>
    {
        /// <summary>
        /// <see cref="DataTablesRequest{TEntity}"/> instance to filter the original <see cref="IQueryable{TEntity}"/>.
        /// </summary>
        DataTablesRequest<TEntity, TEntityViewModel> Request { get; }
    }

    /// <summary>
    /// Internal implementation of <see cref="IDataTablesQueryable{TEntity, TEntityViewModel}"/> interface.
    /// In fact, this is a wrapper around an <see cref="IQueryable{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">Model type</typeparam>
    internal class DataTablesQueryable<TEntity, TEntityViewModel> : IDataTablesQueryable<TEntity, TEntityViewModel>
    {
        private IQueryable<TEntity> _sourceQueryable;
        private DataTablesQueryProvider<TEntity, TEntityViewModel> _sourceProvider;

        public DataTablesRequest<TEntity, TEntityViewModel> Request { get; }

        internal DataTablesQueryable(IQueryable<TEntity> query, DataTablesRequest<TEntity, TEntityViewModel> request)
        {
            _sourceQueryable = query;
            _sourceProvider = new DataTablesQueryProvider<TEntity, TEntityViewModel>(query.Provider, request);
            Request = request;
        }

        public Type ElementType
        {
            get => typeof(TEntity);
        }

        public Expression Expression
        {
            get => _sourceQueryable.Expression;
        }

        public IQueryProvider Provider
        {
            get => _sourceProvider;
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return _sourceQueryable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _sourceQueryable.GetEnumerator();
        }

        public override string ToString()
        {
            return _sourceQueryable.ToString();
        }
    }
}
