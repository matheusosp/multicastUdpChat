namespace ConsoleApp1;

public class MultiCastConfig
{
    public string IP { get; set; }
    public int Port { get; set; }
    public string SymmetricKey { get; set; }

    public MultiCastConfig(string ip, int port, string symmetricKey)
    {
        IP = ip;
        Port = port;
        SymmetricKey = symmetricKey;
    }
}