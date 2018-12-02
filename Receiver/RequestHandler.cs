using Common;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace ReceiverWorker
{
    public class RequestHandler : IConsumer<Request>
    {
        public async Task Consume(ConsumeContext<Request> context)
        {
            Console.WriteLine($"Message is : {context.Message.TestMessage}");

            await Task.CompletedTask;
        }
    }
}
