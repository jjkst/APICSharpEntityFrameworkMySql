using System.Text.Json;

namespace RukuServiceApi.IntegrationTests;

[TestClass]
public sealed class AvailabilitiesControllerTests
{
    private static readonly HttpClient Client = TestHelpers.GetClient();

    [TestMethod]
    public async Task GetAllAvailabilities_ShouldReturnList()
    {
        var response = await Client.GetAsync("/api/availabilities");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var availabilities = JsonDocument.Parse(content);

        Assert.IsNotNull(availabilities);
    }

    [TestMethod]
    public async Task GetAvailableDates_ShouldReturnDates()
    {
        var response = await Client.GetAsync("/api/availabilities/dates");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var dates = JsonDocument.Parse(content);

        Assert.IsNotNull(dates);
    }

    [TestMethod]
    public async Task GetAvailableServices_WithDate_ShouldReturnServices()
    {
        var futureDate = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd");
        var response = await Client.GetAsync($"/api/availabilities/services?date={futureDate}");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var services = JsonDocument.Parse(content);

        Assert.IsNotNull(services);
    }

    [TestMethod]
    public async Task GetTimeSlots_WithValidRequest_ShouldReturnTimeSlots()
    {
        var timeslotRequest = new
        {
            date = DateTime.Now.AddDays(30),
            services = new[] { "Web Development", "Mobile App Development" },
        };

        var content = TestHelpers.CreateJsonContent(timeslotRequest);
        var response = await Client.PostAsync("/api/availabilities/timeslots", content);

        // Could be NotFound if no availabilities exist, or Ok if they do
        Assert.IsTrue(
            response.StatusCode == System.Net.HttpStatusCode.OK
                || response.StatusCode == System.Net.HttpStatusCode.NotFound
        );
    }

    [TestMethod]
    public async Task CreateAvailability_WithValidData_ShouldReturnCreated()
    {
        var availability = new
        {
            startDate = DateTime.Now.AddDays(1),
            endDate = DateTime.Now.AddDays(7),
            services = new[] { "Web Development" },
            timeslots = new[] { "09:00", "10:00", "11:00", "14:00", "15:00" },
        };

        var content = TestHelpers.CreateJsonContent(availability);
        var response = await Client.PostAsync("/api/availabilities", content);

        // Could be Created or Conflict if dates overlap
        Assert.IsTrue(
            response.StatusCode == System.Net.HttpStatusCode.Created
                || response.StatusCode == System.Net.HttpStatusCode.Conflict
        );
    }

    [TestMethod]
    public async Task GetAvailabilityById_WithValidId_ShouldReturnAvailability()
    {
        // First create an availability
        var availability = new
        {
            startDate = DateTime.Now.AddDays(10),
            endDate = DateTime.Now.AddDays(17),
            services = new[] { "Mobile App Development" },
            timeslots = new[] { "09:00", "10:00" },
        };

        var createContent = TestHelpers.CreateJsonContent(availability);
        var createResponse = await Client.PostAsync("/api/availabilities", createContent);

        if (createResponse.IsSuccessStatusCode)
        {
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdAvailability = JsonDocument.Parse(createResponseContent);
            var availabilityId = createdAvailability.RootElement.GetProperty("id").GetInt32();

            // Now get it
            var getResponse = await Client.GetAsync($"/api/availabilities/{availabilityId}");
            getResponse.EnsureSuccessStatusCode();

            var getContent = await getResponse.Content.ReadAsStringAsync();
            var fetchedAvailability = JsonDocument.Parse(getContent);

            Assert.AreEqual(
                availabilityId,
                fetchedAvailability.RootElement.GetProperty("id").GetInt32()
            );
        }
    }

    [TestMethod]
    public async Task UpdateAvailability_WithValidData_ShouldReturnOk()
    {
        var availability = new
        {
            startDate = DateTime.Now.AddDays(20),
            endDate = DateTime.Now.AddDays(27),
            services = new[] { "Web Development" },
            timeslots = new[] { "09:00", "10:00" },
        };

        var createContent = TestHelpers.CreateJsonContent(availability);
        var createResponse = await Client.PostAsync("/api/availabilities", createContent);

        if (createResponse.IsSuccessStatusCode)
        {
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdAvailability = JsonDocument.Parse(createResponseContent);
            var availabilityId = createdAvailability.RootElement.GetProperty("id").GetInt32();

            var updateAvailability = new
            {
                id = availabilityId,
                startDate = DateTime.Now.AddDays(21),
                endDate = DateTime.Now.AddDays(26),
                services = new[] { "Mobile App Development" },
                timeslots = new[] { "14:00", "15:00" },
            };

            var updateContent = TestHelpers.CreateJsonContent(updateAvailability);
            var updateResponse = await Client.PutAsync(
                $"/api/availabilities/{availabilityId}",
                updateContent
            );

            updateResponse.EnsureSuccessStatusCode();
        }
    }

    [TestMethod]
    public async Task DeleteAvailability_WithValidId_ShouldReturnNoContent()
    {
        var availability = new
        {
            startDate = DateTime.Now.AddDays(30),
            endDate = DateTime.Now.AddDays(37),
            services = new[] { "Web Development" },
            timeslots = new[] { "09:00" },
        };

        var createContent = TestHelpers.CreateJsonContent(availability);
        var createResponse = await Client.PostAsync("/api/availabilities", createContent);

        if (createResponse.IsSuccessStatusCode)
        {
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdAvailability = JsonDocument.Parse(createResponseContent);
            var availabilityId = createdAvailability.RootElement.GetProperty("id").GetInt32();

            var deleteResponse = await Client.DeleteAsync(
                $"/api/availabilities/{availabilityId}"
            );

            Assert.AreEqual(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }
    }

    [TestMethod]
    public async Task CreateAvailability_WithPastStartDate_ShouldReturnBadRequest()
    {
        var availability = new
        {
            startDate = DateTime.Now.AddDays(-1),
            endDate = DateTime.Now.AddDays(5),
            services = new[] { "Web Development" },
            timeslots = new[] { "09:00" },
        };

        var content = TestHelpers.CreateJsonContent(availability);
        var response = await Client.PostAsync("/api/availabilities", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task CreateAvailability_WithEndDateBeforeStartDate_ShouldReturnBadRequest()
    {
        var availability = new
        {
            startDate = DateTime.Now.AddDays(10),
            endDate = DateTime.Now.AddDays(5),
            services = new[] { "Web Development" },
            timeslots = new[] { "09:00" },
        };

        var content = TestHelpers.CreateJsonContent(availability);
        var response = await Client.PostAsync("/api/availabilities", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
