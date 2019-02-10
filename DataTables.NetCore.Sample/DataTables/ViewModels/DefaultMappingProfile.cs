using AutoMapper;
using DataTables.NetCore.Sample.Models;

namespace DataTables.NetCore.Sample.DataTables.ViewModels
{
    public class DefaultMappingProfile : Profile
    {
        public DefaultMappingProfile()
        {
            CreateMap<User, UserViewModel>()
                .ForMember(vm => vm.Address, m => m.MapFrom(u => $"{u.Location.Street} {u.Location.HouseNumber}"))
                .ForMember(vm => vm.PostCode, m => m.MapFrom(u => u.Location.PostCode))
                .ForMember(vm => vm.City, m => m.MapFrom(u => u.Location.City))
                .ForMember(vm => vm.Country, m => m.MapFrom(u => u.Location.Country));
        }
    }
}
