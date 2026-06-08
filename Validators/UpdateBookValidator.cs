using BookStoreApi.Models;
using FluentValidation;

namespace BookStoreApi.Validators;

public class UpdateBookValidator : AbstractValidator<UpdateBookDto>
{
    public UpdateBookValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .When(x => x.Title != null)
            .WithMessage("Title cannot be empty");

        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.Language)
            .NotEmpty()
            .When(x => x.Language != null)
            .WithMessage("Language cannot be empty");

        RuleFor(x => x.Language)
            .MaximumLength(5)
            .When(x => !string.IsNullOrWhiteSpace(x.Language));

        RuleFor(x => x.Category)
            .NotEmpty()
            .When(x => x.Category != null)
            .WithMessage("Category cannot be empty");

        RuleFor(x => x.Category)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Category));


        RuleFor(x => x.Year)
            .InclusiveBetween(1000, 2500)
            .When(x => x.Year.HasValue)
            .WithMessage("Year must be between 1000 and 2500");


        RuleFor(x => x.Price)
            .GreaterThan(0)
            .When(x => x.Price.HasValue)
            .WithMessage("Price must be greater than 0");

        RuleFor(x => x.Authors)
            .NotEmpty()
            .When(x => x.Authors != null)
            .WithMessage("Authors list cannot be empty");

        RuleForEach(x => x.Authors)
            .NotEmpty()
            .When(x => x.Authors != null)
            .WithMessage("Author name cannot be empty");

        RuleFor(x => x.Cover)
            .MaximumLength(200)
            .When(x => x.Cover != null);
    }
}