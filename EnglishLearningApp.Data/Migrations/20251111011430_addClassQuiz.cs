using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishLearningApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class addClassQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassAchievements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AchievementType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassAchievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassAchievements_ClassRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassAchievements_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassMemberStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VocabulariesLearned = table.Column<int>(type: "int", nullable: false),
                    QuizzesCompleted = table.Column<int>(type: "int", nullable: false),
                    AverageQuizScore = table.Column<double>(type: "float", nullable: false),
                    TotalPoints = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassMemberStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassMemberStats_ClassRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassMemberStats_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassQuizzes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TimeLimit = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassQuizzes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassQuizzes_ClassRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassQuizzes_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassQuizAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassQuizAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassQuizAttempts_ClassQuizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "ClassQuizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassQuizAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassQuizQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VocabularyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Options = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassQuizQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassQuizQuestions_ClassQuizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "ClassQuizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassQuizQuestions_Vocabularies_VocabularyId",
                        column: x => x.VocabularyId,
                        principalTable: "Vocabularies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassQuizAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassQuizAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassQuizAnswers_ClassQuizAttempts_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "ClassQuizAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassQuizAnswers_ClassQuizQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "ClassQuizQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassAchievements_ClassRoomId",
                table: "ClassAchievements",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassAchievements_UserId",
                table: "ClassAchievements",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassMemberStats_ClassRoomId",
                table: "ClassMemberStats",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassMemberStats_UserId",
                table: "ClassMemberStats",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassQuizAnswers_AttemptId",
                table: "ClassQuizAnswers",
                column: "AttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassQuizAnswers_QuestionId",
                table: "ClassQuizAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassQuizAttempts_QuizId",
                table: "ClassQuizAttempts",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassQuizAttempts_UserId",
                table: "ClassQuizAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassQuizQuestions_QuizId",
                table: "ClassQuizQuestions",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassQuizQuestions_VocabularyId",
                table: "ClassQuizQuestions",
                column: "VocabularyId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassQuizzes_ClassRoomId",
                table: "ClassQuizzes",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassQuizzes_CreatedById",
                table: "ClassQuizzes",
                column: "CreatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassAchievements");

            migrationBuilder.DropTable(
                name: "ClassMemberStats");

            migrationBuilder.DropTable(
                name: "ClassQuizAnswers");

            migrationBuilder.DropTable(
                name: "ClassQuizAttempts");

            migrationBuilder.DropTable(
                name: "ClassQuizQuestions");

            migrationBuilder.DropTable(
                name: "ClassQuizzes");
        }
    }
}
