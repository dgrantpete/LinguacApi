using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinguacApi.Migrations
{
	/// <inheritdoc />
	public partial class AddAnswer : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Answers",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					Text = table.Column<string>(type: "text", nullable: false),
					IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
					QuestionId = table.Column<Guid>(type: "uuid", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Answers", x => x.Id);
					table.ForeignKey(
						name: "FK_Answers_Questions_QuestionId",
						column: x => x.QuestionId,
						principalTable: "Questions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Answers_QuestionId",
				table: "Answers",
				column: "QuestionId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Answers");
		}
	}
}
