using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DataTables.NetStandard.Abstract;
using DataTables.NetStandard.Sample.DataTables.ViewModels;
using DataTables.NetStandard.Sample.Models;
using Microsoft.EntityFrameworkCore;

namespace DataTables.NetStandard.Sample.DataTables
{
    public class PersonDataTable : DataTable<Person, PersonViewModel>, IDataTable<Person, PersonViewModel>
    {
        protected SampleDbContext _dbContext;

        public PersonDataTable(SampleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override IList<DataTablesColumn<Person, PersonViewModel>> Columns()
        {
            var columns = new List<DataTablesColumn<Person, PersonViewModel>>
            {
                new DataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "id",
                    DisplayName = "ID",
                    PublicPropertyName = nameof(PersonViewModel.Id),
                    PrivatePropertyName = nameof(Person.Id),
                    IsOrderable = true,
                    IsSearchable = true,
                    SearchRegex = true
                },
                new DataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "name",
                    DisplayName = "Name",
                    PublicPropertyName = nameof(PersonViewModel.Name),
                    PrivatePropertyName = nameof(Person.Name),
                    IsOrderable = true,
                    IsSearchable = true
                },
                new DataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "email",
                    DisplayName = "Email",
                    PublicPropertyName = nameof(PersonViewModel.Email),
                    PrivatePropertyName = nameof(Person.Email),
                    IsOrderable = true,
                    IsSearchable = true
                },
                new DataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "dateOfBirth",
                    DisplayName = "Date of Birth",
                    PublicPropertyName = nameof(PersonViewModel.DateOfBirth),
                    PrivatePropertyName = nameof(Person.DateOfBirth),
                    IsOrderable = true,
                    IsSearchable = false
                },
                new DataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "address",
                    DisplayName = "Address",
                    PublicPropertyName = nameof(PersonViewModel.Address),
                    PrivatePropertyName = $"{nameof(Person.Location)}.{nameof(Location.Street)}",
                    IsOrderable = true,
                    IsSearchable = true,
                    ColumnSearchPredicate = (p, s) => (p.Location.Street + " " + p.Location.HouseNumber).Contains(s),
                    GlobalSearchPredicate = (p, s) => (p.Location.Street + " " + p.Location.HouseNumber).Contains(s)
                },
                new DataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "postCode",
                    DisplayName = "Post Code",
                    PublicPropertyName = nameof(PersonViewModel.PostCode),
                    PrivatePropertyName = $"{nameof(Person.Location)}.{nameof(Location.PostCode)}",
                    IsOrderable = true,
                    IsSearchable = true
                },
                new DataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "city",
                    DisplayName = "City",
                    PublicPropertyName = nameof(PersonViewModel.City),
                    PrivatePropertyName = $"{nameof(Person.Location)}.{nameof(Location.City)}",
                    IsOrderable = true,
                    IsSearchable = true
                },
                new DataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "country",
                    DisplayName = "Country",
                    PublicPropertyName = nameof(PersonViewModel.Country),
                    PrivatePropertyName = $"{nameof(Person.Location)}.{nameof(Location.Country)}",
                    IsOrderable = true,
                    IsSearchable = true
                },
                new DataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "action",
                    DisplayName = "Action",
                    PublicPropertyName = nameof(PersonViewModel.Action),
                    PrivatePropertyName = null,
                    IsOrderable = false,
                    IsSearchable = false
                },
                new DataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "action2",
                    DisplayName = "Action 2",
                    PublicPropertyName = nameof(PersonViewModel.Action2),
                    PrivatePropertyName = null,
                    IsOrderable = false,
                    IsSearchable = false
                },
                new DataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "action3",
                    DisplayName = "Action 3",
                    PublicPropertyName = nameof(PersonViewModel.Action3),
                    PrivatePropertyName = null,
                    IsOrderable = false,
                    IsSearchable = false
                }
            };

            // We can also add additional options to a column
            columns.Last().AdditionalOptions.Add("className", "text-center");

            return columns;
        }

        public override IQueryable<Person> Query()
        {
            return _dbContext.Persons.Include(p => p.Location);
        }

        public override Expression<Func<Person, PersonViewModel>> MappingFunction()
        {
            return p => AutoMapper.Mapper.Map<PersonViewModel>(p);
        }

        public override IDictionary<string, dynamic> AdditionalDataTableOptions()
        {
            return new Dictionary<string, dynamic>
            {
                { "stateSave", true },
                { "pagingType", "full_numbers" },
                { "search", new { smart = true } }
            };
        }
    }
}
