using AgendaApi_Blue.Models;
using FluentValidation;

namespace AgendaApi_Blue.Validations
{
    public class ContatoValidator : AbstractValidator<Contato>
    {
        public ContatoValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome não pode ser vazio.")
                .Length(3, 100).WithMessage("O nome deve ter entre 3 e 100 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O email não pode ser vazio.")
                .EmailAddress().WithMessage("O email deve ser válido.");

            RuleFor(x => x.Telefone)
                .NotEmpty().WithMessage("O telefone não pode ser vazio.")
                .Matches(@"^\(\d{2}\) \d{4,5}-\d{4}$").WithMessage("O telefone deve estar no formato (XX) 9XXXX-XXXX.");
        }
    }
}