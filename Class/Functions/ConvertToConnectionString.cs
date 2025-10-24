using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace task2Backend
{
    public class ConvertToConnectionString
    {
        public ConvertToConnectionString(string url)
        {
            Url = url;
        }

        private string Url { get; }

        public string convert()
        {
            var pattern = @"postgres(?:ql)?://(?<user>[^:]+):(?<pass>[^@]+)@(?<host>[^:]+):(?<port>\d+)/(?<db>.+)";
            var match = Regex.Match(Url, pattern);

            if (!match.Success)
                throw new Exception("Formato inválido de DATABASE_URL");

            var user = match.Groups["user"].Value;
            var pass = match.Groups["pass"].Value;
            var host = match.Groups["host"].Value;
            var port = match.Groups["port"].Value;
            var db = match.Groups["db"].Value;

            return $"Host={host};Port={port};Username={user};Password={pass};Database={db};SSL Mode=Require;Trust Server Certificate=true";
        }
    }
}