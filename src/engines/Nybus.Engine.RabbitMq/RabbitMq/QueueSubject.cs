using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace Nybus.RabbitMq
{
    public class QueueSubject<T> : ISubject<T>
    {
        private readonly Subject<T> _subject = new Subject<T>();
        private readonly Queue<Action<IObserver<T>>> _actions = new Queue<Action<IObserver<T>>>();

        private bool _isCompleted = false;
        private Exception _error;

        public bool IsRunning => !_isCompleted && _error == null;
        public bool HasObservers => _refCount > 0;

        public void OnCompleted()
        {
            _isCompleted = true;

            if (!HasObservers)
            {
                _actions.Enqueue(o => o.OnCompleted());
            }

            _subject.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _error = error;

            if (!HasObservers)
            {
                _actions.Enqueue(o => o.OnError(error));
            }

            _subject.OnError(error);
        }

        public void OnNext(T value)
        {
            if (IsRunning)
            {
                if (HasObservers)
                {
                    _subject.OnNext(value);
                }
                else
                {
                    _actions.Enqueue(o => o.OnNext(value));
                }
            }
        }

        private int _refCount = 0;

        public IDisposable Subscribe(IObserver<T> observer)
        {
            Interlocked.Increment(ref _refCount);

            return new CompositeDisposable(
                Observable.Create<T>(o => ConsumeActions(o)).Concat(_subject).Subscribe(observer),
                Disposable.Create(() => Interlocked.Decrement(ref _refCount))
            );
        }

        private IDisposable ConsumeActions(IObserver<T> observable)
        {
            while (_actions.Count > 0)
            {
                var action = _actions.Dequeue();
                action(observable);
            }

            observable.OnCompleted();

            return Disposable.Empty;
        }

    }
}