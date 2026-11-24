using System.Net;

namespace DomainTests.Authentication;

internal sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    : HttpMessageHandler
{
    public HttpMethod? LastMethod { get; private set; }
    public Uri? LastUri { get; private set; }
    public string? LastBody { get; private set; }
    public int CallCount { get; private set; }
    public List<Uri> Uris { get; } = [];
    public List<string?> Bodies { get; } = [];

    public static FakeHttpMessageHandler ReturnsOk(string content = """{"ok":true,"result":[]}""")
        => new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(content)
        });

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        CallCount++;
        LastMethod = request.Method;
        LastUri = request.RequestUri;
        Uris.Add(request.RequestUri!);
        LastBody = request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken);
        Bodies.Add(LastBody);
        return responder(request);
    }
}
