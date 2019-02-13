using AutoMapper;
using DataTables.NetCore.Sample.Models;
using DataTables.NetCore.ViewRenderer;

namespace DataTables.NetCore.Sample.DataTables.ViewModels
{
    public class DefaultMappingProfile : Profile
    {
        public DefaultMappingProfile(IViewRenderService viewRenderService)
        {
            CreateMap<User, UserViewModel>()
                .ForMember(vm => vm.Address, m => m.MapFrom(u => $"{u.Location.Street} {u.Location.HouseNumber}"))
                .ForMember(vm => vm.PostCode, m => m.MapFrom(u => u.Location.PostCode))
                .ForMember(vm => vm.City, m => m.MapFrom(u => u.Location.City))
                .ForMember(vm => vm.Country, m => m.MapFrom(u => u.Location.Country))

                // Raw columns containing some HTML (like action buttons) consist of simple strings. This means
                // you can basically add a string column on the view model which does not have to exist on the
                // query model and return some custom HTML for it here in the mapper. In this example we are simply
                // building a link inline. The following two columns do the same but using file-based templates.
                .ForMember(vm => vm.Action, m => m.MapFrom(u => $"<a href=\"#user-{u.Id}\">Link 1</a>"))

                // This uses the package Scriban which parses Liquid templates and renders them with the row data.
                // The Scriban package does not require any dependency injection and offers static methods, which
                // makes it a very easy to use library. The template language Liquid is quite different from Razor
                // though, so it can be a bit of work to get used to it.
                // Probably important: If the row object (user) is passed directly as second argument, its properties
                // will be accessible in the template directly (i.e. <code>u.Id</code> -> <code>{{ id }}</code>).
                // If the row object is wrapped in another object like <code>new { User = u }</code>, the properties
                // will be accessible with <code>{{ user.id }}</code> for example.
                // Important: Template files have to be copied to the output folder during builds. Make sure this
                //            setting is set correctly in the file properties.
                .ForMember(vm => vm.Action2, m => m.MapFrom(u => ViewRenderService.RenderLiquidToString("DataTables/User/Action.twig", u)))

                // This renders the given view as Razor template through the ASP.NET Core MVC Razor engine. Rendering
                // the view this way allows you to use basically all Razor functions available. There is a significant
                // downside to this though: The AutoMapper profile (this class) has to receive the IViewRenderService
                // from the dependency injector somehow, which does not happen by itself and is only possible through
                // a hack in the Startup.ConfigureService() method. Have a look there to learn more about it.
                .ForMember(vm => vm.Action3, m => m.MapFrom(u => viewRenderService.RenderRazorToStringAsync("DataTables/User/ActionColumn", u).Result));
        }
    }
}
