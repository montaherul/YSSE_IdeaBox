using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace YSSE.IdeaBox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSpecialRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "guardian_links");

            migrationBuilder.DropTable(
                name: "helper_assisted_submissions");

            migrationBuilder.DropTable(
                name: "organization_partner_members");

            migrationBuilder.DropTable(
                name: "organization_partners");

            // Remove user-role assignments for the deleted roles before the role rows
            // themselves, because fk_user_roles_roles_role_id is ON DELETE RESTRICT.
            // role_permissions rows are removed automatically (fk ... ON DELETE CASCADE).
            migrationBuilder.Sql(
                """
                DELETE FROM user_roles
                WHERE role_id IN (
                    SELECT id FROM roles
                    WHERE name IN ('IdeaHelper', 'OrganizationPartner', 'Guardian', 'LanguageEditor')
                );
                """);

            migrationBuilder.Sql(
                """
                DELETE FROM roles
                WHERE name IN ('IdeaHelper', 'OrganizationPartner', 'Guardian', 'LanguageEditor');
                """);

            // The minor/guardian concept was introduced alongside the Guardian role and
            // is no longer used; drop the vestigial flag.
            migrationBuilder.DropColumn(
                name: "is_minor",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_minor",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "name", "description", "created_at" },
                values: new object[,]
                {
                    { "IdeaHelper", "Community facilitator who can assist idea submission", new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { "OrganizationPartner", "Verified partner organization representative", new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { "Guardian", "Approve and monitor the account of a linked minor", new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { "LanguageEditor", "Maintains Bangla/English translations and localized content", new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                });

            migrationBuilder.CreateTable(
                name: "guardian_links",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    can_block_inquiries = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    guardian_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    minor_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                name: "helper_assisted_submissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    consent_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    creator_confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    creator_user_id = table.Column<int>(type: "integer", nullable: false),
                    helper_user_id = table.Column<int>(type: "integer", nullable: false),
                    idea_id = table.Column<int>(type: "integer", nullable: false),
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
                name: "organization_partners",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_partners", x => x.id);
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
                name: "ix_organization_partner_members_organization_partner_id_user_id",
                table: "organization_partner_members",
                columns: new[] { "organization_partner_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organization_partner_members_user_id",
                table: "organization_partner_members",
                column: "user_id");
        }
    }
}
