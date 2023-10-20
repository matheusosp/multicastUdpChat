using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using ConsoleApp1;
using Newtonsoft.Json;

var username = string.Empty;

Console.WriteLine("Nome de usuário: ");
username = Console.ReadLine();


var (multicastIp, multicastPort, symmetricKey) = ExchangeWithServer();
var remoteEndPoint = new IPEndPoint(IPAddress.Parse(multicastIp), multicastPort);
var udpclient = new UdpClient();
JoinMultiCast(udpclient, multicastPort, multicastIp);

var thread1 = new Thread(() =>
{
    while (true)
    {
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write("Digite a mensagem a ser enviada: ");
        var message = Console.ReadLine();
        var model = new Model(username!, message!);
        var payload = JsonConvert.SerializeObject(model);
        var encryptedMessage = AesCriptografia.EncryptMessage(payload, symmetricKey);
        udpclient.Send(encryptedMessage, encryptedMessage.Length, remoteEndPoint);
        Console.SetCursorPosition(55, Console.CursorTop);
    }
});

var thread2 = new Thread(() => {
    var sender = new IPEndPoint(0, 0);
    while (true)
    {
        var data = udpclient.Receive(ref sender);
        var decryptedMessage = AesCriptografia.DecryptMessage(data, symmetricKey);
        var deserialized = JsonConvert.DeserializeObject<Model>(decryptedMessage);
        Console.SetCursorPosition(55, Console.CursorTop);
        Console.WriteLine($"[{deserialized?.date}:{deserialized?.time}] {deserialized?.username}: Mensagem-> {deserialized?.message}");
    }
});

thread1.Start();
thread2.Start();
return;

static (string?, int, byte[]) ExchangeWithServer()
{
    using var client = new TcpClient("192.168.1.21", 12345);
    using var stream = client.GetStream();
    
    var (publicKey, privateKey) = GetPublicKeyAndPrivateKey();
    var publicKeyBytes = Encoding.UTF8.GetBytes(publicKey);
    stream.Write(publicKeyBytes, 0, publicKeyBytes.Length);

    var dataRes = new byte[1024]; 
    var dataLength = stream.Read(dataRes, 0, dataRes.Length);
    var encryptedDataBytes = new byte[dataLength];
    Array.Copy(dataRes, encryptedDataBytes, dataLength);
        
    var decryptedMessage = DecryptDataFromPrivateKey(encryptedDataBytes,privateKey);
    var multiCastConfig = JsonConvert.DeserializeObject<MultiCastConfig>(decryptedMessage);
    var symmetryKey = HexStringToByteArray(multiCastConfig!.SymmetricKey);
    return (multiCastConfig?.IP, multiCastConfig!.Port, symmetryKey);
}

static (string, string) GetPublicKeyAndPrivateKey()
{
    using var rsa = new RSACryptoServiceProvider(4096);
    
    var publicKey = rsa.ToXmlString(false);
    var privateKey = rsa.ToXmlString(true);

    return (publicKey, privateKey);
}

static string DecryptDataFromPrivateKey(byte[] encryptedData, string privateKey)
{
    using var rsa = new RSACryptoServiceProvider(4096);
    rsa.FromXmlString(privateKey);
    
    var decryptedData = rsa.Decrypt(encryptedData, false);
    
    var data = Encoding.Unicode.GetString(decryptedData);
    return data;
}
static byte[] HexStringToByteArray(string hex)
{
    var length = hex.Length;
    var bytes = new byte[length / 2];
    for (var i = 0; i < length; i += 2)
    {
        bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
    }
    return bytes;
}

void JoinMultiCast(UdpClient udpClient, int multicastPort, string multicastIp)
{
    udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
    udpClient.ExclusiveAddressUse = false;
    udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, multicastPort));
    udpClient.Client.MulticastLoopback = true;
    udpClient.MulticastLoopback = true;
    udpClient.JoinMulticastGroup(IPAddress.Parse(multicastIp), IPAddress.Any);
}