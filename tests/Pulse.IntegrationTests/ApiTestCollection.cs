namespace Pulse.IntegrationTests
{
    [CollectionDefinition(Name)]
    public class ApiTestCollection : ICollectionFixture<ApiWebApplicationFactory>
    {
        public const string Name = "Api";
    }
}
