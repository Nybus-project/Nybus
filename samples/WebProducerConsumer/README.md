This sample shows how to use Nybus to invoke commands on a worker service and notify to the visitor when the work is complete.

In this sample a MassTransit binding is used. A RabbitMQ node is used as transport layer.

## Solution

The solution is composed of three projects. It uses the version 0.3.0.61 of Nybus.

### Messages
The project contains the definitions of the messages used in this application.
It references only **Nybus.Interfaces** and contains a command called `ReverseString` and an event called `StringReversed`.

### Consumer
The worker process is implemented as Windows Service based on [TopShelf](http://topshelf-project.com/).
It also uses [Castle Windsor](http://www.castleproject.org/projects/windsor/) as Inversion of Control container.
Finally, it references the packages **Nybus.Interfaces**, **Nybus.Core**, **Nybus.MassTransit** and **Nybus.Castle.Windsor**.

You might want to give a look at the `ReverseStringCommandHandler` to see a concrete example of a Command handler and how you can notify eventual subscribers of the completion of the work.
In `AppInstaller` you can see a typical `IWindsorInstaller` used to configure Nybus.

### WebProducer
The producer is implemented as an ASP.NET MVC web site. When launched, you will be given the possibility to insert the text that you want to be reversed.
The result will be displayed when ready by using a SignalR hub that pushes the notification to the browser.

You might want to give a look at the `StringReversedEventHandler` to see how to interact with a SignalR Hub.

Finally, it references the packages **Nybus.Interfaces**, **Nybus.Core**, **Nybus.MassTransit** and **Nybus.Castle.Windsor**.

## Setup
To be able to run this sample, you need a running node of RabbitMQ. 

Once you have it ready, modify the `App.config` in the Consumer project and the `Web.config` in the WebProducer project.
The two projects are set up to look for a connection string named _ServiceBus_ in their configuration file.
Replace the connection string accordingly to estabilish a connection to your RabbitMQ node.