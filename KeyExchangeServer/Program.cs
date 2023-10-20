
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using KeyExchangeServer;
using Newtonsoft.Json;

var port = 12345;
var localIP = GetLocalIpAddress();
var listener = new TcpListener(localIP!, port);
listener.Start();
Console.WriteLine($"Servidor de chaves iniciado na IP {localIP} porta {port}");

while (true)
{
    using var client = listener.AcceptTcpClient();
    using var stream = client.GetStream();

    Console.WriteLine("Cliente conectado: " + ((IPEndPoint)client.Client.RemoteEndPoint!)?.Address);

    var publicKeyBytes = new byte[1024];
    var dataLength = stream.Read(publicKeyBytes, 0, publicKeyBytes.Length);
    var publicKey = Encoding.UTF8.GetString(publicKeyBytes, 0, dataLength);

    using var rsa = new RSACryptoServiceProvider(4096);
    rsa.FromXmlString(publicKey);

    var multicastConfig = new MultiCastConfig();
    var payload = JsonConvert.SerializeObject(multicastConfig);
    var buffer = Encoding.Unicode.GetBytes(payload.ToCharArray());
    var encryptedData = rsa.Encrypt(buffer, false);
    stream.Write(encryptedData, 0, encryptedData.Length);
    Console.WriteLine("Configuração Multicast enviada ao cliente.");
}

IPAddress? GetLocalIpAddress()
{
    using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
    socket.Connect("8.8.8.8", 65530);
    var endPoint = socket.LocalEndPoint as IPEndPoint;
    return endPoint?.Address;
}



