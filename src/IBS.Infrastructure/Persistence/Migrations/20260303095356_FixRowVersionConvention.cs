using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixRowVersionConvention : Migration
    {
        private static readonly string[] _tables = ["Users", "Tenants", "Roles", "Policies", "Permissions", "Clients"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQL Server does not allow ALTER COLUMN to/from rowversion (timestamp).
            // The column must be dropped and re-added.
            foreach (var table in _tables)
            {
                migrationBuilder.DropColumn(name: "RowVersion", table: table);

                migrationBuilder.AddColumn<byte[]>(
                    name: "RowVersion",
                    table: table,
                    type: "rowversion",
                    rowVersion: true,
                    nullable: false,
                    defaultValue: new byte[0]);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var table in _tables)
            {
                migrationBuilder.DropColumn(name: "RowVersion", table: table);

                migrationBuilder.AddColumn<byte[]>(
                    name: "RowVersion",
                    table: table,
                    type: "varbinary(max)",
                    nullable: false,
                    defaultValue: new byte[0]);
            }
        }
    }
}
