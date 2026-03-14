using System.Text.Json;

namespace RukuServiceApi.IntegrationTests;

[TestClass]
public sealed class SchedulesControllerTests
{
    private static readonly HttpClient Client = TestHelpers.GetClient();

    [TestMethod]
    public async Task GetAllSchedules_ShouldReturnList()
    {
        var response = await Client.GetAsync("/api/schedules");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var schedules = JsonDocument.Parse(content);

        Assert.IsNotNull(schedules);
    }

    [TestMethod]
    public async Task CreateSchedule_WithValidData_ShouldReturnCreated()
    {
        var schedule = new
        {
            contactName = "Test User",
            selectedDate = DateTime.Now.AddDays(5),
            services = new[] { "Web Development" },
            timeslots = new[] { "10:00" },
            note = "Test schedule",
            uid = "test-uid-123",
        };

        var content = TestHelpers.CreateJsonContent(schedule);
        var response = await Client.PostAsync("/api/schedules", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Created, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdSchedule = JsonDocument.Parse(responseContent);

        Assert.IsTrue(createdSchedule.RootElement.TryGetProperty("id", out _));
    }

    [TestMethod]
    public async Task GetScheduleById_WithValidId_ShouldReturnSchedule()
    {
        // First create a schedule
        var schedule = new
        {
            contactName = "Get Test User",
            selectedDate = DateTime.Now.AddDays(6),
            services = new[] { "Mobile App Development" },
            timeslots = new[] { "11:00" },
        };

        var createContent = TestHelpers.CreateJsonContent(schedule);
        var createResponse = await Client.PostAsync("/api/schedules", createContent);
        createResponse.EnsureSuccessStatusCode();

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdSchedule = JsonDocument.Parse(createResponseContent);
        var scheduleId = createdSchedule.RootElement.GetProperty("id").GetInt32();

        // Now get it
        var getResponse = await Client.GetAsync($"/api/schedules/{scheduleId}");
        getResponse.EnsureSuccessStatusCode();

        var getContent = await getResponse.Content.ReadAsStringAsync();
        var fetchedSchedule = JsonDocument.Parse(getContent);

        Assert.AreEqual(scheduleId, fetchedSchedule.RootElement.GetProperty("id").GetInt32());
    }

    [TestMethod]
    public async Task UpdateSchedule_WithValidData_ShouldReturnOk()
    {
        // First create a schedule
        var schedule = new
        {
            contactName = "Update Test User",
            selectedDate = DateTime.Now.AddDays(7),
            services = new[] { "Web Development" },
            timeslots = new[] { "14:00" },
        };

        var createContent = TestHelpers.CreateJsonContent(schedule);
        var createResponse = await Client.PostAsync("/api/schedules", createContent);
        createResponse.EnsureSuccessStatusCode();

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdSchedule = JsonDocument.Parse(createResponseContent);
        var scheduleId = createdSchedule.RootElement.GetProperty("id").GetInt32();

        // Now update it
        var updateSchedule = new
        {
            id = scheduleId,
            contactName = "Updated Test User",
            selectedDate = DateTime.Now.AddDays(8),
            services = new[] { "Mobile App Development" },
            timeslots = new[] { "15:00" },
            note = "Updated note",
        };

        var updateContent = TestHelpers.CreateJsonContent(updateSchedule);
        var updateResponse = await Client.PutAsync($"/api/schedules/{scheduleId}", updateContent);

        updateResponse.EnsureSuccessStatusCode();
    }

    [TestMethod]
    public async Task DeleteSchedule_WithValidId_ShouldReturnNoContent()
    {
        // First create a schedule
        var schedule = new
        {
            contactName = "Delete Test User",
            selectedDate = DateTime.Now.AddDays(9),
            services = new[] { "Web Development" },
            timeslots = new[] { "16:00" },
        };

        var createContent = TestHelpers.CreateJsonContent(schedule);
        var createResponse = await Client.PostAsync("/api/schedules", createContent);
        createResponse.EnsureSuccessStatusCode();

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdSchedule = JsonDocument.Parse(createResponseContent);
        var scheduleId = createdSchedule.RootElement.GetProperty("id").GetInt32();

        // Now delete it
        var deleteResponse = await Client.DeleteAsync($"/api/schedules/{scheduleId}");

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
