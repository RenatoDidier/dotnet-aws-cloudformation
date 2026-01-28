using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;

var builder = WebApplication.CreateBuilder(args);

// Config
var region = builder.Configuration["AWS:Region"] ?? "us-east-1";
var queueUrl = builder.Configuration["Sqs:QueueUrl"] ?? "";

// AWS SQS client
builder.Services.AddSingleton<IAmazonSQS>(_ =>
{
    var regionEndpoint = RegionEndpoint.GetBySystemName(region);
    return new AmazonSQSClient(regionEndpoint);
});

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Simple endpoint to publish messages
app.MapPost("/publish", async (PublishRequest payload, IAmazonSQS sqs) =>
{
    if (string.IsNullOrWhiteSpace(queueUrl))
        return Results.Problem("Sqs:QueueUrl is not configured.");

    if (string.IsNullOrWhiteSpace(payload.Message))
        return Results.BadRequest(new { error = "Message is required." });

    var send = new SendMessageRequest
    {
        QueueUrl = queueUrl,
        MessageBody = payload.Message,
        MessageAttributes = new Dictionary<string, MessageAttributeValue>
        {
            ["source"] = new MessageAttributeValue { DataType = "String", StringValue = "publisher-api" }
        }
    };

    var resp = await sqs.SendMessageAsync(send);

    return Results.Ok(new
    {
        messageId = resp.MessageId,
        md5 = resp.MD5OfMessageBody
    });
});

app.Run();

public record PublishRequest(string Message);
