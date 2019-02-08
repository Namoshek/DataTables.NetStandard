using System.Linq;
using DataTables.NetCore.Sample.DataTables.ViewModels;
using DataTables.NetCore.Sample.Models;

namespace DataTables.NetCore.Sample.DataTables
{
    public class UserDataTable : DataTable<User, UserViewModel>, IDataTable<User, UserViewModel>
    {
        protected SampleDbContext _dbContext;

        public UserDataTable(SampleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override IQueryable<User> Query()
        {
            return _dbContext.Users;
        }
    }
}
