using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PowerLinesDataService.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fixture",
                columns: table => new
                {
                    FixtureId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Division = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    HomeTeam = table.Column<string>(nullable: true),
                    AwayTeam = table.Column<string>(nullable: true),
                    HomeOddsAverage = table.Column<decimal>(nullable: false),
                    DrawOddsAverage = table.Column<decimal>(nullable: false),
                    AwayOddsAverage = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fixture", x => x.FixtureId);
                });

            migrationBuilder.CreateTable(
                name: "Result",
                columns: table => new
                {
                    ResultId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Division = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    HomeTeam = table.Column<string>(nullable: true),
                    AwayTeam = table.Column<string>(nullable: true),
                    FullTimeHomeGoals = table.Column<int>(nullable: false),
                    FullTimeAwayGoals = table.Column<int>(nullable: false),
                    FullTimeResult = table.Column<string>(nullable: true),
                    HalfTimeHomeGoals = table.Column<int>(nullable: false),
                    HalfTimeAwayGoals = table.Column<int>(nullable: false),
                    HalfTimeResult = table.Column<string>(nullable: true),
                    HomeOddsAverage = table.Column<decimal>(nullable: false),
                    DrawOddsAverage = table.Column<decimal>(nullable: false),
                    AwayOddsAverage = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Result", x => x.ResultId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fixture");

            migrationBuilder.DropTable(
                name: "Result");
        }
    }
}
