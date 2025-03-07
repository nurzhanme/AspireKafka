// using MassTransit;
// using Microsoft.Extensions.Logging;
//
// namespace AspireKafka.Infrastructure.Messaging
// {
//     public class UserCreatedEventConsumer : IConsumer<IConsumer>
//     {
//         ILogger<UserCreatedEventConsumer> _logger;
//
//         public UserCreatedEventConsumer(ILogger<UserCreatedEventConsumer> logger)
//         {
//             _logger = logger;
//         }
//
//         public Task Consume(ConsumeContext<IConsumer> context)
//         {
//             try
//             {
//                 _logger.LogInformation("Message Consumed ${message}", context.Message);
//             }
//             catch (Exception e)
//             {
//                 _logger.LogError(e, "Error consuming message");
//             }
//
//             return Task.CompletedTask;
//         }
//     }
// }
