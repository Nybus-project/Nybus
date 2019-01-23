[![Build status](https://ci.appveyor.com/api/projects/status/x8o0xh40cdf6a67l?svg=true)](https://ci.appveyor.com/project/Kralizek/nybus) [![Coverage Status](https://coveralls.io/repos/github/Nybus-project/Nybus/badge.svg?branch=master)](https://coveralls.io/github/Nybus-project/Nybus?branch=master) [![Nybus](https://img.shields.io/nuget/v/Nybus.svg)](https://www.nuget.org/packages/nybus)

# Nybus

Nybus is an application framework that allows you to build message-based applications.

Since it natively targets .NET Standard 2.0, it can be used to create applications that will run on both .NET Framework and .NET Core.

## Commands and Events

Nybus is based on the idea that messages are not all the same and should not be treated in the same way. Messages come in two kinds: commands and events.

Commands are messages that must be handled only by the first available responder.

```csharp
public class AddItemToCart : ICommand
{
    public Guid CartId { get; set; }

    public string ItemSku { get; set; }

    public double Quantity { get; set; }
}
```

On the other hand, events are messages that must be delivered to all subscribers. If no subscriber is available, the message is lost by default.

```csharp
public class CartUpdated : IEvent
{
    public Guid CartId { get; set; }
}
```

## Example

A typical example of a message-based architecture is a web application that offloads a long-running job to an array of work processes by invoking a command.

```csharp
await bus.InvokeCommandAsync(new AddItemToCart {
    CartId = Guid.NewGuid(),
    ItemSku = "something-cool",
    Quantity = 1.0
});
```

The first available worker process will execute the job and, when completed, will raise an event to notify that the job is complete.

```csharp
public async Task HandleAsync(IDispatcher dispatcher, ICommandContext<AddItemToCart> context)
{
    await DoSomethingCoolAsync(context.Command);

    await dispatcher.RaiseEventAsync(new CartUpdated {
        CartId = context.Command.CartId
    });
}
```

The web application itself might have subscribed to such event and notify its clients via a push notification.

Similarly, another listener could be used to deliver a notification to a smartphone app.

## How to Install

Nybus is available on NuGet.

Currently, the following packages are available:

* [Nybus.Abstractions](https://www.nuget.org/packages/Nybus.Abstractions) contains the base types. It's small by design so that you can include it into your business logic without carrying too many unwanted dependencies.
* [Nybus](https://www.nuget.org/packages/Nybus) contains the core classes of Nybus.
* [Nybus.Engine.RabbitMq](https://www.nuget.org/packages/Nybus.Engine.RabbitMq) contains the engine needed to interact with [RabbitMq](http://www.rabbitmq.com/).

## Future development

In order for the 1.0 to go live, there is a need for small quality-of-life improvements. You can track the status of this release [here](https://github.com/Nybus-project/Nybus/milestone/1).

Once the version 1.0 is live, the future developments will go in the direction of broadening the feature set and the supported engines together with some utilities that will help developers.

### Additional engines

Currently, in the pipeline there are the following engines
* An engine based on AWS managed services such as SQS and SNS fully serverless
* An engine based on ActiveMQ to be used in conjunction with the ActiveMQ managed service offered by Amazon.
* An engine based on MassTransit + RabbitMQ: this will make possible the cooperation between applications written in Nybus v1 and those written in Nybus v0.

### Execution filters

Support for execution filters will be added in Nybus 1.1. The idea is to have the possibility to intercept the handlers' execution so to minimize the risk of cross-cutting concerns to pollute the handlers.

Two concrete examples of execution filters could be the integration with services like AWS CloudWatch and AWS X-Ray to properly push metrics and traces.

### Templates

Once few patterns of usage will be established, it would be convenient for the developers to create a new Nybus application by simply typing `dotnet new nybus` in their console of choice.

## Previous versions

Whilst not having hit version 1.0 yet, Nybus has been around for a while already. Given the huge differences between the two codebases, the previous version of Nybus lives in the [v0 branch](https://github.com/Nybus-project/Nybus/tree/v0).

Also, the packages produced by the new codebase have been named so that there is no risk of collision with the previous major version.

Here are the links to the previous packages
* [Nybus.Interfaces](https://www.nuget.org/packages/Nybus.Interfaces)
* [Nybus.Core](https://www.nuget.org/packages/Nybus.Core)
* [Nybus.Castle.Windsor](https://www.nuget.org/packages/Nybus.Castle.Windsor)
* [Nybus.MassTransit](https://www.nuget.org/packages/Nybus.MassTransit)
* [Nybus.NLog](https://www.nuget.org/packages/Nybus.NLog)
* [Nybus.Rx](https://www.nuget.org/packages/Nybus.Rx)

## Versioning

Nybus follows [Semantic Versioning 2.0.0](http://semver.org/spec/v2.0.0.html) for the public releases (published to the [nuget.org](https://www.nuget.org/)).

## Building

Nybus uses [CAKE](https://cakebuild.net/) as a build engine.
* [Here you can see the build history](https://ci.appveyor.com/project/Kralizek/nybus/history)
* [Here you can see the code coverage history](https://coveralls.io/github/Nybus-project/Nybus)

If you would like to build Nybus locally, just execute the `build.cake` script.

You can do it by using the .NET tool created by CAKE authors and use it to execute the build script.
```powershell
dotnet tool install -g Cake.Tool
dotnet cake
```

Many thanks to [AppVeyor](http://www.appveyor.com/) and [Coveralls](https://coveralls.io/) for their support to the .NET Open Source community.

