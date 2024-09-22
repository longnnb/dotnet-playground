namespace TestOption;

public class MyServiceOptions
{
    public string ConnectionString { get; set; }
    public int Timeout { get; set; } = 30;
    public int IntValue { get; set; } = 50;
    public string StringValue { get; set; }
    public bool BoolValue { get; set; }
}