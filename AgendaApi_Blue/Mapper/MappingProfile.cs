using AutoMapper;
using AgendaApi_Blue.Models;
using AgendaApi_Blue.Models.ViewModels.Usuario;
using AgendaApi_Blue.Models.ViewModels.Contato;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UsuarioViewModel, Usuario>();
        CreateMap<ContatoViewModel, Contato>();
    }
}