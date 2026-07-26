using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Pulse.Application.Conversations.DTOs;
using Pulse.Application.Messages.DTOs;
using Pulse.Domain.Enums;

namespace Pulse.IntegrationTests
{
    [Collection(ApiTestCollection.Name)]
    public class MessagesEndpointsTests(ApiWebApplicationFactory factory)
    {
        private static void Authenticate(HttpClient client, string token) =>
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        private async Task<(HttpClient AliceClient, HttpClient BobClient, int ConversationId)> CreateDirectConversationAsync()
        {
            var aliceClient = factory.CreateClient();
            var alice = await TestDataHelper.RegisterAsync(aliceClient);
            Authenticate(aliceClient, alice.Token);

            var bobClient = factory.CreateClient();
            var bob = await TestDataHelper.RegisterAsync(bobClient);
            Authenticate(bobClient, bob.Token);

            var createResponse = await aliceClient.PostAsJsonAsync(
                "/api/conversations", new CreateConversationRequest([bob.User.Id], null, false));
            var conversation = await createResponse.Content.ReadFromJsonAsync<ConversationDto>(TestDataHelper.JsonOptions);

            return (aliceClient, bobClient, conversation!.Id);
        }

        [Fact]
        public async Task SendMessage_AsParticipant_PersistsAndAppearsInHistory()
        {
            var (aliceClient, bobClient, conversationId) = await CreateDirectConversationAsync();

            var sendResponse = await aliceClient.PostAsJsonAsync(
                $"/api/conversations/{conversationId}/messages", new SendMessageRequest("Hey Bob!", MessageType.Text));

            sendResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var sent = await sendResponse.Content.ReadFromJsonAsync<MessageDto>(TestDataHelper.JsonOptions);
            sent!.Content.Should().Be("Hey Bob!");

            var history = await bobClient.GetFromJsonAsync<List<MessageDto>>(
                $"/api/conversations/{conversationId}/messages", TestDataHelper.JsonOptions);
            history!.Should().ContainSingle(m => m.Id == sent.Id);
        }

        [Fact]
        public async Task SendMessage_AsNonParticipant_ReturnsForbidden()
        {
            var (_, _, conversationId) = await CreateDirectConversationAsync();

            var eveClient = factory.CreateClient();
            var eve = await TestDataHelper.RegisterAsync(eveClient);
            Authenticate(eveClient, eve.Token);

            var response = await eveClient.PostAsJsonAsync(
                $"/api/conversations/{conversationId}/messages", new SendMessageRequest("Sneaky", MessageType.Text));

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task SendMessage_EmptyContent_ReturnsBadRequest()
        {
            var (aliceClient, _, conversationId) = await CreateDirectConversationAsync();

            var response = await aliceClient.PostAsJsonAsync(
                $"/api/conversations/{conversationId}/messages", new SendMessageRequest("   ", MessageType.Text));

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
