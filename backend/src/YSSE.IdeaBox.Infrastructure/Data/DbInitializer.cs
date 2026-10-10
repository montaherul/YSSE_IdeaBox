using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Infrastructure.Data;

/// <summary>
/// Idempotent database bootstrap: applies migrations (when requested) and seeds the
/// reference data and demo content the UI needs. Safe to run repeatedly.
/// </summary>
public static class DbInitializer
{
    private static readonly string[] PermissionCodes =
    {
        "idea.publish",
        "user.manage",
        "voting.configure",
        "moderation.manage",
        "audit.view",
        "setting.manage",
        "category.manage"
    };

    private static readonly string[] CategoryNames =
    {
        "Health", "Education", "Agriculture", "Technology", "Environment", "Community", "Transport", "Energy"
    };

    private static readonly string[] TagNames =
    {
        "Solar", "Water", "AI", "Education", "Recycling", "Mobility", "Health", "Finance", "Youth", "Climate"
    };

    public static async Task InitializeAsync(this IServiceProvider services, bool migrate, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var db = provider.GetRequiredService<AppDbContext>();
        var hasher = provider.GetRequiredService<IPasswordHasher>();
        var hashing = provider.GetRequiredService<IHashingService>();
        var clock = provider.GetRequiredService<IDateTimeProvider>();
        var now = clock.UtcNow;

        if (migrate)
        {
            await db.Database.MigrateAsync(ct);
        }

        await SeedRolesAsync(db, now, ct);
        await SeedPermissionsAsync(db, now, ct);
        await SeedPlatformSettingsAsync(db, now, ct);
        await SeedUsersAsync(db, hasher, now, ct);
        await SeedCategoriesAsync(db, now, ct);
        await SeedTagsAsync(db, now, ct);
        await SeedProfilesAsync(db, now, ct);

        if (!await db.Ideas.AnyAsync(i => !i.IsDeleted, ct))
        {
            await SeedSampleContentAsync(db, hashing, now, ct);
        }
    }

    // ------------------------------------------------------------------ reference data

