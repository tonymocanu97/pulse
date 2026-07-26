using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Pulse.Application.Conversations.DTOs;

namespace Pulse.IntegrationTests
{
    [Collection(ApiTestCollection.Name)]
    public class ConversationsEndpointsTests(ApiWebApplicationFactory factory)
    {
        private static void Authenticate(HttpClient client, string token) =>
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        [Fact]
        public async Task CreateConversation_BetweenTwoUsers_AppearsInBothUsersLists()
        {
            var aliceClient = factory.CreateClient();
            var alice = await TestDataHelper.RegisterAsync(aliceClient);
            Authenticate(aliceClient, alice.Token);

            var bobClient = factory.CreateClient();
            var bob = await TestDataHelper.RegisterAsync(bobClient);
            Authenticate(bobClient, bob.Token);

            var createResponse = await aliceClient.PostAsJsonAsync(
                "/api/conversations", new CreateConversationRequest([bob.User.Id], null, false));

            createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var conversation = await createResponse.Content.ReadFromJsonAsync<ConversationDto>();
            conversation!.Participants.Should().HaveCount(2);

            var aliceList = await aliceClient.GetFromJsonAsync<List<ConversationDto>>("/api/conversations");
            aliceList!.Should().ContainSingle(c => c.Id == conversation.Id);

            var bobList = await bobClient.GetFromJsonAsync<List<ConversationDto>>("/api/conversations");
            bobList!.Should().ContainSingle(c => c.Id == conversation.Id);
        }

        [Fact]
        public async Task CreateConversation_CalledTwice_ReusesExistingDirectConversation()
        {
            var aliceClient = factory.CreateClient();
            var alice = await TestDataHelper.RegisterAsync(aliceClient);
            Authenticate(aliceClient, alice.Token);

            var bobClient = factory.CreateClient();
            var bob = await TestDataHelper.RegisterAsync(bobClient);

            var first = await aliceClient.PostAsJsonAsync(
                "/api/conversations", new CreateConversationRequest([bob.User.Id], null, false));
            var firstConversation = await first.Content.ReadFromJsonAsync<ConversationDto>();

            var second = await aliceClient.PostAsJsonAsync(
                "/api/conversations", new CreateConversationRequest([bob.User.Id], null, false));
            var secondConversation = await second.Content.ReadFromJsonAsync<ConversationDto>();

            secondConversation!.Id.Should().Be(firstConversation!.Id);
        }

        [Fact]
        public async Task GetConversationById_AsNonParticipant_ReturnsForbidden()
        {
            var aliceClient = factory.CreateClient();
            var alice = await TestDataHelper.RegisterAsync(aliceClient);
            Authenticate(aliceClient, alice.Token);

            var bobClient = factory.CreateClient();
            var bob = await TestDataHelper.RegisterAsync(bobClient);

            var eveClient = factory.CreateClient();
            var eve = await TestDataHelper.RegisterAsync(eveClient);
            Authenticate(eveClient, eve.Token);

            var createResponse = await aliceClient.PostAsJsonAsync(
                "/api/conversations", new CreateConversationRequest([bob.User.Id], null, false));
            var conversation = await createResponse.Content.ReadFromJsonAsync<ConversationDto>();

            var response = await eveClient.GetAsync($"/api/conversations/{conversation!.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
