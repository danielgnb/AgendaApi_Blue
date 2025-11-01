using AutoMapper;
using AgendaApi_Blue.Models;
using AgendaApi_Blue.Models.ViewModels.Usuario;
using AgendaApi_Blue.Models.ViewModels.Contato;
using AgendaApi_Blue.Models.ViewModels.Auth;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<LoginViewModel, Usuario>();
        CreateMap<UsuarioViewModel, Usuario>();
        CreateMap<ContatoViewModel, Contato>();
    }
}