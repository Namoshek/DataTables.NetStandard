﻿using System.Linq;
using System.Linq.Expressions;

namespace DataTables.NetStandard
{
    internal class DataTablesQueryProvider<TEntity, TEntityViewModel> : IQueryProvider
    {
        private IQueryProvider sourceProvider;
        private DataTablesRequest<TEntity, TEntityViewModel> request;

        internal DataTablesQueryProvider(IQueryProvider sourceProvider, DataTablesRequest<TEntity, TEntityViewModel> request)
        {
            this.sourceProvider = sourceProvider;
            this.request = request;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new DataTablesQueryable<TEntity, TEntityViewModel>((IQueryable<TEntity>)sourceProvider.CreateQuery(expression), request);
        }

        public IQueryable<TResult> CreateQuery<TResult>(Expression expression)
        {
            return (IQueryable<TResult>)CreateQuery(expression);
        }

        public object Execute(Expression expression)
        {
            return sourceProvider.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)sourceProvider.Execute(expression);
        }
    }
}
