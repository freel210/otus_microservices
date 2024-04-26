namespace Gateway.ConfigOptions;

public class PostgresOptions
{
    public string? Host { get; set; }
    public int Port { get; set; }
    public string? Database { get; set; }
    public bool Pooling { get; set; }
}
