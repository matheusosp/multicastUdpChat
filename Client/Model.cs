namespace ConsoleApp1;

public class Model
{
    public string date { get; set; }
    public string time { get; set; }
    public string username { get; set; }
    public string message { get; set; }
    public Model(string username, string message)
    {
        date = DateTime.Today.ToString("dd-MM-yyyy");
        time = DateTime.Now.ToString("HH:mm:ss");
        this.username = username;
        this.message = message;
    }
}