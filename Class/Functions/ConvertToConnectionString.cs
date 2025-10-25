using System.Text.RegularExpressions;

namespace task2Backend
{
    public class ConvertToConnectionString
    {
        private readonly string _databaseUrl;

        public ConvertToConnectionString(string databaseUrl)
        {
            _databaseUrl = databaseUrl;
        }
        public string convert()
        {
            var match = Regex.Match(_databaseUrl,
                @"^postgres(ql)?:\/\/(?<user>[^:]+):(?<pass>[^@]+)@(?<host>[^:]+):(?<port>\d+)\/(?<db>.*)$");

            if (!match.Success)
            {
                throw new FormatException("A DATABASE_URL não está no formato 'postgresql://user:pass@host:port/db'.");
            }

            string connectionString = $"Host={match.Groups["host"].Value};" +
                                      $"Port={match.Groups["port"].Value};" +
                                      $"Username={match.Groups["user"].Value};" +
                                      $"Password={match.Groups["pass"].Value};" +
                                      $"Database={match.Groups["db"].Value};" +
                                      "SSL Mode=Require;Trust Server Certificate=true";
            
            return connectionString;
        }
    }
}
