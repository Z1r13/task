namespace TR.Connector.Configurations;

/// <summary>
/// Конфигурация подключения к API
/// </summary>
internal class ConnectionConfig
{
    public string Url { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }

    /// <summary>
    /// Парсит строку подключения к API в формате: "url=...;login=...;password=..."
    /// </summary>
    public static ConnectionConfig Parse(string connectionString)
    {
        var configMap = connectionString
            .Split(";")
            .Select(part => part.Split("=", 2))
            .Where(part => part.Length == 2)
            .ToDictionary(part => part[0].Trim().ToLowerInvariant(), part => part[1].Trim());

        var config = new ConnectionConfig
        {
            Url = configMap.GetValueOrDefault("url"),
            Login = configMap.GetValueOrDefault("login"),
            Password = configMap.GetValueOrDefault("password"),
        };

        if (string.IsNullOrEmpty(config.Url))
            throw new ArgumentException("URL is required");
        if (string.IsNullOrEmpty(config.Login))
            throw new ArgumentException("Login is requred");
        if (string.IsNullOrEmpty(config.Password))
            throw new ArgumentException("Password is required");

        return config;
    }
}