    private static async Task SeedRolesAsync(AppDbContext db, DateTime now, CancellationToken ct)
    {
        var existing = await db.Roles.Select(r => r.Name).ToListAsync(ct);

        foreach (var role in Enum.GetValues<RoleName>())
        {
            if (!existing.Contains(role))
            {
                db.Roles.Add(new Role { Name = role, Description = Humanize(role), CreatedAt = now });
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedPermissionsAsync(AppDbContext db, DateTime now, CancellationToken ct)
    {
        var existingCodes = await db.Permissions.Select(p => p.Code).ToListAsync(ct);

        foreach (var code in PermissionCodes)
        {
            if (!existingCodes.Contains(code))
            {
                db.Permissions.Add(new Permission { Code = code, Description = Humanize(code), CreatedAt = now });
            }
        }

        await db.SaveChangesAsync(ct);

        var adminRoleId = await db.Roles.Where(r => r.Name == RoleName.Administrator).Select(r => r.Id).FirstOrDefaultAsync(ct);
        if (adminRoleId == 0)
        {
            return;
        }

        var permissionIds = await db.Permissions
            .Where(p => PermissionCodes.Contains(p.Code))
            .Select(p => p.Id)
            .ToListAsync(ct);

        var granted = await db.RolePermissions
            .Where(rp => rp.RoleId == adminRoleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync(ct);

        foreach (var permissionId in permissionIds)
        {
            if (!granted.Contains(permissionId))
            {
                db.RolePermissions.Add(new RolePermission { RoleId = adminRoleId, PermissionId = permissionId, CreatedAt = now });
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedPlatformSettingsAsync(AppDbContext db, DateTime now, CancellationToken ct)
    {
        var defaults = new (string Key, string Value, string Description)[]
        {
            ("review.policy", "manual_moderation", "Community reviews are moderated before publication."),
            ("data.retention.days", "365", "Days visitor/analytics data is retained before purging."),
            ("guest.review.policy", "one_review_per_idea", "Best-effort limit for anonymous guest reviews."),
            ("marketplace.listing.expiry.days", "60", "Days a marketplace listing stays active before expiry."),
            ("inquiry.daily.limit", "10", "Maximum inquiries a seeker may open per day."),
            ("voting.visibility", "hidden_until_close", "Default vote visibility for new voting periods.")
        };

        var existingKeys = await db.PlatformSettings.Select(s => s.Key).ToListAsync(ct);

        foreach (var (key, value, description) in defaults)
        {
            if (!existingKeys.Contains(key))
            {
                db.PlatformSettings.Add(new PlatformSetting { Key = key, Value = value, Description = description, CreatedAt = now });
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedUsersAsync(AppDbContext db, IPasswordHasher hasher, DateTime now, CancellationToken ct)
    {
        var admin = await EnsureUserAsync(db, hasher, "admin@ysse.local", "Admin@12345", "Platform Administrator", now, ct);
        await EnsureUserRoleAsync(db, admin.Id, RoleName.Administrator, now, ct);
        await EnsureUserRoleAsync(db, admin.Id, RoleName.ScreeningReviewer, now, ct);
        await EnsureUserRoleAsync(db, admin.Id, RoleName.Moderator, now, ct);
        await EnsureUserRoleAsync(db, admin.Id, RoleName.Member, now, ct);

        var creator = await EnsureUserAsync(db, hasher, "creator@ysse.local", "Creator@12345", "Amina Yusuf", now, ct);
        await EnsureUserRoleAsync(db, creator.Id, RoleName.Creator, now, ct);
        await EnsureUserRoleAsync(db, creator.Id, RoleName.Member, now, ct);

        var member = await EnsureUserAsync(db, hasher, "member@ysse.local", "Member@12345", "Daniel Okoro", now, ct);
        await EnsureUserRoleAsync(db, member.Id, RoleName.Member, now, ct);

        var investor = await EnsureUserAsync(db, hasher, "investor@ysse.local", "Investor@12345", "Grace Mensah", now, ct);
        await EnsureUserRoleAsync(db, investor.Id, RoleName.Investor, now, ct);
        await EnsureUserRoleAsync(db, investor.Id, RoleName.Member, now, ct);

        var entrepreneur = await EnsureUserAsync(db, hasher, "entrepreneur@ysse.local", "Entrepreneur@12345", "Samuel Adeyemi", now, ct);
        await EnsureUserRoleAsync(db, entrepreneur.Id, RoleName.Entrepreneur, now, ct);
        await EnsureUserRoleAsync(db, entrepreneur.Id, RoleName.Member, now, ct);

        var moderator = await EnsureUserAsync(db, hasher, "moderator@ysse.local", "Moderator@12345", "Fatima Bello", now, ct);
        await EnsureUserRoleAsync(db, moderator.Id, RoleName.Moderator, now, ct);
        await EnsureUserRoleAsync(db, moderator.Id, RoleName.Member, now, ct);
    }

    private static async Task<User> EnsureUserAsync(AppDbContext db, IPasswordHasher hasher, string email,
        string password, string displayName, DateTime now, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is not null)
        {
            return user;
        }

        user = new User
        {
            Email = email,
            PasswordHash = hasher.Hash(password),
            DisplayName = displayName,
            IsEmailVerified = true,
            CreatedAt = now
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        db.MemberProfiles.Add(new MemberProfile
        {
            UserId = user.Id,
            DisplayName = displayName,
            PreferredLanguage = "en",
            CreatedAt = now
        });

        await db.SaveChangesAsync(ct);
        return user;
    }

    private static async Task EnsureUserRoleAsync(AppDbContext db, int userId, RoleName role, DateTime now, CancellationToken ct)
    {
        var roleId = await db.Roles.Where(r => r.Name == role).Select(r => r.Id).FirstOrDefaultAsync(ct);
        if (roleId == 0)
        {
            return;
        }

        var exists = await db.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);
        if (exists)
        {
            return;
        }

        db.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId, GrantedAt = now, CreatedAt = now });
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedCategoriesAsync(AppDbContext db, DateTime now, CancellationToken ct)
    {
        var existing = await db.Categories.Select(c => c.Slug).ToListAsync(ct);

        foreach (var name in CategoryNames)
        {
            var slug = Slugify(name);
            if (!existing.Contains(slug))
            {
                db.Categories.Add(new Category
                {
                    Name = name,
                    Slug = slug,
                    Description = $"{name} related ideas and solutions.",
                    IsActive = true,
                    CreatedAt = now
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedTagsAsync(AppDbContext db, DateTime now, CancellationToken ct)
    {
        var existing = await db.Tags.Select(t => t.Slug).ToListAsync(ct);

        foreach (var name in TagNames)
        {
            var slug = Slugify(name);
            if (!existing.Contains(slug))
            {
                db.Tags.Add(new Tag { Name = name, Slug = slug, CreatedAt = now });
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedProfilesAsync(AppDbContext db, DateTime now, CancellationToken ct)
    {
        var creator = await db.Users.FirstOrDefaultAsync(u => u.Email == "creator@ysse.local", ct);
        if (creator is not null && !await db.CreatorProfiles.AnyAsync(p => p.UserId == creator.Id, ct))
        {
            db.CreatorProfiles.Add(new CreatorProfile
            {
                UserId = creator.Id,
                Alias = "amina-creator",
                Headline = "Community health innovator",
                ShortBio = "Building low-cost health solutions for rural northern Nigeria.",
                District = "Kaduna",
                Languages = "English, Hausa",
                Availability = "Weekends",
                CreatedAt = now
            });
            await db.SaveChangesAsync(ct);
        }

        var entrepreneur = await db.Users.FirstOrDefaultAsync(u => u.Email == "entrepreneur@ysse.local", ct);
        if (entrepreneur is not null && !await db.EntrepreneurProfiles.AnyAsync(p => p.UserId == entrepreneur.Id, ct))
        {
            db.EntrepreneurProfiles.Add(new EntrepreneurProfile
            {
                UserId = entrepreneur.Id,
                Kind = OpportunitySeekerKind.Investor,
                Organization = "Sahel Ventures",
                Industry = "Agritech",
                Description = "Early-stage fund backing climate-smart agriculture.",
                CoarseLocation = "Abuja",
                IsVerified = true,
                IsApproved = true,
                ApprovedAt = now,
                CreatedAt = now
            });
            await db.SaveChangesAsync(ct);
        }
    }

    // ------------------------------------------------------------------ sample content

    private static async Task SeedSampleContentAsync(AppDbContext db, IHashingService hashing, DateTime now, CancellationToken ct)
    {
        var creator = await db.Users.FirstAsync(u => u.Email == "creator@ysse.local", ct);
        var member = await db.Users.FirstAsync(u => u.Email == "member@ysse.local", ct);
        var investor = await db.Users.FirstAsync(u => u.Email == "investor@ysse.local", ct);
        var entrepreneur = await db.Users.FirstAsync(u => u.Email == "entrepreneur@ysse.local", ct);
        var admin = await db.Users.FirstAsync(u => u.Email == "admin@ysse.local", ct);

        var categories = await db.Categories.ToDictionaryAsync(c => c.Name, ct);
        Category Cat(string name) => categories[name];

        var ideas = new List<Idea>
        {
            NewIdea("Community Health Clinic Mapping", "A live map of the nearest clinics and services for rural communities.",
                Cat("Health"), creator.Id, IdeaStatus.Published, IdeaAttribution.Public,
                now.AddDays(-45), now.AddDays(-40), now.AddDays(-42)),
            NewIdea("Solar-Powered Irrigation for Smallholders", "Affordable solar irrigation kits that cut pumping costs for small farms.",
                Cat("Agriculture"), creator.Id, IdeaStatus.Published, IdeaAttribution.Public,
                now.AddDays(-38), now.AddDays(-34), now.AddDays(-35)),
            NewIdea("AI Tutor for Under-Resourced Schools", "An offline-first AI tutor that adapts to each learner's pace.",
                Cat("Education"), member.Id, IdeaStatus.UnderReview, IdeaAttribution.Public,
                now.AddDays(-12), null, now.AddDays(-11)),
            NewIdea("Neighbourhood Plastic Recycling Exchange", "Residents trade sorted plastic for household goods at kiosk points.",
                Cat("Environment"), member.Id, IdeaStatus.Published, IdeaAttribution.Anonymous,
                now.AddDays(-30), now.AddDays(-25), now.AddDays(-27)),
            NewIdea("Rural Mobility Ride-Sharing", "A community ride board connecting drivers and passengers on rural routes.",
                Cat("Transport"), creator.Id, IdeaStatus.Draft, IdeaAttribution.Public,
                null, null, null),
            NewIdea("Mobile Blood Donation Network", "A verified donor network that alerts nearby donors during shortages.",
                Cat("Health"), creator.Id, IdeaStatus.Shortlisted, IdeaAttribution.Public,
                now.AddDays(-60), now.AddDays(-55), now.AddDays(-57)),
            NewIdea("Youth Skills Marketplace", "A marketplace connecting young artisans with paid micro-projects.",
                Cat("Community"), member.Id, IdeaStatus.Published, IdeaAttribution.Public,
                now.AddDays(-22), now.AddDays(-18), now.AddDays(-20)),
            NewIdea("Flood Early-Warning Sensors", "Low-cost water-level sensors that push SMS alerts before flash floods.",
                Cat("Environment"), member.Id, IdeaStatus.UnderReview, IdeaAttribution.Anonymous,
                now.AddDays(-9), null, now.AddDays(-8))
        };

        db.Ideas.AddRange(ideas);
        await db.SaveChangesAsync(ct);

        var published = ideas.Where(i => i.Status == IdeaStatus.Published).ToList();
        var underReview = ideas.Where(i => i.Status == IdeaStatus.UnderReview).ToList();

        // Tags
        var tagMap = await db.Tags.ToDictionaryAsync(t => t.Name, ct);
        void Tag(Idea idea, params string[] names)
        {
            foreach (var name in names)
            {
                if (tagMap.TryGetValue(name, out var tag))
                {
                    db.IdeaTags.Add(new IdeaTag { IdeaId = idea.Id, TagId = tag.Id, CreatedAt = now });
                }
            }
        }

        Tag(ideas[0], "Health", "Water");
        Tag(ideas[1], "Solar", "Climate");
        Tag(ideas[2], "AI", "Education");
        Tag(ideas[3], "Recycling", "Climate");
        Tag(ideas[5], "Health", "Finance");
        Tag(ideas[6], "Youth", "Finance");
        Tag(ideas[7], "Climate", "Water");

        // Status history
        foreach (var idea in published)
        {
            db.IdeaStatusHistories.Add(new IdeaStatusHistory
            {
                IdeaId = idea.Id,
                FromStatus = IdeaStatus.UnderReview,
                ToStatus = IdeaStatus.Published,
                ActorUserId = admin.Id,
                Reason = "Passed screening.",
                CreatedAt = idea.PublishedAt ?? now
            });
        }

        foreach (var idea in underReview)
        {
            db.IdeaStatusHistories.Add(new IdeaStatusHistory
            {
                IdeaId = idea.Id,
                FromStatus = IdeaStatus.Submitted,
                ToStatus = IdeaStatus.UnderReview,
                ActorUserId = admin.Id,
                Reason = "Entered screening queue.",
                CreatedAt = now.AddDays(-2)
            });

            db.IdeaScreeningDecisions.Add(new IdeaScreeningDecision
            {
                IdeaId = idea.Id,
                ReviewerUserId = admin.Id,
                Decision = IdeaStatus.UnderReview,
                OriginalityScore = 4,
                FeasibilityScore = 3,
                RelevanceScore = 5,
                ImpactScore = 4,
                Feedback = "Promising concept; add cost and feasibility detail.",
                CreatedAt = now.AddDays(-2)
            });
        }

        await db.SaveChangesAsync(ct);

        // Live voting period
        var votingPeriod = new VotingPeriod
        {
            Name = "Community Choice 2026",
            StartAt = now.AddDays(-7),
            EndAt = now.AddDays(30),
            Visibility = VoteVisibility.HiddenUntilClose,
            MinimumAccountAgeDays = 0,
            RequiresVerifiedAccount = false,
            IsActive = true,
            IsClosed = false,
            CreatedAt = now.AddDays(-7)
        };
        db.VotingPeriods.Add(votingPeriod);
        await db.SaveChangesAsync(ct);

        // Votes on published ideas
        foreach (var idea in published.Take(4))
        {
            db.IdeaVotes.Add(new IdeaVote { IdeaId = idea.Id, UserId = investor.Id, VotingPeriodId = votingPeriod.Id, IsCounted = true, CreatedAt = now.AddDays(-3) });
            db.IdeaVotes.Add(new IdeaVote { IdeaId = idea.Id, UserId = member.Id, VotingPeriodId = votingPeriod.Id, IsCounted = true, CreatedAt = now.AddDays(-2) });
        }

        await db.SaveChangesAsync(ct);

        // Reviews (registered + guest) on the first published idea
        var reviewIdea = published[0];
        var guest = new GuestVisitor
        {
            GuestIdHash = hashing.Sha256("seed-guest-1"),
            IpHash = hashing.Sha256("203.0.113.10"),
            FirstReviewIdeaId = reviewIdea.Id,
            FirstSeenAt = now.AddDays(-5),
            LastSeenAt = now.AddDays(-1),
            CreatedAt = now.AddDays(-5)
        };
        db.GuestVisitors.Add(guest);
        await db.SaveChangesAsync(ct);

        db.IdeaReviews.AddRange(
            new IdeaReview
            {
                IdeaId = reviewIdea.Id,
                ReviewerUserId = investor.Id,
                Body = "Strong social impact. I would like to understand the data-collection model.",
                Rating = 4,
                IsGuest = false,
                ModerationStatus = ReviewModerationStatus.Published,
                CreatedAt = now.AddDays(-4)
            },
            new IdeaReview
            {
                IdeaId = reviewIdea.Id,
                GuestVisitorId = guest.Id,
                Body = "This would genuinely help my community. Please keep going.",
                Rating = 5,
                IsGuest = true,
                ModerationStatus = ReviewModerationStatus.Published,
                CreatedAt = now.AddDays(-3)
            });

        await db.SaveChangesAsync(ct);

        // Problem posts + endorsements + idea links
        var problem1 = new ProblemPost
        {
            AuthorUserId = member.Id,
            Title = "Clean water access in rural schools",
            WhoAffected = "Over 2,000 pupils in six rural schools drink from unprotected sources.",
            WhyItMatters = "Waterborne disease keeps children out of school and drives absenteeism.",
            CoarseLocation = "Kaduna",
            Urgency = "High",
            Status = ProblemStatus.Open,
            Attribution = "Public",
            CreatedAt = now.AddDays(-14)
        };

        var problem2 = new ProblemPost
        {
            AuthorUserId = creator.Id,
            Title = "Post-harvest losses for tomato farmers",
            WhoAffected = "Smallholder tomato farmers lose up to 40% of each harvest.",
            WhyItMatters = "Losses directly reduce household income and food availability.",
            CoarseLocation = "Kano",
            Urgency = "Medium",
            Status = ProblemStatus.SolutionsProposed,
            Attribution = "Public",
            CreatedAt = now.AddDays(-20)
        };

        db.ProblemPosts.AddRange(problem1, problem2);
        await db.SaveChangesAsync(ct);

        db.ProblemEndorsements.AddRange(
            new ProblemEndorsement { ProblemPostId = problem1.Id, UserId = investor.Id, CreatedAt = now.AddDays(-12) },
            new ProblemEndorsement { ProblemPostId = problem1.Id, UserId = entrepreneur.Id, CreatedAt = now.AddDays(-11) },
            new ProblemEndorsement { ProblemPostId = problem2.Id, UserId = investor.Id, CreatedAt = now.AddDays(-18) });

        db.IdeaProblemLinks.Add(new IdeaProblemLink
        {
            IdeaId = ideas[1].Id,
            ProblemPostId = problem2.Id,
            CreatedAt = now.AddDays(-10)
        });

        await db.SaveChangesAsync(ct);

        // Marketplace listings + reservation
        var listing = new MarketplaceListing
        {
            OwnerUserId = creator.Id,
            Title = "Refurbished laptops (lot of 5)",
            Description = "Five refurbished laptops suitable for a community learning centre.",
            Category = "Technology",
            Condition = "Used - Good",
            ListingType = ListingType.Sale,
            Price = 150000m,
            CoarseLocation = "Kaduna",
            Status = ListingStatus.Available,
            ExpiresAt = now.AddDays(45),
            CreatedAt = now.AddDays(-6)
        };

        var donation = new MarketplaceListing
        {
            OwnerUserId = member.Id,
            Title = "Assorted science textbooks",
            Description = "Donation of secondary-school science textbooks in good condition.",
            Category = "Education",
            Condition = "Used - Acceptable",
            ListingType = ListingType.Donation,
            CoarseLocation = "Lagos",
            Status = ListingStatus.Available,
            ExpiresAt = now.AddDays(30),
            CreatedAt = now.AddDays(-4)
        };

        db.MarketplaceListings.AddRange(listing, donation);
        await db.SaveChangesAsync(ct);

        db.Reservations.Add(new Reservation
        {
            MarketplaceListingId = listing.Id,
            RequesterUserId = entrepreneur.Id,
            Message = "Interested in the laptops for our learning centre; can collect this week.",
            Status = ReservationStatus.Requested,
            CreatedAt = now.AddDays(-2)
        });

        await db.SaveChangesAsync(ct);

        // Notifications
        db.Notifications.AddRange(
            new Notification
            {
                UserId = creator.Id,
                Type = NotificationType.IdeaSubmitted,
                Title = "Your idea passed screening",
                Body = $"\"{reviewIdea.Title}\" is now published.",
                LinkUrl = $"/ideas/{reviewIdea.Slug}",
                IsRead = false,
                CreatedAt = now.AddHours(-10)
            },
            new Notification
            {
                UserId = member.Id,
                Type = NotificationType.VotingOpened,
                Title = "Voting is open",
                Body = "Community Choice 2026 is now accepting votes.",
                IsRead = false,
                CreatedAt = now.AddHours(-8)
            },
            new Notification
            {
                UserId = investor.Id,
                Type = NotificationType.NewReview,
                Title = "New activity on an idea you follow",
                Body = "A guest left a new review on Community Health Clinic Mapping.",
                LinkUrl = $"/ideas/{reviewIdea.Slug}",
                IsRead = true,
                ReadAt = now.AddHours(-1),
                CreatedAt = now.AddHours(-4)
            });

        await db.SaveChangesAsync(ct);

        // Conversation + messages + inquiry
        var conversation = new Conversation
        {
            IdeaId = reviewIdea.Id,
            Status = ConversationStatus.Open,
            LastMessageAt = now.AddHours(-2),
            CreatedAt = now.AddDays(-3)
        };
        db.Conversations.Add(conversation);
        await db.SaveChangesAsync(ct);

        db.ConversationParticipants.AddRange(
            new ConversationParticipant { ConversationId = conversation.Id, UserId = investor.Id, CreatedAt = now.AddDays(-3) },
            new ConversationParticipant { ConversationId = conversation.Id, UserId = creator.Id, CreatedAt = now.AddDays(-3) });

        db.Messages.AddRange(
            new Message
            {
                ConversationId = conversation.Id,
                SenderUserId = investor.Id,
                Body = "Hello Amina, I'd like to explore supporting the clinic mapping idea.",
                DeliveredAt = now.AddHours(-3),
                ReadAt = now.AddHours(-2),
                CreatedAt = now.AddHours(-3)
            },
            new Message
            {
                ConversationId = conversation.Id,
                SenderUserId = creator.Id,
                Body = "Thank you Grace! Happy to share more detail on the rollout plan.",
                DeliveredAt = now.AddHours(-2),
                CreatedAt = now.AddHours(-2)
            });

        await db.SaveChangesAsync(ct);

        db.EntrepreneurInquiries.Add(new EntrepreneurInquiry
        {
            IdeaId = reviewIdea.Id,
            SeekerUserId = investor.Id,
            Type = InquiryType.Investment,
            Message = "We are exploring a grant for community health infrastructure and this idea fits.",
            IsAnswered = true,
            AnsweredAt = now.AddHours(-2),
            ConversationId = conversation.Id,
            CreatedAt = now.AddDays(-3)
        });

        await db.SaveChangesAsync(ct);
    }

    // ------------------------------------------------------------------ helpers

    private static Idea NewIdea(string title, string shortSummary, Category category, int creatorId,
        IdeaStatus status, IdeaAttribution attribution, DateTime? submittedAt, DateTime? publishedAt, DateTime? createdAt)
    {
        return new Idea
        {
            Slug = Slugify(title),
            Title = title,
            ShortSummary = shortSummary,
            ProblemStatement = $"{shortSummary} This entry captures the core problem, affected groups, and why it matters.",
            ProposedSolution = "A phased pilot with community partners, measurable milestones, and a lightweight operating model.",
            TargetAudience = "Underserved communities and local partners",
            ExpectedImpact = "Improved access, lower cost, and measurable community outcomes.",
            Stage = status is IdeaStatus.Draft ? DevelopmentStage.Concept : DevelopmentStage.Prototype,
            Attribution = attribution,
            CreatorUserId = creatorId,
            CategoryId = category.Id,
            Status = status,
            LanguageTag = "en",
            SubmittedAt = submittedAt,
            PublishedAt = publishedAt,
            ViewsCount = status == IdeaStatus.Draft ? 0 : Random.Shared.Next(20, 400),
            CreatedAt = createdAt ?? submittedAt ?? DateTime.UtcNow
        };
    }

    private static string Slugify(string value)
    {
        var builder = new StringBuilder(value.Length);
        var lastDash = false;

        foreach (var ch in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
                lastDash = false;
            }
            else if (!lastDash)
            {
                builder.Append('-');
                lastDash = true;
            }
        }

        var slug = builder.ToString().Trim('-');
        return slug.Length == 0 ? Guid.NewGuid().ToString("N")[..12] : slug;
    }

    private static string Humanize(RoleName role) => Humanize(role.ToString());

    private static string Humanize(string value)
    {
        var builder = new StringBuilder(value.Length + 4);

        for (var i = 0; i < value.Length; i++)
        {
            var ch = value[i];
            if (i > 0 && char.IsUpper(ch) && !char.IsUpper(value[i - 1]))
            {
                builder.Append(' ');
            }
            else if (ch is '.' or '_' or '-')
            {
                builder.Append(' ');
                continue;
            }

            builder.Append(ch);
        }

        var text = builder.ToString().Trim();
        return text.Length == 0 ? value : char.ToUpperInvariant(text[0]) + text[1..];
    }
}
