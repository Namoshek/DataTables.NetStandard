using System.Linq;
using System.Linq.Expressions;

namespace DataTables.NetStandard
{
    internal class DataTablesQueryProvider<TEntity, TEntityViewModel> : IQueryProvider
    {
        private readonly IQueryProvider _sourceProvider;
        private readonly DataTablesRequest<TEntity, TEntityViewModel> _request;

        internal DataTablesQueryProvider(IQueryProvider sourceProvider, DataTablesRequest<TEntity, TEntityViewModel> request)
        {
            _sourceProvider = sourceProvider;
            _request = request;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new DataTablesQueryable<TEntity, TEntityViewModel>((IQueryable<TEntity>)_sourceProvider.CreateQuery(expression), _request);
        }

        public IQueryable<TResult> CreateQuery<TResult>(Expression expression)
        {
            return (IQueryable<TResult>)CreateQuery(expression);
        }

        public object Execute(Expression expression)
        {
            return _sourceProvider.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)_sourceProvider.Execute(expression);
        }
    }
}
