using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using AutoFixture;
using AutoFixture.AutoMoq;
using Kralizek.Lambda;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Nybus;

namespace Tests
{
    public class TestFunction : NybusFunction<string>
    {
        protected override void ConfigureServices(IServiceCollection services, IExecutionEnvironment executionEnvironment)
        {
            IFixture fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization
            {
                GenerateDelegates = true,
                ConfigureMembers = true
            });

            BusHost = fixture.Create<IBusHost>();
            services.AddSingleton(BusHost);

            Handler = fixture.Create<Kralizek.Lambda.IEventHandler<string>>();
            services.AddSingleton(Handler);
        }

        public IBusHost BusHost { get; private set; }

        public Kralizek.Lambda.IEventHandler<string> Handler { get; private set; }
    }

    public class TestFunctionWithHandler : NybusFunction<string>
    {
        protected override void ConfigureServices(IServiceCollection services, IExecutionEnvironment executionEnvironment)
        {
            IFixture fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization
            {
                GenerateDelegates = true,
                ConfigureMembers = true
            });

            BusHost = fixture.Create<IBusHost>();
            services.AddSingleton(BusHost);
            
            RegisterHandler<TestHandler>(services);
        }

        public IBusHost BusHost { get; private set; }
    }

    public class TestFunctionWithNoHandler : NybusFunction<string>
    {
        protected override void ConfigureServices(IServiceCollection services, IExecutionEnvironment executionEnvironment)
        {
            IFixture fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization
            {
                GenerateDelegates = true,
                ConfigureMembers = true
            });

            BusHost = fixture.Create<IBusHost>();
            services.AddSingleton(BusHost);
        }

        public IBusHost BusHost { get; private set; }
    }

    public class TestHandler : Kralizek.Lambda.IEventHandler<string>
    {
        public Task HandleAsync(string input, ILambdaContext context)
        {
            return InnerHandler.HandleAsync(input, context);
        }

        public static Kralizek.Lambda.IEventHandler<string> InnerHandler { get; } = Mock.Of<Kralizek.Lambda.IEventHandler<string>>();
    }
}