using System.Collections.Generic;
using System.Linq;
using DataTables.NetCore.Abstract;
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

        public override IList<DataTablesColumn<User, UserViewModel>> Columns()
        {
            return new List<DataTablesColumn<User, UserViewModel>>
            {
                new DataTablesColumn<User, UserViewModel>
                {
                    PublicName = "id",
                    DisplayName = "ID",
                    PublicPropertyName = nameof(UserViewModel.Id),
                    PrivatePropertyName = nameof(UserViewModel.Id),
                    IsOrderable = true,
                    IsSearchable = true
                },
                new DataTablesColumn<User, UserViewModel>
                {
                    PublicName = "name",
                    DisplayName = "Name",
                    PublicPropertyName = nameof(UserViewModel.Name),
                    PrivatePropertyName = nameof(User.Name),
                    IsOrderable = true,
                    IsSearchable = true
                },
                new DataTablesColumn<User, UserViewModel>
                {
                    PublicName = "email",
                    DisplayName = "Email",
                    PublicPropertyName = nameof(UserViewModel.Email),
                    PrivatePropertyName = nameof(User.Email),
                    IsOrderable = true,
                    IsSearchable = true
                }
            };
        }

        public override IQueryable<User> Query()
        {
            return _dbContext.Users;
        }
    }
}
