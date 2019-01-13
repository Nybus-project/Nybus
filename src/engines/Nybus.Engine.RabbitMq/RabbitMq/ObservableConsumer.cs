using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Nybus.RabbitMq
{
    public class ObservableConsumer : IBasicConsumer, IObservable<BasicDeliverEventArgs>
    {
        private readonly ISubject<BasicDeliverEventArgs> _subject = new BufferSubject<BasicDeliverEventArgs>();
        private readonly ISet<string> _consumerTags = new HashSet<string>();
        private readonly object _consumerTagsLock = new object();

        public ObservableConsumer(IModel channel)
        {
            Model = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public void ConsumeFrom(string queueName)
        {
            if (queueName == null)
            {
                throw new ArgumentNullException(nameof(queueName));
            }

            Model.BasicConsume(queueName, false, this);
        }

        public bool IsRunning => _consumerTags.Count > 0;

        public IDisposable Subscribe(IObserver<BasicDeliverEventArgs> observer)
        {
            return _subject.Subscribe(observer);
        }

        public void HandleBasicCancel(string consumerTag)
        {
            lock (_consumerTagsLock)
            {
                if (_consumerTags.Contains(consumerTag))
                {
                    _consumerTags.Remove(consumerTag);
                }

                ConsumerCancelled?.Invoke(this, new ConsumerEventArgs(consumerTag));
            }
        }

        public void HandleBasicCancelOk(string consumerTag)
        {
            lock (_consumerTagsLock)
            {
                if (_consumerTags.Contains(consumerTag))
                {
                    _consumerTags.Remove(consumerTag);
                }

                ConsumerCancelled?.Invoke(this, new ConsumerEventArgs(consumerTag));
            }
        }

        public void HandleBasicConsumeOk(string consumerTag)
        {
            lock (_consumerTagsLock)
            {
                if (!_consumerTags.Contains(consumerTag))
                {
                    _consumerTags.Add(consumerTag);
                }
            }
        }

        public void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            var message = new BasicDeliverEventArgs(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);

            _subject.OnNext(message);
        }

        public void HandleModelShutdown(object model, ShutdownEventArgs reason) { }

        public IModel Model { get; }

        public event EventHandler<ConsumerEventArgs> ConsumerCancelled;
    }
}