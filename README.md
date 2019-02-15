# DataTables.NetCore

This package provides a way to create self-contained DataTable classes which
manage rendering, querying, filtering, sorting and other desireable tasks for you,
written in .NET Standard for ASP.NET Core applications.
The package is heavily inspired by Laravels (PHP) counterpart [`yajra/datatables`](https://github.com/yajra/laravel-datatables)
and extensions of said package.

#### Table of Contents

- [Installation](#installation)
- [Usage](#usage)
- [Full Example](#full-example)
- [Configuration](#configuration)
  - [Global Configuration and per-table Overrides](#global-configuration-and-per-table-overrides)
  - [Configuring DataTable instances](#configuring-datatable-instances)
  - [Defining and Configuring `DataTablesColumn`s](#defining-and-configuring-datatablescolumns)
  - [Global Filtering](#global-filtering)
- [Advanced Usage](#advanced-usage)
  - [Using a custom Mapping Function](#using-a-custom-mapping-function)
  - [Usage without separate View Model](#usage-without-separate-view-model)
  - [Action Column Rendering and Data Transformation](#action-column-rendering-and-data-transformation)
  - [Customizing the Table Rendering](#customizing-the-table-rendering)
  - [Extending DataTables with Plugins](#extending-datatables-with-plugins)
- [License](#license)

## Installation

Currently, there is no NuGet download available just yet.
Package usage is only possible through cloning and manual compilation or import.

## Usage

To create a DataTable, you'll need to create a new class implementing the 
[`IDataTable` ](DataTables.NetCore/Abstract/IDataTable.cs) interface.
There is an abstract base class called [`DataTable`](DataTables.NetCore/DataTable.cs)
available for you to inherit from, providing default implementations for most methods.
You only have to provide own implementations for a few methods:

```csharp
public class PersonDataTable 
    : DataTable<Person, PersonViewModel>, IDataTable<Person, PersonViewModel>
{
    public override IList<DataTablesColumn<Person, PersonViewModel>> Columns()
    {
        return new List<DataTablesColumn<Person, PersonViewModel>>
        {
            // Your DataTable column definitions come here
        };
    }

    public override IQueryable<Person> Query()
    {
        return _dbContext.Persons;
    }

    public override Expression<Func<Person, PersonViewModel>> MappingFunction()
    {
        return p => AutoMapper.Mapper.Map<PersonViewModel>(p);
    }
}
```

As you can see, a DataTable always requires two models to work. One is used internally to access 
the underlying data while the other is used to render the results for the response.
The dats is mapped using a configurable mapping function. We recommend using the great
[`AutoMapper`](https://github.com/AutoMapper/AutoMapper) package by initializing the mapper
in the `Startup.cs` and configuring the mapping function in the custom DataTable class
as seen above.

```csharp
// Startup.cs
Mapper.Initialize(m =>
{
    m.AddProfile(new DefaultMappingProfile(services.BuildServiceProvider().GetService<IViewRenderService>()));
});
```

Of course you can also create a base class for all your DataTables with a generic implementation
of the mapping provider function if you don't want to define the same function over and over again.

For a quick start, we recommend having a look at the [UserDataTable](DataTables.NetCore.Sample/DataTables/UserDataTable.cs)
example in the [Sample](DataTables.NetCore.Sample/) project. It is a basic example showcasing
what is possible with this package and how easy it is to setup a new DataTable.

After defining a custom DataTable, you only have to register it in your service container,
inject it to your controller and pass it to the view via the `ViewBag`. In the view,
you can then render the HTML and the JavaScript for your table. Rendering the global defaults
for your DataTables is optional:

```csharp
// MyTable.cshtml
<div class="table-responsive">
  @Html.Raw(MyCustomDataTable.RenderHtml())
</div>

@section Scripts {
    $(document).ready(function () {
        @Html.Raw(MyCustomDataTable.RenderScript(Url.Action("TableData", "MyController")))
    });
}

// _Layout.cshtml
<script type="text/javascript">
    @Html.Raw(DataTables.NetCore.Configuration.DataTablesConfigurationBuilder.BuildGlobalConfigurationScript())
</script>
@RenderSection("Scripts", required: false)
```

## Full Example

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

    public override IList<DataTablesColumn<Person, PersonViewModel>> Columns()
    {
        return new List<DataTablesColumn<Person, PersonViewModel>>
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

### Global Configuration and per-table Overrides

TODO

### Configuring DataTable instances

TODO

### Defining and Configuring `DataTablesColumn`s

Within your DataTable class, you'll need to override the abstract method
`public abstract IList<DataTablesColumn<TEntity, TEntityViewModel>> Columns()`
with your concrete implementation. The method needs to return a collection of
`DataTablesColumn`s:

```csharp
public override IList<DataTablesColumn<Person, PersonViewModel>> Columns()
{
    return new List<DataTablesColumn<Person, PersonViewModel>>
    {
        new DataTablesColumn<Person, PersonViewModel>
        {
            PublicName = "id",
            DisplayName = "ID",
            PublicPropertyName = nameof(PersonViewModel.Id),
            PrivatePropertyName = nameof(Person.Id),
            IsOrderable = true,
            IsSearchable = true,
            SearchRegex = true,
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
`SearchRegex`               | If column search values should be evaluated as regex expressions. The server-side option can still be disabled by the client, but the client cannot enable regex evaluation if the server has it disabled for a column. **Note: regex search is performed in-memory as Linq queries containing `Regex.IsMatch(value, pattern)` cannot be translated to native SQL queries. Avoid usage for larger data sets if possible.**
`GlobalSearchPredicate`     | An expression that is used to search the column when a global search value is set. The expression receives the query model and the global search value as parameters.
`ColumnSearchPredicate`     | An expression that is used to search the column when a column search value is set. The expression receives the query model and the column search value as parameters.
`ColumnOrderingProperty`    | An expression that selects a column of the query model to order the results by. Can be a nested property.

Properties selected with dot-notation require that the given nested object gets loaded by the query
which is returned from the `Query()` method.

### Global Filtering

TODO

## Advanced Usage

### Using a custom Mapping Function

TODO

### Usage without separate View Model

For very basic usage of the package (with reduced security and functionality),
it is also possible to use the same model for data querying and data rendering.
Simply use the same model twice when creating a DataTable class:

```csharp
public class PersonDataTable : DataTable<Person, Person>, IDataTable<Person, Person> { }
```

### Action Column Rendering and Data Transformation

TODO

### Customizing the Table Rendering

TODO

### Extending DataTables with Plugins

## License

The code is licensed under the [MIT license](LICENSE.md).
