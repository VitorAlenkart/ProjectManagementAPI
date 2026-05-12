using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class StudentProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Teachers_teacherId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "ProjectStudent");

            migrationBuilder.RenameColumn(
                name: "educationalInstitution",
                table: "Students",
                newName: "EducationalInstitution");

            migrationBuilder.RenameColumn(
                name: "teacherId",
                table: "Projects",
                newName: "TeacherId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Projects",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Projects",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "date",
                table: "Projects",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Projects",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_teacherId",
                table: "Projects",
                newName: "IX_Projects_TeacherId");

            migrationBuilder.AddColumn<int>(
                name: "Studentid",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StudentProjects",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProjects", x => new { x.StudentId, x.ProjectId });
                    table.ForeignKey(
                        name: "FK_StudentProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentProjects_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Studentid",
                table: "Projects",
                column: "Studentid");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProjects_ProjectId",
                table: "StudentProjects",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Students_Studentid",
                table: "Projects",
                column: "Studentid",
                principalTable: "Students",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Teachers_TeacherId",
                table: "Projects",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Students_Studentid",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Teachers_TeacherId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "StudentProjects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Studentid",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Studentid",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "EducationalInstitution",
                table: "Students",
                newName: "educationalInstitution");

            migrationBuilder.RenameColumn(
                name: "TeacherId",
                table: "Projects",
                newName: "teacherId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Projects",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Projects",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Projects",
                newName: "date");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Projects",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_TeacherId",
                table: "Projects",
                newName: "IX_Projects_teacherId");

            migrationBuilder.CreateTable(
                name: "ProjectStudent",
                columns: table => new
                {
                    projectsid = table.Column<int>(type: "int", nullable: false),
                    studentsid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectStudent", x => new { x.projectsid, x.studentsid });
                    table.ForeignKey(
                        name: "FK_ProjectStudent_Projects_projectsid",
                        column: x => x.projectsid,
                        principalTable: "Projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectStudent_Students_studentsid",
                        column: x => x.studentsid,
                        principalTable: "Students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectStudent_studentsid",
                table: "ProjectStudent",
                column: "studentsid");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Teachers_teacherId",
                table: "Projects",
                column: "teacherId",
                principalTable: "Teachers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
