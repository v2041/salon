namespace WebApiTests.Infrastructure;

public static class WebApiCollection
{
    public const string Name = "WebApi";
}

[CollectionDefinition(WebApiCollection.Name)]
public sealed class WebApiCollectionDefinition : ICollectionFixture<WebApiFixture>
{
}
