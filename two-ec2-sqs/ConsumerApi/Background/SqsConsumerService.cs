using Amazon.SQS;
using Amazon.SQS.Model;
using System.Collections.Concurrent;

namespace ConsumerApi.Background;

public class SqsConsumerService : BackgroundService
{
    private readonly ILogger<SqsConsumerService> _logger;
    private readonly IAmazonSQS _sqs;
    private readonly IConfiguration _config;

    public static long ConsumedCount = 0;

    public SqsConsumerService(ILogger<SqsConsumerService> logger, IAmazonSQS sqs, IConfiguration config)
    {
        _logger = logger;
        _sqs = sqs;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueUrl = _config["Sqs:QueueUrl"] ?? "";
        if (string.IsNullOrWhiteSpace(queueUrl))
        {
            _logger.LogError("Sqs:QueueUrl is not configured. Consumer will not start.");
            return;
        }

        var maxMessages = int.TryParse(_config["Sqs:MaxMessages"], out var mm) ? mm : 5;
        var waitTime = int.TryParse(_config["Sqs:WaitTimeSeconds"], out var w) ? w : 10;
        var visibilityTimeout = int.TryParse(_config["Sqs:VisibilityTimeoutSeconds"], out var vt) ? vt : 30;

        _logger.LogInformation("Starting SQS consumer. QueueUrl={QueueUrl}", queueUrl);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var receive = new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = Math.Clamp(maxMessages, 1, 10),
                    WaitTimeSeconds = Math.Clamp(waitTime, 0, 20),
                    VisibilityTimeout = visibilityTimeout,
                    MessageAttributeNames = new List<string> { "All" }
                };

                var resp = await _sqs.ReceiveMessageAsync(receive, stoppingToken);

                if (resp.Messages.Count == 0)
                    continue;

                foreach (var msg in resp.Messages)
                {
                    // "Process" message (here we just log it)
                    _logger.LogInformation("Consumed message {MessageId}: {Body}", msg.MessageId, msg.Body);

                    // delete after processing
                    await _sqs.DeleteMessageAsync(queueUrl, msg.ReceiptHandle, stoppingToken);

                    Interlocked.Increment(ref ConsumedCount);
                }
            }
            catch (OperationCanceledException)
            {
                // shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while consuming messages. Retrying...");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }

        _logger.LogInformation("SQS consumer stopped.");
    }
}
