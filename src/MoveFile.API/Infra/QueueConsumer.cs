using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MoveFile.API.Infra
{
    public class QueueConsumer<T> : IQueueConsumer<T>
    {
        private readonly IModel rabbitMQModel;

        public QueueConsumer(IModel rabbitMqModel)
        {
           this.rabbitMQModel = rabbitMqModel;
        }

        public void Start(string queueName, Action<T> action)
        {
            var consumer = new EventingBasicConsumer(rabbitMQModel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();

                var message = Encoding.UTF8.GetString(body);

                T resultObject = default(T);

                try
                {
                    resultObject = System.Text.Json.JsonSerializer.Deserialize<T>(message);
                }
                catch (Exception e)
                {
                    rabbitMQModel.BasicReject(ea.DeliveryTag, true);
                    throw;
                }

                try
                {
                    Dispatch(resultObject, action);

                    rabbitMQModel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception e)
                {
                    rabbitMQModel.BasicNack(ea.DeliveryTag, false, true);
                    throw;
                }
            };

            rabbitMQModel.BasicConsume(queueName, autoAck: false, consumer);
        }

        protected virtual void Dispatch(T resultObject, Action<T> action) => action(resultObject);
    }
}
