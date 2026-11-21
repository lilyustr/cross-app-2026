using Api.Contracts;
using FluentValidation;

namespace Api.Validation;

public sealed class SaveProductRequestValidator : AbstractValidator<SaveProductRequest>
{
    public SaveProductRequestValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("Артикул обов'язковий.")
            .Must(s => s is null || s.Trim().Length <= 50)
            .WithMessage("Артикул має містити до 50 символів.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва обов'язкова.")
            .Must(s => s is null || s.Trim().Length <= 200)
            .WithMessage("Назва має містити до 200 символів.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Одиниця виміру обов'язкова.")
            .Must(s => s is null || s.Trim().Length <= 20)
            .WithMessage("Одиниця виміру має містити до 20 символів.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Кількість не може бути від'ємною.");
    }
}