using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YSSE.IdeaBox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdeaFeaturedFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "featured_at",
                table: "ideas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_featured",
                table: "ideas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_ideas_is_featured_status",
                table: "ideas",
                columns: new[] { "is_featured", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_ideas_is_featured_status",
                table: "ideas");

            migrationBuilder.DropColumn(
                name: "featured_at",
                table: "ideas");

            migrationBuilder.DropColumn(
                name: "is_featured",
                table: "ideas");
        }
    }
}
