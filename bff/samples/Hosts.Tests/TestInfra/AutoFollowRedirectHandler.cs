﻿using Microsoft.Extensions.Logging;

namespace Hosts.Tests.TestInfra;

public class AutoFollowRedirectHandler(ILogger<AutoFollowRedirectHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage result;
        var previousUri = request.RequestUri;
        for (var i = 0; i < 20; i++)
        {
            result = await base.SendAsync(request, cancellationToken);
            if (result.StatusCode == HttpStatusCode.Found && result.Headers.Location != null)
            {
                logger.LogInformation("Redirecting from {0} to {1}", previousUri, result.Headers.Location);

                var newUri = result.Headers.Location;
                if (!newUri.IsAbsoluteUri) newUri = new Uri(previousUri!, newUri);
                var headers = request.Headers;
                request = new HttpRequestMessage(HttpMethod.Get, newUri);
                foreach (var header in headers) request.Headers.Add(header.Key, header.Value);
                previousUri = request.RequestUri;
                continue;
            }

            return result;
        }

        throw new InvalidOperationException("Keeps redirecting forever");
    }
}

public class CloningHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpClient _innerHttpClient;

    public CloningHttpMessageHandler(HttpClient innerHttpClient)
    {
        _innerHttpClient = innerHttpClient ?? throw new ArgumentNullException(nameof(innerHttpClient));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Clone the incoming request
        var clonedRequest = await CloneHttpRequestMessageAsync(request);

        // Send the cloned request using the inner HttpClient
        var response = await _innerHttpClient.SendAsync(clonedRequest, cancellationToken);

        // Clone the response and return it
        return await CloneHttpResponseMessageAsync(response);
    }

    private async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage original)
    {
        var cloned = new HttpRequestMessage(original.Method, original.RequestUri)
        {
            Version = original.Version
        };

        // Copy the content if present
        if (original.Content != null)
        {
            //var memoryStream = new MemoryStream();
            //await original.Content.CopyToAsync(memoryStream);
            //memoryStream.Position = 0;
            //cloned.Content = new StreamContent(memoryStream);
            cloned.Content = new StreamContent(await original.Content.ReadAsStreamAsync());

            // Copy headers from the original content to the cloned content
            foreach (var header in original.Content.Headers)
            {
                cloned.Content.Headers.Add(header.Key, header.Value);
            }
        }

        // Copy headers
        foreach (var header in original.Headers)
        {
            cloned.Headers.Add(header.Key, header.Value);
        }

        return cloned;
    }

    private async Task<HttpResponseMessage> CloneHttpResponseMessageAsync(HttpResponseMessage original)
    {
        var cloned = new HttpResponseMessage(original.StatusCode)
        {
            Version = original.Version,
            ReasonPhrase = original.ReasonPhrase,
            RequestMessage = original.RequestMessage,
        };

        // Copy the content if present
        if (original.Content != null)
        {
            //var memoryStream = new MemoryStream();
            //await original.Content.CopyToAsync(memoryStream);
            //memoryStream.Position = 0;
            //cloned.Content = new StreamContent(memoryStream);
            cloned.Content = new StreamContent(await original.Content.ReadAsStreamAsync());

            // Copy headers from the original content to the cloned content
            foreach (var header in original.Content.Headers)
            {
                cloned.Content.Headers.Add(header.Key, header.Value);
            }
        }

        // Copy headers
        foreach (var header in original.Headers)
        {
            cloned.Headers.Add(header.Key, header.Value);
        }

        return cloned;
    }
}