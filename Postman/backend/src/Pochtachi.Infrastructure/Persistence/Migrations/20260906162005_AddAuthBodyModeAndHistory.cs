using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pochtachi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthBodyModeAndHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestHistories_Users_UserId",
                table: "RequestHistories");

            migrationBuilder.AddColumn<string>(
                name: "BodyMode",
                table: "Requests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "RequestHistories",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<long>(
                name: "DurationMs",
                table: "RequestHistories",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Method",
                table: "RequestHistories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "RequestHistories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "RequestHistories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                table: "RequestHistories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_RequestHistories_WorkspaceId",
                table: "RequestHistories",
                column: "WorkspaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestHistories_Users_UserId",
                table: "RequestHistories",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestHistories_Workspaces_WorkspaceId",
                table: "RequestHistories",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestHistories_Users_UserId",
                table: "RequestHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestHistories_Workspaces_WorkspaceId",
                table: "RequestHistories");

            migrationBuilder.DropIndex(
                name: "IX_RequestHistories_WorkspaceId",
                table: "RequestHistories");

            migrationBuilder.DropColumn(
                name: "BodyMode",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "DurationMs",
                table: "RequestHistories");

            migrationBuilder.DropColumn(
                name: "Method",
                table: "RequestHistories");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "RequestHistories");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "RequestHistories");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "RequestHistories");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "RequestHistories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestHistories_Users_UserId",
                table: "RequestHistories",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
