using Amazon;
using Amazon.SQS;
using ConsumerApi.Background;

var builder = WebApplication.CreateBuilder(args);

var region = builder.Configuration["AWS:Region"] ?? "us-east-1";

builder.Services.AddSingleton<IAmazonSQS>(_ =>
{
    var regionEndpoint = RegionEndpoint.GetBySystemName(region);
    return new AmazonSQSClient(regionEndpoint);
});

builder.Services.AddHostedService<SqsConsumerService>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/metrics", () =>
{
    return Results.Ok(new
    {
        consumedCount = SqsConsumerService.ConsumedCount
    });
});

app.Run();
