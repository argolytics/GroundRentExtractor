using AutoMapper;
using DataLibrary.Dtos;
using DataLibrary.Models;

namespace DataLibrary.AutoMapperProfiles;
public class AddressProfile : Profile
{
    public AddressProfile()
    {
        CreateMap<AddressDto, AddressModel>();
    }
}
