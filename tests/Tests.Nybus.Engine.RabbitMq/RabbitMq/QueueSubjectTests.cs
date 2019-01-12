using System;
using System.Collections.Generic;
using System.Text;
using AutoFixture;
using Moq;
using NUnit.Framework;
using Nybus.RabbitMq;

namespace Tests.RabbitMq
{
    [TestFixture]
    public class QueueSubjectTests
    {
        [Test, AutoMoqData]
        public void IsRunning_returns_false_when_OnCompleted(QueueSubject<string> sut)
        {
            sut.OnCompleted();

            Assert.That(sut.IsRunning, Is.False);
        }

        [Test, AutoMoqData]
        public void IsRunning_returns_false_when_OnCompleted_with_subscribers(QueueSubject<string> sut, IObserver<string> observer)
        {
            sut.Subscribe(observer);

            sut.OnCompleted();

            Assert.That(sut.IsRunning, Is.False);
        }

        [Test, AutoMoqData]
        public void IsRunning_returns_false_after_error(QueueSubject<string> sut, Exception error)
        {
            sut.OnError(error);

            Assert.That(sut.IsRunning, Is.False);
        }

        [Test, AutoMoqData]
        public void IsRunning_returns_false_after_error_with_subscribers(QueueSubject<string> sut, IObserver<string> observer, Exception error)
        {
            sut.Subscribe(observer);

            sut.OnError(error);

            Assert.That(sut.IsRunning, Is.False);
        }

        [Test, AutoMoqData]
        public void Early_subscriber_is_notified_when_completed(QueueSubject<string> sut, IObserver<string> observer)
        {
            sut.Subscribe(observer);

            sut.OnCompleted();

            observer.IsCompleted();
        }

        [Test, AutoMoqData]
        public void Late_subscriber_is_notified_when_completed(QueueSubject<string> sut, IObserver<string> observer)
        {
            sut.OnCompleted();

            sut.Subscribe(observer);

            observer.IsCompleted();
        }

        [Test, AutoMoqData]
        public void Second_early_subscriber_is_notified_when_completed(QueueSubject<string> sut, IObserver<string> first, IObserver<string> second)
        {
            sut.Subscribe(first);

            sut.Subscribe(second);

            sut.OnCompleted();

            second.IsCompleted();
        }

        [Test, AutoMoqData]
        public void Second_late_subscriber_is_notified_when_completed(QueueSubject<string> sut, IObserver<string> first, IObserver<string> second)
        {
            sut.OnCompleted();

            sut.Subscribe(first);

            sut.Subscribe(second);

            second.IsCompleted();
        }

        [Test, AutoMoqData]
        public void Second_late_with_early_first_subscriber_is_notified_when_completed(QueueSubject<string> sut, IObserver<string> first, IObserver<string> second)
        {
            sut.Subscribe(first);

            sut.OnCompleted();

            sut.Subscribe(second);

            second.IsCompleted();
        }

        [Test, AutoMoqData]
        public void Early_subscriber_is_notified_when_error(QueueSubject<string> sut, IObserver<string> observer, Exception error)
        {
            sut.Subscribe(observer);

            sut.OnError(error);

            observer.HasError(error);
        }

        [Test, AutoMoqData]
        public void Late_subscriber_is_notified_when_error(QueueSubject<string> sut, IObserver<string> observer, Exception error)
        {
            sut.OnError(error);

            sut.Subscribe(observer);

            observer.HasError(error);
        }

        [Test, AutoMoqData]
        public void Second_early_subscriber_is_notified_when_error(QueueSubject<string> sut, IObserver<string> first, IObserver<string> second, Exception error)
        {
            sut.Subscribe(first);

            sut.Subscribe(second);

            sut.OnError(error);

            second.HasError(error);
        }

        [Test, AutoMoqData]
        public void Second_late_subscriber_is_notified_when_error(QueueSubject<string> sut, IObserver<string> first, IObserver<string> second, Exception error)
        {
            sut.OnError(error);

            sut.Subscribe(first);

            sut.Subscribe(second);

            second.HasError(error);
        }

        [Test, AutoMoqData]
        public void Second_late_with_early_first_subscriber_is_notified_when_error(QueueSubject<string> sut, IObserver<string> first, IObserver<string> second, Exception error)
        {
            sut.Subscribe(first);

            sut.OnError(error);

            sut.Subscribe(second);

            second.HasError(error);
        }

        [Test, AutoMoqData]
        public void Early_subscriber_gets_all_items(QueueSubject<string> sut, IObserver<string> observer, Generator<string> generator)
        {
            sut.Subscribe(observer);

            sut.OnNext(generator);

            sut.OnNext(generator);

            sut.OnNext(generator);

            observer.ReceivedItems(3);
        }

        [Test, AutoMoqData]
        public void Late_subscriber_gets_all_items(QueueSubject<string> sut, IObserver<string> observer, Generator<string> generator)
        {
            sut.OnNext(generator);

            sut.OnNext(generator);

            sut.OnNext(generator);

            sut.Subscribe(observer);

            observer.ReceivedItems(3);
        }

        [Test, AutoMoqData]
        public void Second_early_subscriber_gets_all_items(QueueSubject<string> sut, IObserver<string> first, IObserver<string> second, Generator<string> generator)
        {
            sut.Subscribe(first);

            sut.Subscribe(second);

            sut.OnNext(generator);

            sut.OnNext(generator);

            sut.OnNext(generator);

            second.ReceivedItems(3);
        }

        [Test, AutoMoqData]
        public void Second_late_subscriber_gets_no_item(QueueSubject<string> sut, IObserver<string> first, IObserver<string> second, Generator<string> generator)
        {
            sut.OnNext(generator);

            sut.OnNext(generator);

            sut.OnNext(generator);

            sut.Subscribe(first);

            sut.Subscribe(second);

            second.ReceivedNoItem();
        }

        [Test, AutoMoqData]
        public void Second_late_subscriber_with_early_first_subscriber_gets_no_item(QueueSubject<string> sut, IObserver<string> first, IObserver<string> second, Generator<string> generator)
        {
            sut.Subscribe(first);

            sut.OnNext(generator);

            sut.OnNext(generator);

            sut.OnNext(generator);

            sut.Subscribe(second);

            second.ReceivedNoItem();
        }
    }

    [TestFixture]
    public class QueueSubjectComplexTests
    {
        [Test, AutoMoqData]
        public void Complex_scenario_1(QueueSubject<string> sut, IObserver<string> first, IObserver<string> second, Generator<string> generator)
        {
            sut.OnNext(generator);
            sut.Subscribe(first);
            sut.OnNext(generator);
            sut.OnCompleted();
            sut.Subscribe(second);

            first.ReceivedItems(2);
            first.IsCompleted();
            second.ReceivedNoItem();
            second.IsCompleted();
        }

        [Test, AutoMoqData]
        public void Complex_scenario_2(QueueSubject<string> sut, IObserver<string> first, IObserver<string> second, Generator<string> generator)
        {
            sut.OnNext(generator);
            sut.Subscribe(first);
            sut.OnNext(generator);
            sut.Subscribe(second);
            sut.OnNext(generator);
            sut.OnCompleted();

            first.ReceivedItems(3);
            first.IsCompleted();
            second.ReceivedItems(1);
            second.IsCompleted();
        }
    }
}
