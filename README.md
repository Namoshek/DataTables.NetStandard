# DataTables.NetCore

This package provides a way to create self-contained DataTable classes which
manage rendering, querying, filtering, sorting and other desireable tasks for you,
written in .NET Standard for ASP.NET Core applications.
The package is heavily inspired by Laravels (PHP) counterpart [`yajra/datatables`](https://github.com/yajra/laravel-datatables)
and extensions of said package.

## Installation

Currently, there is no NuGet download available just yet.
Package usage is only possible through cloning and manual compilation or import.

## Usage

To create a DataTable, you'll need to create a new class implementing the `IDataTable` interface.
There is an abstract base class called `DataTable` available for you to inherit from,
providing default implementations for most methods:

```csharp
public class PersonDataTable 
    : DataTable<Person, PersonViewModel>, IDataTable<Person, PersonViewModel> { }
```

A DataTable always requires two models to work. One is used internally to access 
the underlying data while the other is used to render the results for the response.
The data is mapped by the great [`AutoMapper`](https://github.com/AutoMapper/AutoMapper)
package under the hood. Defining and registering a mapper for the model to the view model
lies within the responsibility of the user and is not handled by the package.

#### Usage without separate View Model

For very basic usage of the package (with reduced security and functionality),
it is also possible to use the same model for data querying and data rendering.
Simply use the same model twice when creating a DataTable class:

```csharp
public class PersonDataTable : DataTable<Person, Person>, IDataTable<Person, Person> { }
```

### Full Example

The following is a working example of a DataTable that queries `Persons` with
some related `Location` information. The example showcases a lot of the supported
options combined in one table.

```csharp
public class PersonDataTable
    : DataTable<Person, PersonViewModel>, IDataTable<Person, PersonViewModel>
{
    protected SampleDbContext _dbContext;

    public PersonDataTable(SampleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override IDataTablesColumnsCollection<Person, PersonViewModel> Columns()
    {
        return new DataTablesColumnsList<Person, PersonViewModel>
        {
            new DataTablesColumn<Person, PersonViewModel>
            {
                PublicName = "id",
                DisplayName = "ID",
                PublicPropertyName = nameof(PersonViewModel.Id),
                PrivatePropertyName = nameof(Person.Id),
                IsOrderable = true,
                IsSearchable = true
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
            }
        };
    }

    public override IQueryable<Person> Query()
    {
        return _dbContext.Persons.Include(u => u.Location);
    }
}
```

## Options

### Defining `DataTablesColumn`s

Within your DataTable class, you'll need to override the abstract method
`public abstract IDataTablesColumnsCollection<TEntity, TEntityViewModel> Columns()`
with your concrete implementation. The method needs to return a collection of
`DataTablesColumn`s:

```csharp
public override IDataTablesColumnsCollection<Person, PersonViewModel> Columns()
{
    return new DataTablesColumnsList<Person, PersonViewModel>
    {
        new DataTablesColumn<Person, PersonViewModel>
        {
            PublicName = "id",
            DisplayName = "ID",
            PublicPropertyName = nameof(PersonViewModel.Id),
            PrivatePropertyName = nameof(Person.Id),
            IsOrderable = true,
            IsSearchable = true,
            GlobalSearchPredicate = (p, s) => p.Id.ToString().Contains(s),
            ColumnSearchPredicate = (p, s) => p.Id.ToString().Contains(s),
            ColumnOrderingProperty = (p) => p.Id
        },
        // More column definitions ...
    };
```

The columns have the following functions:

Column                      | Function
----------------------------|-----------------------------------------
`PublicName`                | The name that will be used in the response JSON object within the `data` segment.
`DisplayName`               | The column named used as title in the table header.
`PublicPropertyName`        | The name of the property used within the column on the view model.
`PrivatePropertyName`       | The name of the property used to query the data on the query model. Can be a composite property name in dot-notation (e.g. `Location.Street`).
`IsOrderable`               | If the table should be orderable by this column.
`IsSearchable`              | If the column should be searchable. Affects both column search as well as global search.
`GlobalSearchPredicate`     | An expression that is used to search the column when a global search value is set. The expression receives the query model and the global search value as parameters.
`ColumnSearchPredicate`     | An expression that is used to search the column when a column search value is set. The expression receives the query model and the column search value as parameters.
`ColumnOrderingProperty`    | An expression that selects a column of the query model to order the results by. Can be a nested property.

Properties selected with dot-notation require that the given nested object gets loaded by the query
which is returned from the `Query()` method.

## License

The code is licensed under the [MIT license](LICENSE.md).
