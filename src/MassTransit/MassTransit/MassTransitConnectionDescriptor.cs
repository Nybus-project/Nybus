using System;
using System.Configuration;
using System.Data.Common;

namespace Nybus.MassTransit
{
    public class MassTransitConnectionDescriptor
    {
        public static MassTransitConnectionDescriptor Parse(string connectionString)
        {
            DbConnectionStringBuilder builder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            var host = new Uri((string) builder["Host"]);
            var username = (string) builder["userName"];
            var password = (string) builder["password"];

            return new MassTransitConnectionDescriptor(host, username, password);
        }

        public static MassTransitConnectionDescriptor FromConfiguration(string nameOfConnectionString)
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings[nameOfConnectionString];

            if (connectionStringSettings == null)
                return null;

            string connectionString = connectionStringSettings.ConnectionString;
            return Parse(connectionString);
        }

        public MassTransitConnectionDescriptor(Uri host, string userName, string password)
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