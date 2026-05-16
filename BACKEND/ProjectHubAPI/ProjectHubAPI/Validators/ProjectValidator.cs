using FluentValidation;
using ProjectHubAPI.DTOs;

namespace ProjectHubAPI.Validators
{
    public class CreateProjectValidator : AbstractValidator<CreateProjectDto>
    {
        public CreateProjectValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Project name is required")
                .MaximumLength(100).WithMessage("Project name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

            RuleFor(x => x.Budget)
                .GreaterThanOrEqualTo(0).WithMessage("Budget cannot be negative");

            RuleFor(x => x.DueDate)
                .GreaterThan(System.DateTime.Now).WithMessage("Due date must be in the future")
                .When(x => x.DueDate.HasValue);
        }
    }
}
 
