using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinguacApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthenticationIter2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Stories_StoryId",
                table: "Questions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Stories",
                table: "Stories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Questions",
                table: "Questions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Answers",
                table: "Answers");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Stories",
                newName: "stories");

            migrationBuilder.RenameTable(
                name: "Questions",
                newName: "questions");

            migrationBuilder.RenameTable(
                name: "Answers",
                newName: "answers");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "users",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "IsAdmin",
                table: "users",
                newName: "is_admin");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "stories",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Level",
                table: "stories",
                newName: "level");

            migrationBuilder.RenameColumn(
                name: "Language",
                table: "stories",
                newName: "language");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "stories",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "stories",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Text",
                table: "questions",
                newName: "text");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "questions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StoryId",
                table: "questions",
                newName: "story_id");

            migrationBuilder.RenameIndex(
                name: "IX_Questions_StoryId",
                table: "questions",
                newName: "ix_questions_story_id");

            migrationBuilder.RenameColumn(
                name: "Text",
                table: "answers",
                newName: "text");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "answers",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "QuestionId",
                table: "answers",
                newName: "question_id");

            migrationBuilder.RenameColumn(
                name: "IsCorrect",
                table: "answers",
                newName: "is_correct");

            migrationBuilder.RenameIndex(
                name: "IX_Answers_QuestionId",
                table: "answers",
                newName: "ix_answers_question_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stories",
                table: "stories",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_questions",
                table: "questions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_answers",
                table: "answers",
                column: "id");

            migrationBuilder.CreateTable(
                name: "pending_email_confirmations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pending_email_confirmations", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_answers_questions_question_id",
                table: "answers",
                column: "question_id",
                principalTable: "questions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_questions_stories_story_id",
                table: "questions",
                column: "story_id",
                principalTable: "stories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_answers_questions_question_id",
                table: "answers");

            migrationBuilder.DropForeignKey(
                name: "fk_questions_stories_story_id",
                table: "questions");

            migrationBuilder.DropTable(
                name: "pending_email_confirmations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_email",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_stories",
                table: "stories");

            migrationBuilder.DropPrimaryKey(
                name: "pk_questions",
                table: "questions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_answers",
                table: "answers");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "stories",
                newName: "Stories");

            migrationBuilder.RenameTable(
                name: "questions",
                newName: "Questions");

            migrationBuilder.RenameTable(
                name: "answers",
                newName: "Answers");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "is_admin",
                table: "Users",
                newName: "IsAdmin");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "Stories",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "level",
                table: "Stories",
                newName: "Level");

            migrationBuilder.RenameColumn(
                name: "language",
                table: "Stories",
                newName: "Language");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "Stories",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Stories",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "text",
                table: "Questions",
                newName: "Text");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Questions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "story_id",
                table: "Questions",
                newName: "StoryId");

            migrationBuilder.RenameIndex(
                name: "ix_questions_story_id",
                table: "Questions",
                newName: "IX_Questions_StoryId");

            migrationBuilder.RenameColumn(
                name: "text",
                table: "Answers",
                newName: "Text");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Answers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "question_id",
                table: "Answers",
                newName: "QuestionId");

            migrationBuilder.RenameColumn(
                name: "is_correct",
                table: "Answers",
                newName: "IsCorrect");

            migrationBuilder.RenameIndex(
                name: "ix_answers_question_id",
                table: "Answers",
                newName: "IX_Answers_QuestionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Stories",
                table: "Stories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Questions",
                table: "Questions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Answers",
                table: "Answers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Stories_StoryId",
                table: "Questions",
                column: "StoryId",
                principalTable: "Stories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
