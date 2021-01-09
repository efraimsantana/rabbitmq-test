using RabbitMQ.Client;

namespace MoveFile.API
{
    public class ApplicationInitializer
    {
        private readonly IModel rabbitMqModel;

        public ApplicationInitializer(IModel rabbitMqModel)
        {
            this.rabbitMqModel = rabbitMqModel;
        }

        public void InitializeRabbitMQ()
        {
            rabbitMqModel.QueueDeclare("s3", true, false, false);
        }
    }
}
