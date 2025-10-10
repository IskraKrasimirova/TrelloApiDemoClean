using Microsoft.Extensions.Configuration;

namespace TrelloApiDemo.Tests
{
    public static class TestConfig
    {
        public static IConfigurationRoot LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory()) 
                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            return builder.Build();
        }
    }
}

