using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YSSE.IdeaBox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertRoleNameToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Roles move from an enum (stored as integer) to free-form names. Existing rows are
            // mapped from their numeric enum value to the canonical role name so no data is lost.
            migrationBuilder.Sql(
                """
                ALTER TABLE roles
                    ALTER COLUMN name TYPE character varying(100)
                    USING (CASE name
                        WHEN 1 THEN 'Member'
                        WHEN 2 THEN 'Creator'
                        WHEN 3 THEN 'Entrepreneur'
                        WHEN 4 THEN 'Investor'
                        WHEN 5 THEN 'Mentor'
                        WHEN 6 THEN 'Moderator'
                        WHEN 7 THEN 'ScreeningReviewer'
                        WHEN 8 THEN 'Administrator'
                        WHEN 9 THEN 'IdeaHelper'
                        WHEN 10 THEN 'OrganizationPartner'
                        WHEN 11 THEN 'Guardian'
                        WHEN 12 THEN 'LanguageEditor'
                        ELSE name::text
                    END);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverses the conversion for the built-in roles. Custom roles cannot be represented
            // by the legacy enum and map to 0 (an undefined value).
            migrationBuilder.Sql(
                """
                ALTER TABLE roles
                    ALTER COLUMN name TYPE integer
                    USING (CASE name
                        WHEN 'Member' THEN 1
                        WHEN 'Creator' THEN 2
                        WHEN 'Entrepreneur' THEN 3
                        WHEN 'Investor' THEN 4
                        WHEN 'Mentor' THEN 5
                        WHEN 'Moderator' THEN 6
                        WHEN 'ScreeningReviewer' THEN 7
                        WHEN 'Administrator' THEN 8
                        WHEN 'IdeaHelper' THEN 9
                        WHEN 'OrganizationPartner' THEN 10
                        WHEN 'Guardian' THEN 11
                        WHEN 'LanguageEditor' THEN 12
                        ELSE 0
                    END);
                """);
        }
    }
}
