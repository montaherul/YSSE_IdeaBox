using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace YSSE.IdeaBox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "conversations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    idea_id = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    last_message_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conversations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "guest_visitors",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guest_id_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ip_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    first_review_idea_id = table.Column<int>(type: "integer", nullable: true),
                    first_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_guest_visitors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organization_partners",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_partners", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "platform_settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_platform_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    password_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    display_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    is_email_verified = table.Column<bool>(type: "boolean", nullable: false),
                    is_phone_verified = table.Column<bool>(type: "boolean", nullable: false),
                    is_suspended = table.Column<bool>(type: "boolean", nullable: false),
                    suspension_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_minor = table.Column<bool>(type: "boolean", nullable: false),
                    is_pending_deletion = table.Column<bool>(type: "boolean", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "voting_periods",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    start_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    visibility = table.Column<int>(type: "integer", nullable: false),
                    minimum_account_age_days = table.Column<int>(type: "integer", nullable: false),
                    requires_verified_account = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_closed = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_voting_periods", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    permission_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_role_permissions_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    actor_user_id = table.Column<int>(type: "integer", nullable: true),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    justification = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    metadata = table.Column<string>(type: "text", nullable: true),
                    ip_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_audit_logs_users_actor_user_id",
                        column: x => x.actor_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "conversation_participants",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    conversation_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    is_muted = table.Column<bool>(type: "boolean", nullable: false),
                    last_read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conversation_participants", x => x.id);
                    table.ForeignKey(
                        name: "fk_conversation_participants_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_conversation_participants_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "creator_profiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    alias = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    headline = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    short_bio = table.Column<string>(type: "text", nullable: true),
                    district = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    languages = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    skills_offered = table.Column<string>(type: "text", nullable: true),
                    help_needed = table.Column<string>(type: "text", nullable: true),
                    areas_of_interest = table.Column<string>(type: "text", nullable: true),
                    links = table.Column<string>(type: "text", nullable: true),
                    availability = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_creator_profiles", x => x.id);
                    table.ForeignKey(
                        name: "fk_creator_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "entrepreneur_profiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    organization = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    industry = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    website = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    coarse_location = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    interest_areas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    expertise = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    preferred_collaboration_types = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    is_approved = table.Column<bool>(type: "boolean", nullable: false),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entrepreneur_profiles", x => x.id);
                    table.ForeignKey(
                        name: "fk_entrepreneur_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "guardian_links",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guardian_user_id = table.Column<int>(type: "integer", nullable: false),
                    minor_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    can_block_inquiries = table.Column<bool>(type: "boolean", nullable: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_guardian_links", x => x.id);
                    table.ForeignKey(
                        name: "fk_guardian_links_users_guardian_user_id",
                        column: x => x.guardian_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_guardian_links_users_minor_user_id",
                        column: x => x.minor_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ideas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    short_summary = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    problem_statement = table.Column<string>(type: "text", nullable: true),
                    proposed_solution = table.Column<string>(type: "text", nullable: true),
                    target_audience = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    expected_impact = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    stage = table.Column<int>(type: "integer", nullable: false),
                    attribution = table.Column<int>(type: "integer", nullable: false),
                    creator_user_id = table.Column<int>(type: "integer", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    support_sought = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    skills_needed = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estimated_budget = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    expected_timeline = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    success_metrics = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    collaboration_preferences = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    language_tag = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    views_count = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ideas", x => x.id);
                    table.ForeignKey(
                        name: "fk_ideas_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ideas_users_creator_user_id",
                        column: x => x.creator_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "marketplace_listings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    owner_user_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    condition = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    listing_type = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    coarse_location = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    moderation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_marketplace_listings", x => x.id);
                    table.ForeignKey(
                        name: "fk_marketplace_listings_users_owner_user_id",
                        column: x => x.owner_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "member_profiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    display_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    biography = table.Column<string>(type: "text", nullable: true),
                    skills = table.Column<string>(type: "text", nullable: true),
                    interests = table.Column<string>(type: "text", nullable: true),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    preferred_language = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_member_profiles", x => x.id);
                    table.ForeignKey(
                        name: "fk_member_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    conversation_id = table.Column<int>(type: "integer", nullable: false),
                    sender_user_id = table.Column<int>(type: "integer", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_messages_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_messages_users_sender_user_id",
                        column: x => x.sender_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notification_preferences",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    channel = table.Column<int>(type: "integer", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_preferences", x => x.id);
                    table.ForeignKey(
                        name: "fk_notification_preferences_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    link_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_essential = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organization_partner_members",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organization_partner_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_partner_members", x => x.id);
                    table.ForeignKey(
                        name: "fk_organization_partner_members_organization_partners_organiza",
                        column: x => x.organization_partner_id,
                        principalTable: "organization_partners",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organization_partner_members_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "problem_posts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    author_user_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    who_affected = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    coarse_location = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    why_it_matters = table.Column<string>(type: "text", nullable: false),
                    urgency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    attribution = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_problem_posts", x => x.id);
                    table.ForeignKey(
                        name: "fk_problem_posts_users_author_user_id",
                        column: x => x.author_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    replaced_by_token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    target_type = table.Column<int>(type: "integer", nullable: false),
                    target_id = table.Column<int>(type: "integer", nullable: false),
                    reporter_user_id = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    details = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    handled_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    resolution = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reports", x => x.id);
                    table.ForeignKey(
                        name: "fk_reports_users_handled_by_user_id",
                        column: x => x.handled_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reports_users_reporter_user_id",
                        column: x => x.reporter_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_blocks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    blocker_user_id = table.Column<int>(type: "integer", nullable: false),
                    blocked_user_id = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_blocks", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_blocks_users_blocked_user_id",
                        column: x => x.blocked_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_blocks_users_blocker_user_id",
                        column: x => x.blocker_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    granted_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    granted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "profile_field_visibilities",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    creator_profile_id = table.Column<int>(type: "integer", nullable: false),
                    field_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    visibility = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profile_field_visibilities", x => x.id);
                    table.ForeignKey(
                        name: "fk_profile_field_visibilities_creator_profiles_creator_profile",
                        column: x => x.creator_profile_id,
                        principalTable: "creator_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entrepreneur_inquiries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    idea_id = table.Column<int>(type: "integer", nullable: false),
                    seeker_user_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_declined = table.Column<bool>(type: "boolean", nullable: false),
                    is_reported = table.Column<bool>(type: "boolean", nullable: false),
                    is_answered = table.Column<bool>(type: "boolean", nullable: false),
                    answered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    conversation_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entrepreneur_inquiries", x => x.id);
                    table.ForeignKey(
                        name: "fk_entrepreneur_inquiries_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_entrepreneur_inquiries_ideas_idea_id",
                        column: x => x.idea_id,
                        principalTable: "ideas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_entrepreneur_inquiries_users_seeker_user_id",
                        column: x => x.seeker_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "helper_assisted_submissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    helper_user_id = table.Column<int>(type: "integer", nullable: false),
                    idea_id = table.Column<int>(type: "integer", nullable: false),
                    creator_user_id = table.Column<int>(type: "integer", nullable: false),
                    consent_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    creator_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    creator_confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_helper_assisted_submissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_helper_assisted_submissions_ideas_idea_id",
                        column: x => x.idea_id,
                        principalTable: "ideas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_helper_assisted_submissions_users_creator_user_id",
                        column: x => x.creator_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_helper_assisted_submissions_users_helper_user_id",
                        column: x => x.helper_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "idea_attachments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    idea_id = table.Column<int>(type: "integer", nullable: false),
                    file_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    content_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    storage_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_idea_attachments", x => x.id);
                    table.ForeignKey(
                        name: "fk_idea_attachments_ideas_idea_id",
                        column: x => x.idea_id,
                        principalTable: "ideas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "idea_comments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    idea_id = table.Column<int>(type: "integer", nullable: false),
                    author_user_id = table.Column<int>(type: "integer", nullable: false),
                    parent_comment_id = table.Column<int>(type: "integer", nullable: true),
                    body = table.Column<string>(type: "text", nullable: false),
                    moderation_status = table.Column<int>(type: "integer", nullable: false),
                    moderation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_idea_comments", x => x.id);
                    table.ForeignKey(
                        name: "fk_idea_comments_idea_comments_parent_comment_id",
                        column: x => x.parent_comment_id,
                        principalTable: "idea_comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_idea_comments_ideas_idea_id",
                        column: x => x.idea_id,
                        principalTable: "ideas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "idea_follows",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    idea_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_idea_follows", x => x.id);
                    table.ForeignKey(
                        name: "fk_idea_follows_ideas_idea_id",
                        column: x => x.idea_id,
                        principalTable: "ideas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "idea_reviews",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    idea_id = table.Column<int>(type: "integer", nullable: false),
                    reviewer_user_id = table.Column<int>(type: "integer", nullable: true),
                    guest_visitor_id = table.Column<int>(type: "integer", nullable: true),
                    body = table.Column<string>(type: "text", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: true),
                    moderation_status = table.Column<int>(type: "integer", nullable: false),
                    moderation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_guest = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_idea_reviews", x => x.id);
                    table.ForeignKey(
                        name: "fk_idea_reviews_guest_visitors_guest_visitor_id",
                        column: x => x.guest_visitor_id,
                        principalTable: "guest_visitors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_idea_reviews_ideas_idea_id",
                        column: x => x.idea_id,
                        principalTable: "ideas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "idea_revisions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    idea_id = table.Column<int>(type: "integer", nullable: false),
                    revision_number = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    short_summary = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    problem_statement = table.Column<string>(type: "text", nullable: true),
                    proposed_solution = table.Column<string>(type: "text", nullable: true),
                    edited_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    change_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_idea_revisions", x => x.id);
                    table.ForeignKey(
                        name: "fk_idea_revisions_ideas_idea_id",
                        column: x => x.idea_id,
                        principalTable: "ideas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "idea_screening_decisions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    idea_id = table.Column<int>(type: "integer", nullable: false),
                    reviewer_user_id = table.Column<int>(type: "integer", nullable: false),
                    decision = table.Column<int>(type: "integer", nullable: false),
                    originality_score = table.Column<int>(type: "integer", nullable: true),
                    feasibility_score = table.Column<int>(type: "integer", nullable: true),
                    relevance_score = table.Column<int>(type: "integer", nullable: true),
                    impact_score = table.Column<int>(type: "integer", nullable: true),
                    feedback = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_idea_screening_decisions", x => x.id);
                    table.ForeignKey(
                        name: "fk_idea_screening_decisions_ideas_idea_id",
                        column: x => x.idea_id,
                        principalTable: "ideas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "idea_status_histories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    idea_id = table.Column<int>(type: "integer", nullable: false),
                    from_status = table.Column<int>(type: "integer", nullable: false),
                    to_status = table.Column<int>(type: "integer", nullable: false),
                    actor_user_id = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_idea_status_histories", x => x.id);
                    table.ForeignKey(
                        name: "fk_idea_status_histories_ideas_idea_id",
                        column: x => x.idea_id,
                        principalTable: "ideas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "idea_tags",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    idea_id = table.Column<int>(type: "integer", nullable: false),
                    tag_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_idea_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_idea_tags_ideas_idea_id",
                        column: x => x.idea_id,
                        principalTable: "ideas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_idea_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "idea_votes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    idea_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    voting_period_id = table.Column<int>(type: "integer", nullable: false),
                    is_counted = table.Column<bool>(type: "boolean", nullable: false),
                    exclusion_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_idea_votes", x => x.id);
                    table.ForeignKey(
                        name: "fk_idea_votes_ideas_idea_id",
                        column: x => x.idea_id,
                        principalTable: "ideas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_idea_votes_voting_periods_voting_period_id",
                        column: x => x.voting_period_id,
                        principalTable: "voting_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "opportunity_outcomes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    idea_id = table.Column<int>(type: "integer", nullable: false),
                    reporter_user_id = table.Column<int>(type: "integer", nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    verified_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_opportunity_outcomes", x => x.id);
                    table.ForeignKey(
                        name: "fk_opportunity_outcomes_ideas_idea_id",
                        column: x => x.idea_id,
                        principalTable: "ideas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_opportunity_outcomes_users_reporter_user_id",
                        column: x => x.reporter_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_opportunity_outcomes_users_verified_by_user_id",
                        column: x => x.verified_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    idea_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    objectives = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    lead_user_id = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_projects", x => x.id);
                    table.ForeignKey(
                        name: "fk_projects_ideas_idea_id",
                        column: x => x.idea_id,
                        principalTable: "ideas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_projects_users_lead_user_id",
                        column: x => x.lead_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "watchlist_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    idea_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_watchlist_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_watchlist_items_ideas_idea_id",
                        column: x => x.idea_id,
                        principalTable: "ideas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_watchlist_items_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reservations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    marketplace_listing_id = table.Column<int>(type: "integer", nullable: false),
                    requester_user_id = table.Column<int>(type: "integer", nullable: false),
                    message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reservations", x => x.id);
                    table.ForeignKey(
                        name: "fk_reservations_marketplace_listings_marketplace_listing_id",
                        column: x => x.marketplace_listing_id,
                        principalTable: "marketplace_listings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_reservations_users_requester_user_id",
                        column: x => x.requester_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "message_attachments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    message_id = table.Column<int>(type: "integer", nullable: false),
                    file_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    content_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    storage_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_message_attachments", x => x.id);
                    table.ForeignKey(
                        name: "fk_message_attachments_messages_message_id",
                        column: x => x.message_id,
                        principalTable: "messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "idea_problem_links",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    idea_id = table.Column<int>(type: "integer", nullable: false),
                    problem_post_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_idea_problem_links", x => x.id);
                    table.ForeignKey(
                        name: "fk_idea_problem_links_ideas_idea_id",
                        column: x => x.idea_id,
                        principalTable: "ideas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_idea_problem_links_problem_posts_problem_post_id",
                        column: x => x.problem_post_id,
                        principalTable: "problem_posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "problem_endorsements",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    problem_post_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_problem_endorsements", x => x.id);
                    table.ForeignKey(
                        name: "fk_problem_endorsements_problem_posts_problem_post_id",
                        column: x => x.problem_post_id,
                        principalTable: "problem_posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_problem_endorsements_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "project_members",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_members", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_members_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_project_members_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "project_milestones",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    due_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_milestones", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_milestones_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_tasks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    assignee_user_id = table.Column<int>(type: "integer", nullable: true),
                    due_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_tasks", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_tasks_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_project_tasks_users_assignee_user_id",
                        column: x => x.assignee_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "project_updates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<int>(type: "integer", nullable: false),
                    author_user_id = table.Column<int>(type: "integer", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    evidence_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_updates", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_updates_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_project_updates_users_author_user_id",
                        column: x => x.author_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_actor_user_id",
                table: "audit_logs",
                column: "actor_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_created_at",
                table: "audit_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_categories_slug",
                table: "categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_conversation_participants_conversation_id_user_id",
                table: "conversation_participants",
                columns: new[] { "conversation_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_conversation_participants_user_id",
                table: "conversation_participants",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_conversations_last_message_at",
                table: "conversations",
                column: "last_message_at");

            migrationBuilder.CreateIndex(
                name: "ix_creator_profiles_alias",
                table: "creator_profiles",
                column: "alias",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_creator_profiles_user_id",
                table: "creator_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_entrepreneur_inquiries_conversation_id",
                table: "entrepreneur_inquiries",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrepreneur_inquiries_idea_id",
                table: "entrepreneur_inquiries",
                column: "idea_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrepreneur_inquiries_idempotency_key",
                table: "entrepreneur_inquiries",
                column: "idempotency_key");

            migrationBuilder.CreateIndex(
                name: "ix_entrepreneur_inquiries_seeker_user_id",
                table: "entrepreneur_inquiries",
                column: "seeker_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrepreneur_profiles_user_id",
                table: "entrepreneur_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_guardian_links_guardian_user_id_minor_user_id",
                table: "guardian_links",
                columns: new[] { "guardian_user_id", "minor_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_guardian_links_minor_user_id",
                table: "guardian_links",
                column: "minor_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_guest_visitors_guest_id_hash",
                table: "guest_visitors",
                column: "guest_id_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_helper_assisted_submissions_creator_user_id",
                table: "helper_assisted_submissions",
                column: "creator_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_helper_assisted_submissions_helper_user_id",
                table: "helper_assisted_submissions",
                column: "helper_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_helper_assisted_submissions_idea_id",
                table: "helper_assisted_submissions",
                column: "idea_id");

            migrationBuilder.CreateIndex(
                name: "ix_idea_attachments_idea_id",
                table: "idea_attachments",
                column: "idea_id");

            migrationBuilder.CreateIndex(
                name: "ix_idea_comments_idea_id",
                table: "idea_comments",
                column: "idea_id");

            migrationBuilder.CreateIndex(
                name: "ix_idea_comments_parent_comment_id",
                table: "idea_comments",
                column: "parent_comment_id");

            migrationBuilder.CreateIndex(
                name: "ix_idea_follows_idea_id_user_id",
                table: "idea_follows",
                columns: new[] { "idea_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_idea_problem_links_idea_id",
                table: "idea_problem_links",
                column: "idea_id");

            migrationBuilder.CreateIndex(
                name: "ix_idea_problem_links_problem_post_id",
                table: "idea_problem_links",
                column: "problem_post_id");

            migrationBuilder.CreateIndex(
                name: "ix_idea_reviews_guest_visitor_id_idea_id",
                table: "idea_reviews",
                columns: new[] { "guest_visitor_id", "idea_id" },
                unique: true,
                filter: "is_deleted = false AND guest_visitor_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_idea_reviews_idea_id_moderation_status",
                table: "idea_reviews",
                columns: new[] { "idea_id", "moderation_status" });

            migrationBuilder.CreateIndex(
                name: "ix_idea_reviews_reviewer_user_id_idea_id",
                table: "idea_reviews",
                columns: new[] { "reviewer_user_id", "idea_id" },
                unique: true,
                filter: "is_deleted = false AND reviewer_user_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_idea_revisions_idea_id",
                table: "idea_revisions",
                column: "idea_id");

            migrationBuilder.CreateIndex(
                name: "ix_idea_screening_decisions_idea_id",
                table: "idea_screening_decisions",
                column: "idea_id");

            migrationBuilder.CreateIndex(
                name: "ix_idea_status_histories_idea_id",
                table: "idea_status_histories",
                column: "idea_id");

            migrationBuilder.CreateIndex(
                name: "ix_idea_tags_idea_id",
                table: "idea_tags",
                column: "idea_id");

            migrationBuilder.CreateIndex(
                name: "ix_idea_tags_tag_id",
                table: "idea_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_idea_votes_idea_id_is_counted",
                table: "idea_votes",
                columns: new[] { "idea_id", "is_counted" });

            migrationBuilder.CreateIndex(
                name: "ix_idea_votes_user_id_idea_id_voting_period_id",
                table: "idea_votes",
                columns: new[] { "user_id", "idea_id", "voting_period_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_idea_votes_voting_period_id",
                table: "idea_votes",
                column: "voting_period_id");

            migrationBuilder.CreateIndex(
                name: "ix_ideas_category_id",
                table: "ideas",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_ideas_creator_user_id",
                table: "ideas",
                column: "creator_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_ideas_slug",
                table: "ideas",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ideas_status_published_at",
                table: "ideas",
                columns: new[] { "status", "published_at" });

            migrationBuilder.CreateIndex(
                name: "ix_marketplace_listings_owner_user_id",
                table: "marketplace_listings",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_marketplace_listings_status",
                table: "marketplace_listings",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_member_profiles_user_id",
                table: "member_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_message_attachments_message_id",
                table: "message_attachments",
                column: "message_id");

            migrationBuilder.CreateIndex(
                name: "ix_messages_conversation_id_created_at",
                table: "messages",
                columns: new[] { "conversation_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_messages_sender_user_id",
                table: "messages",
                column: "sender_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_preferences_user_id_type_channel",
                table: "notification_preferences",
                columns: new[] { "user_id", "type", "channel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id_is_read",
                table: "notifications",
                columns: new[] { "user_id", "is_read" });

            migrationBuilder.CreateIndex(
                name: "ix_opportunity_outcomes_idea_id",
                table: "opportunity_outcomes",
                column: "idea_id");

            migrationBuilder.CreateIndex(
                name: "ix_opportunity_outcomes_reporter_user_id",
                table: "opportunity_outcomes",
                column: "reporter_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_opportunity_outcomes_verified_by_user_id",
                table: "opportunity_outcomes",
                column: "verified_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_organization_partner_members_organization_partner_id_user_id",
                table: "organization_partner_members",
                columns: new[] { "organization_partner_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organization_partner_members_user_id",
                table: "organization_partner_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_permissions_code",
                table: "permissions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_platform_settings_key",
                table: "platform_settings",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_problem_endorsements_problem_post_id_user_id",
                table: "problem_endorsements",
                columns: new[] { "problem_post_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_problem_endorsements_user_id",
                table: "problem_endorsements",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_problem_posts_author_user_id",
                table: "problem_posts",
                column: "author_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_profile_field_visibilities_creator_profile_id_field_name",
                table: "profile_field_visibilities",
                columns: new[] { "creator_profile_id", "field_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_members_project_id_user_id",
                table: "project_members",
                columns: new[] { "project_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_members_user_id",
                table: "project_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_milestones_project_id",
                table: "project_milestones",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_tasks_assignee_user_id",
                table: "project_tasks",
                column: "assignee_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_tasks_project_id",
                table: "project_tasks",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_updates_author_user_id",
                table: "project_updates",
                column: "author_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_updates_project_id",
                table: "project_updates",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_idea_id",
                table: "projects",
                column: "idea_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_lead_user_id",
                table: "projects",
                column: "lead_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_reports_handled_by_user_id",
                table: "reports",
                column: "handled_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_reports_reporter_user_id",
                table: "reports",
                column: "reporter_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_reports_status",
                table: "reports",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_reservations_marketplace_listing_id",
                table: "reservations",
                column: "marketplace_listing_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservations_requester_user_id",
                table: "reservations",
                column: "requester_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_id",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_role_id_permission_id",
                table: "role_permissions",
                columns: new[] { "role_id", "permission_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tags_slug",
                table: "tags",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_blocks_blocked_user_id",
                table: "user_blocks",
                column: "blocked_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_blocks_blocker_user_id_blocked_user_id",
                table: "user_blocks",
                columns: new[] { "blocker_user_id", "blocked_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_user_id_role_id",
                table: "user_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true,
                filter: "email IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_users_phone_number",
                table: "users",
                column: "phone_number",
                unique: true,
                filter: "phone_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_watchlist_items_idea_id",
                table: "watchlist_items",
                column: "idea_id");

            migrationBuilder.CreateIndex(
                name: "ix_watchlist_items_user_id_idea_id",
                table: "watchlist_items",
                columns: new[] { "user_id", "idea_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "conversation_participants");

            migrationBuilder.DropTable(
                name: "entrepreneur_inquiries");

            migrationBuilder.DropTable(
                name: "entrepreneur_profiles");

            migrationBuilder.DropTable(
                name: "guardian_links");

            migrationBuilder.DropTable(
                name: "helper_assisted_submissions");

            migrationBuilder.DropTable(
                name: "idea_attachments");

            migrationBuilder.DropTable(
                name: "idea_comments");

            migrationBuilder.DropTable(
                name: "idea_follows");

            migrationBuilder.DropTable(
                name: "idea_problem_links");

            migrationBuilder.DropTable(
                name: "idea_reviews");

            migrationBuilder.DropTable(
                name: "idea_revisions");

            migrationBuilder.DropTable(
                name: "idea_screening_decisions");

            migrationBuilder.DropTable(
                name: "idea_status_histories");

            migrationBuilder.DropTable(
                name: "idea_tags");

            migrationBuilder.DropTable(
                name: "idea_votes");

            migrationBuilder.DropTable(
                name: "member_profiles");

            migrationBuilder.DropTable(
                name: "message_attachments");

            migrationBuilder.DropTable(
                name: "notification_preferences");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "opportunity_outcomes");

            migrationBuilder.DropTable(
                name: "organization_partner_members");

            migrationBuilder.DropTable(
                name: "platform_settings");

            migrationBuilder.DropTable(
                name: "problem_endorsements");

            migrationBuilder.DropTable(
                name: "profile_field_visibilities");

            migrationBuilder.DropTable(
                name: "project_members");

            migrationBuilder.DropTable(
                name: "project_milestones");

            migrationBuilder.DropTable(
                name: "project_tasks");

            migrationBuilder.DropTable(
                name: "project_updates");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "reports");

            migrationBuilder.DropTable(
                name: "reservations");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "user_blocks");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "watchlist_items");

            migrationBuilder.DropTable(
                name: "guest_visitors");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "voting_periods");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "organization_partners");

            migrationBuilder.DropTable(
                name: "problem_posts");

            migrationBuilder.DropTable(
                name: "creator_profiles");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "marketplace_listings");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "conversations");

            migrationBuilder.DropTable(
                name: "ideas");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
