using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DataTables.NetStandard.Sample.DataTables.ViewModels;
using DataTables.NetStandard.Sample.Models;
using Microsoft.EntityFrameworkCore;

namespace DataTables.NetStandard.Sample.DataTables
{
    public class PersonDataTable : BaseDataTable<Person, PersonViewModel>
    {
        protected SampleDbContext _dbContext;

        public PersonDataTable(IMapper mapper, SampleDbContext dbContext) : base(mapper)
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
                    SearchPredicate = (p, s) => false,                  // The fallback predicate will never match, but since we declared a provider, it is not used anyway.
                    SearchPredicateProvider = (s) => (p, s) => true,    // The provider will return a predicate matching all entities (used for global search). This is just for illustration, it makes no sense.
                    ColumnSearchPredicateProvider = (s) =>              // The column provider will return a predicate matching entities in a numeric range if the search term is properly formatted.
                    {
                        var minMax = s.Split("-delim-", System.StringSplitOptions.RemoveEmptyEntries);
                        if (minMax.Length >= 2)
                        {
                            var min = long.Parse(minMax[0]);
                            var max = long.Parse(minMax[1]);

                            return (p, s) => p.Id >= min && p.Id <= max;
                        }

                        return (p, s) => false;
                    }
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
                    PublicName = "otherEmails",
                    DisplayName = "Other Emails",
                    PublicPropertyName = nameof(PersonViewModel.OtherEmails),
                    PrivatePropertyName = null,
                    IsOrderable = false,
                    IsSearchable = true,
                    SearchPredicate = (p, s) => p.EmailAddresses.Any(e => e.Address.ToUpper().Contains(s.ToUpper()))
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
                    SearchPredicate = (p, s) => (p.Location.Street + " " + p.Location.HouseNumber).ToLower().Contains(s.ToLower()),
                    ColumnOrderingExpression = (p) => (p.Location.Street + " " + (p.Location.HouseNumber ?? "")).Trim().ToLower()
                },
                new DataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "postCode",
                    DisplayName = "Post Code",
                    PublicPropertyName = nameof(PersonViewModel.PostCode),
                    PrivatePropertyName = $"{nameof(Person.Location)}.{nameof(Location.PostCode)}",
                    IsOrderable = true,
                    IsSearchable = true,
                    OrderingIndex = 0,
                    OrderingDirection = System.ComponentModel.ListSortDirection.Ascending
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
                    IsSearchable = true,
                    OrderingIndex = 1,
                    OrderingDirection = System.ComponentModel.ListSortDirection.Descending
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
            return _dbContext.Persons
                .Include(p => p.EmailAddresses)
                .Include(p => p.Location);
        }

        protected override void ConfigureAdditionalOptions(DataTablesConfiguration configuration, IList<DataTablesColumn<Person, PersonViewModel>> columns)
        {
            base.ConfigureAdditionalOptions(configuration, columns);

            configuration.AdditionalOptions["stateSave"] = true;
            configuration.AdditionalOptions["pagingType"] = "full_numbers";
            configuration.AdditionalOptions["search"] = new { smart = true };
        }
    }
}
