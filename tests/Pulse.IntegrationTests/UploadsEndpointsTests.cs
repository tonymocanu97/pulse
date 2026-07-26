using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Pulse.API.Controllers;
using Pulse.Application.Conversations.DTOs;
using Pulse.Application.Messages.DTOs;
using Pulse.Domain.Enums;

namespace Pulse.IntegrationTests
{
    [Collection(ApiTestCollection.Name)]
    public class UploadsEndpointsTests(ApiWebApplicationFactory factory)
    {
        private static void Authenticate(HttpClient client, string token) =>
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        private static MultipartFormDataContent BuildFileContent(byte[] bytes, string fileName, string? contentType = null)
        {
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(bytes);
            if (contentType is not null)
            {
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            }
            content.Add(fileContent, "file", fileName);
            return content;
        }

        [Fact]
        public async Task Upload_ValidImage_ReturnsUrlAndMetadata()
        {
            var client = factory.CreateClient();
            var user = await TestDataHelper.RegisterAsync(client);
            Authenticate(client, user.Token);

            using var content = BuildFileContent([1, 2, 3, 4], "test.png", "image/png");

            var response = await client.PostAsync("/api/uploads", content);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<UploadResponse>();
            result!.Url.Should().StartWith("/uploads/");
            result.FileName.Should().Be("test.png");
            result.SizeBytes.Should().Be(4);
        }

        [Fact]
        public async Task Upload_DisallowedExtension_ReturnsBadRequest()
        {
            var client = factory.CreateClient();
            var user = await TestDataHelper.RegisterAsync(client);
            Authenticate(client, user.Token);

            using var content = BuildFileContent([1, 2, 3], "malware.exe");

            var response = await client.PostAsync("/api/uploads", content);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task SendImageMessage_AfterUpload_AppearsInHistoryWithAttachment()
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

            using var uploadContent = BuildFileContent([1, 2, 3, 4, 5], "photo.png", "image/png");
            var uploadResponse = await aliceClient.PostAsync("/api/uploads", uploadContent);
            var upload = await uploadResponse.Content.ReadFromJsonAsync<UploadResponse>();

            var sendResponse = await aliceClient.PostAsJsonAsync(
                $"/api/conversations/{conversation!.Id}/messages",
                new SendMessageRequest("", MessageType.Image, upload!.Url, upload.FileName, upload.SizeBytes));

            sendResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var sent = await sendResponse.Content.ReadFromJsonAsync<MessageDto>(TestDataHelper.JsonOptions);
            sent!.AttachmentUrl.Should().Be(upload.Url);

            var history = await bobClient.GetFromJsonAsync<List<MessageDto>>(
                $"/api/conversations/{conversation.Id}/messages", TestDataHelper.JsonOptions);
            history!.Should().ContainSingle(m => m.AttachmentFileName == "photo.png" && m.AttachmentSizeBytes == 5);
        }

        [Fact]
        public async Task SendImageMessage_WithoutAttachment_ReturnsBadRequest()
        {
            var aliceClient = factory.CreateClient();
            var alice = await TestDataHelper.RegisterAsync(aliceClient);
            Authenticate(aliceClient, alice.Token);

            var bobClient = factory.CreateClient();
            var bob = await TestDataHelper.RegisterAsync(bobClient);

            var createResponse = await aliceClient.PostAsJsonAsync(
                "/api/conversations", new CreateConversationRequest([bob.User.Id], null, false));
            var conversation = await createResponse.Content.ReadFromJsonAsync<ConversationDto>(TestDataHelper.JsonOptions);

            var response = await aliceClient.PostAsJsonAsync(
                $"/api/conversations/{conversation!.Id}/messages", new SendMessageRequest("", MessageType.Image));

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
