# DataTables.NetStandard

This package provides a way to create self-contained DataTable classes 
for the famous [datatables.net](https://datatables.net) jQuery plugin which
manage rendering, querying, filtering, sorting and other desireable tasks for you,
written in .NET Standard for ASP.NET Core applications with focus on Entity Framework Core.
The package is heavily inspired by Laravels (PHP) counterpart [`yajra/datatables`](https://github.com/yajra/laravel-datatables)
and extensions of said package.

#### Why this package?

Do you also ask yourself why you have to define and configure your DataTables both in the frontend
as well as on the server-side, duplicating code and increasing maintenance efforts?
Awesome, then we have something in common! :smile:

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
  - [Usage without separate View Model](#usage-without-separate-view-model)
  - [Action Column Rendering and Data Transformation](#action-column-rendering-and-data-transformation)
  - [Customizing the Table Rendering](#customizing-the-table-rendering)
  - [Extending DataTables with Plugins](#extending-datatables-with-plugins)
- [Enhanced DataTables](#enhanced-datatables)
  - [Usage](#usage-1)
  - [Supported Filters](#supported-filters)
    - [TextInput Filter](#textinput-filter)
    - [Select Filter](#select-filter)
  - [Filter Configuration](#filter-configuration)
    - [Global Defaults](#global-defaults)
    - [Per-Usage Configuration](#per-usage-configuration)
- [License](#license)

## Installation

The package can be found on [nuget.org](https://www.nuget.org/packages/DataTables.NetStandard/).
You can install the package with:

```pwsh
$> Install-Package DataTables.NetStandard
```

## Usage

To create a DataTable, you'll need to create a new class implementing the 
[`IDataTable` ](DataTables.NetStandard/Abstract/IDataTable.cs) interface.
There is an abstract base class called [`DataTable`](DataTables.NetStandard/DataTable.cs)
available for you to inherit from, providing default implementations for most methods.
You only have to provide own implementations for a few methods:

```csharp
public class PersonDataTable : DataTable<Person, PersonViewModel>, IDataTable<Person, PersonViewModel>
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
    m.AddProfile<DefaultMappingProfile>();

    // Or, to pass some dependencies to the mapping profile (when using AutoMapper)
    m.AddProfile(new DefaultMappingProfile(services.BuildServiceProvider().GetService<IViewRenderService>()));
});

// PersonDataTable.cs
public override Expression<Func<Person, PersonViewModel>> MappingFunction()
{
    return p => AutoMapper.Mapper.Map<PersonViewModel>(p);
}
```

Of course you can also create a base class for all your DataTables with a generic implementation
of the mapping provider function if you don't want to define the same function over and over again.

For a quick start, we recommend having a look at the [PersonDataTable](DataTables.NetStandard.Sample/DataTables/PersonDataTable.cs)
example in the [Sample](DataTables.NetStandard.Sample/) project. It is a basic example showcasing
what is possible with this package and how easy it is to setup a new DataTable.

After defining a custom DataTable, you only have to register it in your service container,
inject it to your controller and pass it to the view via the `ViewBag`. In the view,
you can then render the HTML and the JavaScript for your table. Rendering the global defaults
for your DataTables is optional:

```csharp
// MyTable.cshtml
@{
    var DataTable = (MyCustomDataTable)ViewBag.MyCustomDataTable;
}

<div class="table-responsive">
  @Html.Raw(DataTable.RenderHtml())
</div>

@section Scripts {
    $(document).ready(function () {
        @Html.Raw(DataTable.RenderScript(Url.Action("TableData", "MyController")))
    });
}

// _Layout.cshtml (Optional)
<script type="text/javascript">
    @Html.Raw(DataTables.NetStandard.Configuration.DataTablesConfigurationBuilder.BuildGlobalConfigurationScript())
</script>
@RenderSection("Scripts", required: false)
```

Please note that this package does not include the actual DataTables script file as well as the stylesheet.
You will have to add these files to the layout yourself. This package only generates the table HTML and
the script that renders the actual table.

## Full Example

The following is a working example of a DataTable that queries `Persons` with
some related `Location` information. The example showcases a lot of the supported
options combined in one table.

```csharp
public class PersonDataTable : DataTable<Person, PersonViewModel>, IDataTable<Person, PersonViewModel>
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
                SearchPredicate = (p, s) => (p.Location.Street + " " + p.Location.HouseNumber)
                    .ToLower().Contains(s.ToLower())
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
        return _dbContext.Persons.Include(p => p.Location);
    }
}
```

## Configuration

### Global Configuration and per-table Overrides

Configuring your DataTables globally is possible through the `DataTablesConfigurationBuilder`
which has a singleton-like property called `DefaultConfiguration`. Currently, this configuration
object does not provide properties for common settings directly, but instead exposes a dictionary
which you can fill with any configuration options you need.

```csharp
// Startup.cs (or somewhere else, but before rendering the DataTable scripts)
DataTablesConfigurationBuilder.DefaultConfiguration.AdditionalOptions.Add("stateSave", false);
DataTablesConfigurationBuilder.DefaultConfiguration.AdditionalOptions.Add("search", new
{
    Smart = true,
    Regex = false,
    Search = "Initial search string"
});
```

_Note: Please be aware that the options passed to this AdditionalOptions dictionary will not be transformed
from PascalCase to camelCase or similar. They are serialized the way they are configured. Properties of
objects passed as value, like `Smart`, `Regex` or `Search` in above example, **are** translated to
camelCase though!_

By changing the default configuration, all your rendered DataTable scripts will receive these options
if they are not being overwritten by the concrete DataTable implementation through overriding the 
`public IDictionary<string, dynamic> AdditionalDataTableOptions()` method. The ajax URL as well as 
the method (`GET`/`POST`) are set when rendering the DataTable scripts with 
`personDataTable.RenderScript(Url.Action("TableData", "Person"), "post")`.

### Configuring DataTable instances

Adding additional options for a specific DataTable or overriding global defaults is possible by overriding
the implementation for the `AdditionalDataTableOptions()` method:

```csharp
public override IDictionary<string, dynamic> AdditionalDataTableOptions()
{
    return new Dictionary<string, dynamic>
    {
        { "stateSave", false },
        { "search", new { Smart = true, Regex = false, Search = null } }
    };
}
```

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
            SearchPredicate = (p, s) => p.Id.ToString().Contains(s),
            GlobalSearchPredicate = (p, s) => p.Id.ToString().Contains(s),
            ColumnSearchPredicate = (p, s) => p.Id.ToString().Contains(s),
            ColumnOrderingProperty = (p) => p.Id,
            AdditionalOptions = new Dictionary<string, dynamic>
            {
                { "visible", false },
                { "className", "hidden" }
            }
        },
        // More column definitions ...
    };
```

The columns have the following functions:

Column                      | Mandatory | Default                         | Function
----------------------------|-----------|---------------------------------|---------------------
`PublicName`                | Yes       |                                 | The name that will be used in the response JSON object within the `data` segment.
`DisplayName`               | No        | `PublicName.FirstCharToUpper()` | The column name used as title in the table header.
`PublicPropertyName`        | Yes       |                                 | The name of the property on the view model used within the column.
`PrivatePropertyName`       | Yes       |                                 | The name of the property on the query model used to query the data. Can be a composite property name in dot-notation (e.g. `Location.Street`).
`IsOrderable`               | No        | `false`                         | If the table should be orderable by this column.
`IsSearchable`              | No        | `false`                         | If the column should be searchable. Enables or disables both, column search as well as global search.
`SearchRegex`               | No        | `false`                         | If column search values should be evaluated as regex expressions. The server-side option can still be disabled on a per-request basis by the client, but the client cannot enable regex evaluation if the server has it disabled for a column. **Note: regex search is performed in-memory as Linq queries containing `Regex.IsMatch(value, pattern)` cannot be translated to native SQL queries. Avoid using this option for larger data sets if possible.**
`SearchPredicate`           | No        | `PrivatePropertyName` property `Contains(searchValue)` | An expression that is used to search the column both when a global search value as well as a column search value is set. `ColumnSearchPredicate` and `GlobalSearchPredicate` can both override this predicate for their specific use case. The expression receives the query model and the global or column search value, depending on the search, as parameters. _Note: You should make sure the expression can be translated by Linq to SQL, otherwise it may be evaluated in-memory._
`GlobalSearchPredicate`     | No        | `SearchPredicate` or its default | An expression that is used to search the column when a global search value is set. The expression receives the query model and the global search value as parameters. _Note: You should make sure the expression can be translated by Linq to SQL, otherwise it may be evaluated in-memory._
`ColumnSearchPredicate`     | No        | `SearchPredicate` or its default | An expression that is used to search the column when a column search value is set. The expression receives the query model and the column search value as parameters. _Note: You should make sure the expression can be translated by Linq to SQL, otherwise it may be evaluated in-memory._
`ColumnOrderingProperty`    | No        | `PrivatePropertyName`           | An expression that selects a column of the query model to order the results by. Can be a nested property.
`AdditionalOptions`         | No        | empty `Dictionary`              | A dictionary that can be used to pass additional columns options which are serialized as part of the generated DataTable script. The additional options are serialized as they are, without changing dictionary keys from _PascalCase_ to _camelCase_.

Properties selected with dot-notation require that the given nested objects get loaded by the query
which is returned from the `Query()` method using `Include(propertyExpression)` or similar.

### Global Filtering

A global filter can be applied to the DataTable by constraining the query returned by `IQueryable<TEntity> Query()`:

```csharp
public override IQueryable<Person> Query()
{
    return _dbContext.Persons
        .Include(p => p.Location)
        .Where(p => p.Id > 100 || p.Name.Contains("Admin"));
}
```

## Advanced Usage

### Usage without separate View Model

For very basic usage of the package (with reduced security and functionality),
it is also possible to use the same model for data querying and data rendering.
Simply use the same model twice when creating a DataTable class:

```csharp
public class PersonDataTable : DataTable<Person, Person>, IDataTable<Person, Person> { }
```

All members of type `Person` that are not referenced by the `PublicPropertyName` within at least
one `DataTablesColumn` will not be part of the serialized data that is sent to the client.
This is done in order to prevent sending data to the client which we did not intend to send.

For better security and in order to fully utilize the power of this package,
we recommend using a separate view model at any time though.

### Action Column Rendering and Data Transformation

All of the data transformations you require to perform on your query models in order to display
them to your users should be performed by the mapping function you defined on your DataTable class.
In case you are using `AutoMapper`, this means you can perform this transformation in your 
mapping `Profile` like this:

```csharp
public class DefaultMappingProfile : Profile
{
    public DefaultMappingProfile()
    {
        CreateMap<Person, PersonViewModel>()
            .ForMember(vm => vm.Address, m => m.MapFrom(p => $"{p.Location.Street} {p.Location.HouseNumber}"))
            .ForMember(vm => vm.PostCode, m => m.MapFrom(p => p.Location.PostCode))
            .ForMember(vm => vm.City, m => m.MapFrom(p => p.Location.City))
            .ForMember(vm => vm.Country, m => m.MapFrom(p => p.Location.Country));
    }
}
```

You can also return raw HTML in your view models to generate styled cells or action columns for example.
As this can be a bit cumbersome, we provide some helpers that can render templates with your row data.
In order to use the helpers, you'll have to add the additional package
[`DataTables.NetStandard.TemplateMapper`](https://www.nuget.org/packages/DataTables.NetStandard.TemplateMapper/)
to your project.
You find some examples below with an explanation of the different methods within code comments.

```csharp
public class DefaultMappingProfile : Profile
{
    public DefaultMappingProfile(IViewRenderService viewRenderService)
    {
        CreateMap<Person, PersonViewModel>()

            // Raw columns containing some HTML (like action buttons) consist of simple strings. This means
            // you can basically add a string column on the view model which does not have to exist on the
            // query model and return some custom HTML for it here in the mapper. In this example we are simply
            // building a link inline. The following two columns do the same but using file-based templates.
            .ForMember(vm => vm.Action, m => m.MapFrom(p => $"<a href=\"#person-{p.Id}\">Link 1</a>"))

            // This uses the package Scriban which parses Liquid templates and renders them with the row data.
            // The Scriban package does not require any dependency injection and offers static methods, which
            // makes it a very easy to use library. The template language Liquid is quite different from Razor
            // though, so it can be a bit of work to get used to it.
            // Probably important: If the row object (person) is passed directly as second argument, its properties
            // will be accessible in the template directly (i.e. <code>p.Id</code> -> <code>{{ id }}</code>).
            // If the row object is wrapped in another object like <code>new { Person = p }</code>, the properties
            // will be accessible with <code>{{ person.id }}</code> for example.
            // Important: Template files have to be copied to the output folder during builds. Make sure this
            //            setting is set correctly in the file properties.
            .ForMember(vm => vm.Action, m => m.MapFrom(p => ViewRenderService.RenderLiquidTemplateFileWithData("DataTables/Person/Action.twig", p)))

            // The same renderer is also available for string based templates instead of file based ones.
            .ForMember(vm => vm.Action, m => m.MapFrom(p => ViewRenderService.RenderLiquidTemplateWithData("<a href=\"#person-{{id}}\">Link 2</a>", p)))

            // This renders the given view as Razor template through the ASP.NET Core MVC Razor engine. Rendering
            // the view this way allows you to use basically all Razor functions available. There is a significant
            // downside to this though: The AutoMapper profile (this class) has to receive the IViewRenderService
            // from the dependency injector somehow, which does not happen by itself and is only possible through
            // a hack in the Startup.ConfigureService() method. Have a look there to learn more about it.
            .ForMember(vm => vm.Action, m => m.MapFrom(p => viewRenderService.RenderRazorToStringAsync("DataTables/Person/ActionColumn", p).Result));
    }
}
```

_Note: When you use dependencies within your mapping profile, you'll have to inject these dependencies
into the profile yourself when initializing your `Mapper`. Of course you'll need to register or create
all of the required dependencies before you can pass them to the profile. We provide an extension method
for the `IServiceCollection` called `services.AddDataTablesTemplateMapper()` which does this for you._

### Customizing the Table Rendering

For the method `string RenderHtml()` required by the `IDataTable` interface, a default implementation
has been added to the abstract `DataTable` base class. It renders the table with the help of some
other methods, of which all are marked as `virtual` which allows you to override their implementations
seemlessly:

```csharp
public virtual string RenderHtml()
{
    var sb = new StringBuilder();

    sb.Append($"<table id=\"{GetTableIdentifier()}\">");

    sb.Append(RenderTableHeader());
    sb.Append(RenderTableBody());
    sb.Append(RenderTableFooter());

    sb.Append("</table>");

    return sb.ToString();
}

protected virtual string RenderTableHeader() { ... }
protected virtual string RenderTableHeaderColumn(DataTablesColumn<TEntity, TEntityViewModel> column) { ... }
protected virtual string RenderTableBody() { ... }
protected virtual string RenderTableFooter() { ... }
protected virtual string RenderTableFooterColumn(DataTablesColumn<TEntity, TEntityViewModel> column) { ... }
```

### Extending DataTables with Plugins

Extending your DataTables with plugins is no big deal. The default implementation of `RenderScript(string url, string method)`
produces an output like this:

```js
var dt_PersonDataTable = $('#PersonDataTable').DataTable({
    ajax: 'https://localhost:5001/Persons/TableData',
    method: 'post',
    columns: [...],
    stateSave: false,
    // more options ...
});
```

Using the `personDataTable.GetTableIdentifier()` method, you have access to the table identifier which is `PersonDataTable`
in above example. This means you can simply add your plugin JavaScript code in your view right after rendering the
DataTable in order to have access to the table through the `dt_PersonDataTable` variable.

As an example, you can have a look at the [sample project](DataTables.NetStandard.Sample/) where we are using a DataTable extension package called
[yadcf](https://github.com/vedmack/yadcf). It provides filters for individual columns and can be initialized easily.
For better illustration, here a full example including the rendering of the DataTable script:

```html
<script type="text/javascript">
    $(document).ready(function () {
        @Html.Raw(PersonDataTable.RenderScript(Url.Action("TableData", "Persons")))

        yadcf.init(
            dt_@Html.Raw(PersonDataTable.GetTableIdentifier()),
            [
                { column_number: 0, filter_type: 'text', filter_delay: 350, filter_reset_button_text: false, style_class: 'form-control-sm', exclude: true },
                { column_number: 1, filter_type: 'text', filter_delay: 350, filter_reset_button_text: false, style_class: 'form-control-sm' },
                { column_number: 2, filter_type: 'text', filter_delay: 350, filter_reset_button_text: false, style_class: 'form-control-sm' },
                { column_number: 5, filter_type: 'select', filter_reset_button_text: false, style_class: 'form-control-sm', data: @Html.Raw(JsonConvert.SerializeObject(PersonDataTable.GetDistinctColumnValues("postCode")))}
            ],
            { filters_position: 'footer' }
        );
    });
</script>
```

## Enhanced DataTables

Because filters are an essential part of a DataTable, there is an extension package for `DataTables.NetStandard` which utilizes the great
and aforementioned [yadcf](https://github.com/vedmack/yadcf) library to add built-in support for filters on a per-column basis.
The extension package is written in a way that allows for easy configuration of the filters (although sensible defaults are used anyway).

To make this work, an abstract `EnhancedDataTable` base class is provided by this package, extending on the abstract `DataTable` base class.
This base class provides additional configuration options for filters and customizes the script rendering of the base package to add
script rendering for the `yadcf` filters defined on individual columns.

### Usage

To use the enhanced tables, you only need to base your tables on the `EnhancedDataTable` base class instead of `DataTable`.
You will also need to define `EnhancedColumns()` instead of `Columns()`:

```csharp
public class PersonDataTable : EnhancedDataTable<Person, PersonViewModel>
{
    public override IList<EnhancedDataTablesColumn<Person, PersonViewModel>> EnhancedColumns()
    {
        return new List<EnhancedDataTablesColumn<Person, PersonViewModel>>
        {
            new EnhancedDataTablesColumn<Person, PersonViewModel>
            {
                PublicName = "name",
                DisplayName = "Name",
                PublicPropertyName = nameof(PersonViewModel.Name),
                PrivatePropertyName = nameof(Person.Name),
                IsOrderable = true,
                IsSearchable = true,
                ColumnFilter = new TextInputFilter()
            },
            // More columns ...
        };
    }
}
```

### Supported Filters

Currently, the following filters are supported by this package. You can implement your own filters though (and share them
by making a Pull Request :smile:).

#### TextInput Filter

The most basic filter is the `TextInputFilter`. It provides a way to use free-text search on a per-column basis, just like the
global filter already supported by the base package. Usage is as simple as:

```csharp
new EnhancedDataTablesColumn<Person, PersonViewModel>
{
    PublicName = "name",
    DisplayName = "Name",
    PublicPropertyName = nameof(PersonViewModel.Name),
    PrivatePropertyName = nameof(Person.Name),
    IsOrderable = true,
    IsSearchable = true,
    ColumnFilter = new TextInputFilter()
}
```

#### Select Filter

For columns with a well-defined set of values (like enums) or colums with a finite set of values (like a `country` column),
this filter provides a way to display a select filter that contains these well-defined sets of values:

```csharp
new EnhancedDataTablesColumn<Person, PersonViewModel>
{
    PublicName = "country",
    DisplayName = "Country",
    PublicPropertyName = nameof(PersonViewModel.Country),
    PrivatePropertyName = $"{nameof(Person.Location)}.{nameof(Location.Country)}",
    IsOrderable = true,
    IsSearchable = true,
    ColumnFilter = new SelectFilter<Person>(p => new LabelValuePair(p.Location.Country))
},
```

The filter implements the `IFilterWithSelectableData` interface. For all filters of this type, the `EnhancedDataTable` will load
distinct values based on the given `LabelValuePair` when rendering the table or when returning an ajax response to update the filters
with the remaining set of possible values (cumulative search).
This will only happen if you pass a `Expression<Func<TEntity, LabelValuePair>>` to the filter constructor as seen in the example above.
Alternatively, you can also pass an `IList<LabelValuePair>` with the options to display. This is useful if you want to display
the localized options of an enum for example:

```csharp
new EnhancedDataTablesColumn<Person, PersonViewModel>
{
    PublicName = "country",
    DisplayName = "Country",
    PublicPropertyName = nameof(PersonViewModel.Country),
    PrivatePropertyName = $"{nameof(Person.Location)}.{nameof(Location.Country)}",
    IsOrderable = true,
    IsSearchable = true,
    ColumnFilter = new SelectFilter<Person>(_countryRepository.GetAll())
},
```

Options of a select filter can also display a label different to the value they represent. This is especially useful if you want to
display an element of a foreign table using the values of the foreign table while searching with the foreign key:

```csharp
new EnhancedDataTablesColumn<Person, PersonViewModel>
{
    PublicName = "fullAddress",
    DisplayName = "Full Address",
    PublicPropertyName = nameof(PersonViewModel.FullAddress),
    PrivatePropertyName = nameof(Person.Location.Id),
    IsOrderable = true,
    IsSearchable = true,
    SearchPredicate = (p, s) => p.Location.Id == s.ParseAsIntOrDefault(0),
    ColumnFilter = new SelectFilter<Person>(p => new LabelValuePair(p.Location.FullAddress, p.Location.Id.ToString()))
}
```

_Please note that also the value of a `LabelValuePair` has always to be a string as the search of DataTables works with strings only.
You can still perform foreign key search in the database by using a proper `SearchPredicate` as shown in the example above, though.
In the example, an extension method for the `string` class is used which tries to parse the string as `int` and returns a default on error._

### Filter Configuration

When configuring your filters with additional options, you can always choose between configuring only one instance of a filter
or all of your filters through a global configuration.

#### Global Defaults

You can customize some of the filter options globally through the `EnhancedDataTablesConfiguration` class which holds a singleton
of the `DataTablesFilterConfiguration`. You can do this anywhere in your code. Changes to this configuration will be picked up
by your `EnhancedDataTables` if the configuration changes were made before the table script has been rendered. It is recommended
to place this configuration somewhere in the `Startup` class though:

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Configure DataTables filters
        EnhancedDataTablesConfiguration.FilterConfiguration.DefaultSelectionLabelValue = "Select something";
        EnhancedDataTablesConfiguration.FilterConfiguration.DefaultTextInputPlaceholderValue = "Type to find";

        // Other application configuration...
    }
}
```

#### Per-Usage Configuration

Alternatively, you can also configure your filters when defining your table columns:

```csharp
new EnhancedDataTablesColumn<Person, PersonViewModel>
{
    PublicName = "country",
    DisplayName = "Country",
    PublicPropertyName = nameof(PersonViewModel.Country),
    PrivatePropertyName = $"{nameof(Person.Location)}.{nameof(Location.Country)}",
    IsOrderable = true,
    IsSearchable = true,
    ColumnFilter = new SelectFilter<Person>(p => new LabelValuePair(p.Location.Country, p.Location.Country))
    {
        DefaultSelectionLabelValue = "Choose something",
    }
}
```

## Credits

Some of the code in this repository was inspired by Alexander Krutovs project called [DataTables.Queryable](https://github.com/AlexanderKrutov/DataTables.Queryable).

## License

The code is licensed under the [MIT license](LICENSE.md).
