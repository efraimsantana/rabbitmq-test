using System;

namespace MoveFile.API.Infra
{
    public interface IQueueConsumer<T>
    {
        void Start(string queueName, Action<T> action);
    }
}