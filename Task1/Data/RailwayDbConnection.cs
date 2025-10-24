using Npgsql;

namespace Task1.Data
{
    public static class RailwayDbConnection
    {
        public static string ParseConnectionString(string databaseUrl)
        {
            if (string.IsNullOrEmpty(databaseUrl))
                //return "Host=localhost;Database=myapp;Username=postgres;Password=password";
                return "Host=caboose.proxy.rlwy.net; Port=34370; Database=railway; UserName=postgres; Password=QWSKEfuylBDgPkUKObqwbPcvbIQetHIO;Include Error Detail=true";
            // Railway provides: postgresql://user:password@host:port/database
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':');

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = uri.AbsolutePath.TrimStart('/'),
                SslMode = SslMode.Require,
                TrustServerCertificate = true,
                Pooling = true,
                // Railway specific optimizations
                MaxAutoPrepare = 10,
                AutoPrepareMinUsages = 2
            };

            return builder.ToString();
        }
    }
}
