# Nybus #

Nybus is a service bus that allows you to quickly build loosely coupled applications that react to commands and events.

Commands and events help building a platform where different workers collaborate by executing each a very specific job; each worker can be instantiated several times to properly handle the load and increase the overall throughput of the application.

A Command is a message that must be handled only by the first worker that is available.
An Event is a message that must be delivered to all the running workers that have subscribed it.

## Example ##

A typical example is a web application that offloads a long-running job to an array of worker processes by invoking a command. The first available worker process will execute the job and, when completed, will raise an event to notify that the job is complete. The web application might have subscribed such event and notify the clients with a push notification. 
[See the WebProducerConsumer sample](https://github.com/Nybus-project/Nybus/tree/master/samples/WebProducerConsumer)

## How to Install

Nybus is available through NuGet. 

At the moment there are three packages available:
* [Nybus.Interfaces](https://www.nuget.org/packages/Nybus.Interfaces/) contains the base types. It's very small by design so that you can include it into your business logic projects without carrying too many unwanted dependencies.
* [Nybus.Castle.Windsor](https://www.nuget.org/packages/Nybus.Castle.Windsor/) allows the usage of Castle Windsor as IoC container.
* [Nybus.MassTransit](https://www.nuget.org/packages/Nybus.MassTransit/) is the first real implementation. It's based on [MassTransit 2.10](https://github.com/phatboyg/MassTransit) and [RabbitMQ](http://www.rabbitmq.com/).

## Future development

Beside stabilizing the API, the future plans involves:
* A reusable host process that scans for handlers in the assemblies contained in the same folder.
* Some kind of support for [Rx.NET](https://github.com/Reactive-Extensions/Rx.NET) so that events can be manipulated with LINQ operators.
* A binding based on AWS managed services, namely SQS and SNS.
* A binding based on Azure managed services.
* A binding based on MassTransit 3.0

## Continuous integration [![Build status](https://ci.appveyor.com/api/projects/status/x8o0xh40cdf6a67l?svg=true)](https://ci.appveyor.com/project/Kralizek/nybus)

The project is automatically built whenever a new commit is pushed into this repository.

Many thanks to [AppVeyor](http://www.appveyor.com/) for their support to the .NET Open Source community.

[Here you can see the build history](https://ci.appveyor.com/project/Kralizek/nybus/history)


