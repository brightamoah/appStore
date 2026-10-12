using System.Text.Json;
using System.Text.Json.Serialization;
using appStore.Server.Dto.Boards;
using appStore.Server.Dto.Tasks;
using appStore.Server.Models;
using Xunit;

namespace appStore.Server.Tests
{
    public class JsonEnumSerializationTests
    {
        private readonly JsonSerializerOptions jsonSerializerOptions;

        public JsonEnumSerializationTests()
        {
            jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        [Fact]
        public void Deserialize_CreateTaskDto_With_String_IssueType_Succeeds()
        {
            // Arrange - Exact payload received from client
            var jsonPayload = """
            {
                "columnId": "e01115f5-ee6e-4677-9750-48d3f67e5e2d",
                "description": "Create App Shell",
                "dueDate": "2026-10-13T00:00:00.000Z",
                "estimatedMinutes": 40,
                "issueType": "Task",
                "name": "Create App Shell",
                "priorityId": 2,
                "projectId": "3c73db2b-95d8-4ceb-82c1-6ac99daf0ba6"
            }
            """;

            // Act
            var createdTaskDto = JsonSerializer.Deserialize<CreateTaskDto>(jsonPayload, jsonSerializerOptions);

            // Assert
            Assert.NotNull(createdTaskDto);
            Assert.Equal("Create App Shell", createdTaskDto.Name);
            Assert.Equal(IssueType.Task, createdTaskDto.IssueType);
            Assert.Equal(2, createdTaskDto.PriorityId);
            Assert.Equal(Guid.Parse("3c73db2b-95d8-4ceb-82c1-6ac99daf0ba6"), createdTaskDto.ProjectId);
            Assert.Equal(Guid.Parse("e01115f5-ee6e-4677-9750-48d3f67e5e2d"), createdTaskDto.ColumnId);
            Assert.Equal(40, createdTaskDto.EstimatedMinutes);
        }

        [Theory]
        [InlineData("Task", IssueType.Task)]
        [InlineData("Bug", IssueType.Bug)]
        [InlineData("Story", IssueType.Story)]
        [InlineData("Epic", IssueType.Epic)]
        [InlineData("task", IssueType.Task)]
        [InlineData("bug", IssueType.Bug)]
        public void Deserialize_All_IssueTypes_Succeeds(string issueTypeName, IssueType expectedIssueType)
        {
            // Arrange
            var jsonPayload = $$"""
            {
                "name": "Sample Task",
                "priorityId": 1,
                "projectId": "3c73db2b-95d8-4ceb-82c1-6ac99daf0ba6",
                "issueType": "{{issueTypeName}}"
            }
            """;

            // Act
            var createdTaskDto = JsonSerializer.Deserialize<CreateTaskDto>(jsonPayload, jsonSerializerOptions);

            // Assert
            Assert.NotNull(createdTaskDto);
            Assert.Equal(expectedIssueType, createdTaskDto.IssueType);
        }

        [Fact]
        public void Deserialize_StatusTransitionDto_With_String_Status_Succeeds()
        {
            // Arrange
            var jsonPayload = """
            {
                "targetStatus": "InProgress"
            }
            """;

            // Act
            var transitionDto = JsonSerializer.Deserialize<StatusTransitionDto>(jsonPayload, jsonSerializerOptions);

            // Assert
            Assert.NotNull(transitionDto);
            Assert.Equal(Status.InProgress, transitionDto.TargetStatus);
        }

        [Fact]
        public void Deserialize_CreateColumnDto_With_String_Status_And_Color_Succeeds()
        {
            // Arrange
            var jsonPayload = """
            {
                "name": "Done",
                "statusMapping": "Completed",
                "wipLimit": 5,
                "color": "Success"
            }
            """;

            // Act
            var createColumnDto = JsonSerializer.Deserialize<CreateColumnDto>(jsonPayload, jsonSerializerOptions);

            // Assert
            Assert.NotNull(createColumnDto);
            Assert.Equal("Done", createColumnDto.Name);
            Assert.Equal(Status.Completed, createColumnDto.StatusMapping);
            Assert.Equal(5, createColumnDto.WipLimit);
            Assert.Equal(NuxtUiColor.Success, createColumnDto.Color);
        }

        [Fact]
        public void Serialize_TaskDto_Serializes_Enums_As_Strings()
        {
            // Arrange
            var taskDto = new TaskDto(
                Id: Guid.NewGuid(),
                TaskKey: "CORE-1",
                Name: "Sample",
                Description: "Description",
                PriorityId: 2,
                Status: Status.InProgress,
                IssueType: IssueType.Story,
                IsCompleted: false,
                ProjectId: Guid.NewGuid(),
                ColumnId: Guid.NewGuid(),
                OrderIndex: 0,
                AssigneeId: null,
                AssigneeIds: [],
                ReporterId: Guid.NewGuid(),
                DueDate: null,
                EstimatedMinutes: null,
                CreatedAt: DateTime.UtcNow,
                UpdatedAt: DateTime.UtcNow,
                CompletedAt: null
            );

            // Act
            var jsonString = JsonSerializer.Serialize(taskDto, jsonSerializerOptions);

            // Assert
            Assert.Contains("\"status\":\"InProgress\"", jsonString);
            Assert.Contains("\"issueType\":\"Story\"", jsonString);
        }
    }
}
