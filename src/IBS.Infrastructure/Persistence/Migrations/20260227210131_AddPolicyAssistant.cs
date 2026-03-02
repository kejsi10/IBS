using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyAssistant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PolicyAssistantConversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LineOfBusiness = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtractedData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyAssistantConversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PolicyAssistantReferenceDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LineOfBusiness = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyAssistantReferenceDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PolicyAssistantMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyAssistantMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyAssistantMessages_PolicyAssistantConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "PolicyAssistantConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PolicyAssistantDocumentChunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChunkIndex = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyAssistantDocumentChunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyAssistantDocumentChunks_PolicyAssistantReferenceDocuments_ReferenceDocumentId",
                        column: x => x.ReferenceDocumentId,
                        principalTable: "PolicyAssistantReferenceDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyAssistantConversations_TenantId",
                table: "PolicyAssistantConversations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyAssistantConversations_TenantId_UserId",
                table: "PolicyAssistantConversations",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyAssistantDocumentChunks_ReferenceDocumentId",
                table: "PolicyAssistantDocumentChunks",
                column: "ReferenceDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyAssistantDocumentChunks_ReferenceDocumentId_ChunkIndex",
                table: "PolicyAssistantDocumentChunks",
                columns: new[] { "ReferenceDocumentId", "ChunkIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyAssistantMessages_ConversationId",
                table: "PolicyAssistantMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyAssistantReferenceDocuments_Category",
                table: "PolicyAssistantReferenceDocuments",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyAssistantReferenceDocuments_LineOfBusiness_State",
                table: "PolicyAssistantReferenceDocuments",
                columns: new[] { "LineOfBusiness", "State" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PolicyAssistantDocumentChunks");

            migrationBuilder.DropTable(
                name: "PolicyAssistantMessages");

            migrationBuilder.DropTable(
                name: "PolicyAssistantReferenceDocuments");

            migrationBuilder.DropTable(
                name: "PolicyAssistantConversations");
        }
    }
}
