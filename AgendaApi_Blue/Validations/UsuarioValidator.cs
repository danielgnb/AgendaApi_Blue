using AgendaApi_Blue.Models;
using FluentValidation;

public class UsuarioValidator : AbstractValidator<Usuario>
{
    public UsuarioValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("O nome de usuário não pode ser vazio.")
            .Length(3, 50).WithMessage("O nome de usuário deve ter entre 3 e 50 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha não pode ser vazia.")
            .Length(6, 100).WithMessage("A senha deve ter entre 6 e 100 caracteres.");
    }
}