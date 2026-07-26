using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Pulse.Application.Conversations.DTOs;
using Pulse.Application.Messages.DTOs;
using Pulse.Domain.Enums;

namespace Pulse.IntegrationTests
{
    [Collection(ApiTestCollection.Name)]
    public class MessageReactionsEndpointsTests(ApiWebApplicationFactory factory)
    {
        private static void Authenticate(HttpClient client, string token) =>
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        private async Task<(HttpClient AliceClient, HttpClient BobClient, int ConversationId, int MessageId)> CreateConversationWithMessageAsync()
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

            var sendResponse = await aliceClient.PostAsJsonAsync(
                $"/api/conversations/{conversation!.Id}/messages", new SendMessageRequest("Hello", MessageType.Text));
            var message = await sendResponse.Content.ReadFromJsonAsync<MessageDto>(TestDataHelper.JsonOptions);

            return (aliceClient, bobClient, conversation.Id, message!.Id);
        }

        [Fact]
        public async Task ToggleReaction_AddsThenRemoves()
        {
            var (_, bobClient, conversationId, messageId) = await CreateConversationWithMessageAsync();

            var addResponse = await bobClient.PostAsJsonAsync(
                $"/api/conversations/{conversationId}/messages/{messageId}/reactions", new ToggleReactionRequest("👍"));

            addResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var afterAdd = await addResponse.Content.ReadFromJsonAsync<List<MessageReactionDto>>(TestDataHelper.JsonOptions);
            afterAdd!.Should().ContainSingle(r => r.Emoji == "👍");

            var removeResponse = await bobClient.PostAsJsonAsync(
                $"/api/conversations/{conversationId}/messages/{messageId}/reactions", new ToggleReactionRequest("👍"));

            removeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var afterRemove = await removeResponse.Content.ReadFromJsonAsync<List<MessageReactionDto>>(TestDataHelper.JsonOptions);
            afterRemove.Should().BeEmpty();
        }

        [Fact]
        public async Task ToggleReaction_AsNonParticipant_ReturnsForbidden()
        {
            var (_, _, conversationId, messageId) = await CreateConversationWithMessageAsync();

            var eveClient = factory.CreateClient();
            var eve = await TestDataHelper.RegisterAsync(eveClient);
            Authenticate(eveClient, eve.Token);

            var response = await eveClient.PostAsJsonAsync(
                $"/api/conversations/{conversationId}/messages/{messageId}/reactions", new ToggleReactionRequest("👍"));

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ToggleReaction_OnUnknownMessage_ReturnsNotFound()
        {
            var (aliceClient, _, conversationId, _) = await CreateConversationWithMessageAsync();

            var response = await aliceClient.PostAsJsonAsync(
                $"/api/conversations/{conversationId}/messages/999999/reactions", new ToggleReactionRequest("👍"));

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
