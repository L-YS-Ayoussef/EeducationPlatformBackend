using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveAssignmentsUnderLesson_AddCourseImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Sections_SectionId",
                table: "Assignments");

            migrationBuilder.RenameColumn(
                name: "SectionId",
                table: "Assignments",
                newName: "LessonId");

            migrationBuilder.RenameIndex(
                name: "IX_Assignments_SectionId",
                table: "Assignments",
                newName: "IX_Assignments_LessonId");

            migrationBuilder.AddColumn<string>(
                name: "CourseImage",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Lessons_LessonId",
                table: "Assignments",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Lessons_LessonId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "CourseImage",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "LessonId",
                table: "Assignments",
                newName: "SectionId");

            migrationBuilder.RenameIndex(
                name: "IX_Assignments_LessonId",
                table: "Assignments",
                newName: "IX_Assignments_SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Sections_SectionId",
                table: "Assignments",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
