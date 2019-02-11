using System.Linq;
using DataTables.NetCore.Abstract;
using DataTables.NetCore.Sample.DataTables.ViewModels;
using DataTables.NetCore.Sample.Models;
using Microsoft.EntityFrameworkCore;

namespace DataTables.NetCore.Sample.DataTables
{
    public class UserDataTable : DataTable<User, UserViewModel>, IDataTable<User, UserViewModel>
    {
        protected SampleDbContext _dbContext;

        public UserDataTable(SampleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override IDataTablesColumnsCollection<User, UserViewModel> Columns()
        {
            return new DataTablesColumnsList<User, UserViewModel>
            {
                new DataTablesColumn<User, UserViewModel>
                {
                    PublicName = "id",
                    DisplayName = "ID",
                    PublicPropertyName = nameof(UserViewModel.Id),
                    PrivatePropertyName = nameof(User.Id),
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
                },
                new DataTablesColumn<User, UserViewModel>
                {
                    PublicName = "dateOfBirth",
                    DisplayName = "Date of Birth",
                    PublicPropertyName = nameof(UserViewModel.DateOfBirth),
                    PrivatePropertyName = nameof(User.DateOfBirth),
                    IsOrderable = true,
                    IsSearchable = false
                },
                new DataTablesColumn<User, UserViewModel>
                {
                    PublicName = "address",
                    DisplayName = "Address",
                    PublicPropertyName = nameof(UserViewModel.Address),
                    PrivatePropertyName = $"{nameof(User.Location)}.{nameof(Location.Street)}",
                    IsOrderable = true,
                    IsSearchable = true
                },
                new DataTablesColumn<User, UserViewModel>
                {
                    PublicName = "postCode",
                    DisplayName = "Post Code",
                    PublicPropertyName = nameof(UserViewModel.PostCode),
                    PrivatePropertyName = $"{nameof(User.Location)}.{nameof(Location.PostCode)}",
                    IsOrderable = true,
                    IsSearchable = true
                },
                new DataTablesColumn<User, UserViewModel>
                {
                    PublicName = "city",
                    DisplayName = "City",
                    PublicPropertyName = nameof(UserViewModel.City),
                    PrivatePropertyName = $"{nameof(User.Location)}.{nameof(Location.City)}",
                    IsOrderable = true,
                    IsSearchable = true
                },
                new DataTablesColumn<User, UserViewModel>
                {
                    PublicName = "country",
                    DisplayName = "Country",
                    PublicPropertyName = nameof(UserViewModel.Country),
                    PrivatePropertyName = $"{nameof(User.Location)}.{nameof(Location.Country)}",
                    IsOrderable = true,
                    IsSearchable = true
                }
            };
        }

        public override IQueryable<User> Query()
        {
            return _dbContext.Users.Include(u => u.Location);
        }
    }
}
