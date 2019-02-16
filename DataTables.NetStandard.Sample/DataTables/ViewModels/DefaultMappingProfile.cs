using AutoMapper;
using DataTables.NetStandard.Sample.Models;
using DataTables.NetStandard.TemplateMapper;

namespace DataTables.NetStandard.Sample.DataTables.ViewModels
{
    public class DefaultMappingProfile : Profile
    {
        public DefaultMappingProfile(IViewRenderService viewRenderService)
        {
            CreateMap<Person, PersonViewModel>()
                .ForMember(vm => vm.Address, m => m.MapFrom(p => $"{p.Location.Street} {p.Location.HouseNumber}"))
                .ForMember(vm => vm.PostCode, m => m.MapFrom(p => p.Location.PostCode))
                .ForMember(vm => vm.City, m => m.MapFrom(p => p.Location.City))
                .ForMember(vm => vm.Country, m => m.MapFrom(p => p.Location.Country))

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
                .ForMember(vm => vm.Action2, m => m.MapFrom(p => ViewRenderService.RenderLiquidTemplateFileWithData("DataTables/Person/Action.twig", p)))

                // This renders the given view as Razor template through the ASP.NET Core MVC Razor engine. Rendering
                // the view this way allows you to use basically all Razor functions available. There is a significant
                // downside to this though: The AutoMapper profile (this class) has to receive the IViewRenderService
                // from the dependency injector somehow, which does not happen by itself and is only possible through
                // a hack in the Startup.ConfigureService() method. Have a look there to learn more about it.
                .ForMember(vm => vm.Action3, m => m.MapFrom(p => viewRenderService.RenderRazorToStringAsync("DataTables/Person/ActionColumn", p).Result));
        }
    }
}
