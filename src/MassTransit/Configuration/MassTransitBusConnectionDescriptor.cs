using System;

namespace Nybus.Configuration
{
    public class MassTransitBusConnectionDescriptor
    {
        public MassTransitBusConnectionDescriptor(Uri host, string userName, string password)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }
            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName));
            }
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            Host = host;
            UserName = userName;
            Password = password;
        }

        public Uri Host { get; private set; }

        public string UserName { get; private set; }

        public string Password { get; private set; }
    }
}