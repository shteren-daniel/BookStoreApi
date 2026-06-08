using BookStoreApi.Models;
using FluentValidation;

namespace BookStoreApi.Validators;

public class CreateBookValidator : AbstractValidator<CreateBookDto>
{
    public CreateBookValidator()
    {
        RuleFor(x => x.Isbn)
            .NotEmpty().WithMessage("ISBN is required")
            .Length(10, 20).WithMessage("ISBN length must be between 10 and 20");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200);

        RuleFor(x => x.Language)
            .NotEmpty()
            .MaximumLength(5);

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(50);

        RuleFor(x => x.Year)
            .InclusiveBetween(1000, 2500)
            .WithMessage("Year must be between 1000 and 2500");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0");

        RuleFor(x => x.Authors)
            .NotNull()
            .Must(a => a.Count > 0)
            .WithMessage("At least one author is required");

        RuleForEach(x => x.Authors)
            .NotEmpty()
            .WithMessage("Author name cannot be empty");

        RuleFor(x => x.Cover)
            .MaximumLength(200)
            .When(x => x.Cover != null);
    }
}