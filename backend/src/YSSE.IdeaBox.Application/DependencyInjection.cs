using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Application.Services.Admin;
using YSSE.IdeaBox.Application.Services.Engagement;
using YSSE.IdeaBox.Application.Services.Ideas;
using YSSE.IdeaBox.Application.Services.Identity;
using YSSE.IdeaBox.Application.Validators;

namespace YSSE.IdeaBox.Application;

/// <summary>Registers the application layer's services and validators.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Identity
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<ICategoryService, CategoryService>();

        // Ideas
        services.AddScoped<IIdeaService, IdeaService>();
        services.AddScoped<IScreeningService, ScreeningService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IProblemService, ProblemService>();

        // Engagement
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IVotingService, VotingService>();
        services.AddScoped<ILeaderboardService, LeaderboardService>();
        services.AddScoped<IInquiryService, InquiryService>();
        services.AddScoped<IMessagingService, MessagingService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IOutcomeService, OutcomeService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IMarketplaceService, MarketplaceService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Administration
        services.AddScoped<IAdminService, AdminService>();

        // Validators
        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<IdeaCreateRequest>, IdeaCreateRequestValidator>();
        services.AddScoped<IValidator<ReviewCreateRequest>, ReviewCreateRequestValidator>();
        services.AddScoped<IValidator<InquiryCreateRequest>, InquiryCreateRequestValidator>();
        services.AddScoped<IValidator<MessageSendRequest>, MessageSendRequestValidator>();
        services.AddScoped<IValidator<ListingCreateRequest>, ListingCreateRequestValidator>();
        services.AddScoped<IValidator<CategoryUpsertRequest>, CategoryUpsertRequestValidator>();
        services.AddScoped<IValidator<ProblemPostCreateRequest>, ProblemPostCreateRequestValidator>();

        return services;
    }
}