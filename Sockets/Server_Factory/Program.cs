using System.Net;
using System.Net.Sockets;
using System.Text;

TcpListener lsnr = new TcpListener(IPAddress.Loopback, 2026);
lsnr.Start();

Console.WriteLine("Server läuft auf http://localhost:2026");

while (true)
{
    Socket soc = lsnr.AcceptSocket();
    new Thread(() => HandleRequest(soc)).Start();
}

void HandleRequest(Socket soc)
{
    try
    {
        using (soc)
        using (Stream s = new NetworkStream(soc))
        {
            StreamReader sr = new StreamReader(s);
            string? request = sr.ReadLine();
            if (string.IsNullOrEmpty(request)) return;

            var parts = request.Split(' ');
            if (parts.Length < 2) return;
            
            string rawPath = parts[1].TrimStart('/'); 
            if (string.IsNullOrEmpty(rawPath)) rawPath = "index.html";

            if (rawPath.Contains("..")) 
            {
                ResponseFactory.SendStaticError(s, 403, "Forbidden", "Zugriff verweigert.");
                return;
            }

            string extension = Path.GetExtension(rawPath).ToLower();
            
            ResponseFactory factory = extension switch
            {
                ".jpg" or ".jpeg" => new BinaryResponseFactory("image/jpeg"),
                ".png"            => new BinaryResponseFactory("image/png"),
                ".gif"            => new BinaryResponseFactory("image/gif"),
                ".pdf"            => new BinaryResponseFactory("application/pdf"),
                ".html" or ".htm" => new TextResponseFactory("text/html"),
                ".csv"            => new TextResponseFactory("text/csv"),
                ".txt"            => new TextResponseFactory("text/plain"),
                _                 => new TextResponseFactory("text/plain") 
            };

            factory.SendResponse(s, rawPath);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fehler: {ex.Message}");
    }
}

abstract class ResponseFactory
{
    public abstract string ContentType { get; }
    
    public void SendResponse(Stream s, string filePath)
    {
        if (!File.Exists(filePath))
        {
            SendError(s, 404, "Not Found", "Datei nicht gefunden.");
            return;
        }

        try 
        {
            byte[] data = PrepareData(filePath);
            
            string header = $"HTTP/1.1 200 OK\r\n" +
                            $"Content-Type: {ContentType}\r\n" +
                            $"Content-Length: {data.Length}\r\n" +
                            "Connection: close\r\n\r\n";

            s.Write(Encoding.UTF8.GetBytes(header));
            s.Write(data);
            s.Flush();
        }
        catch
        {
            SendError(s, 500, "Internal Server Error", "Fehler beim Lesen der Datei.");
        }
    }

    protected abstract byte[] PrepareData(string filePath);

    private void SendError(Stream s, int code, string status, string msg)
    {
        byte[] body = Encoding.UTF8.GetBytes($"<html><body><h1>{code} {status}</h1><p>{msg}</p></body></html>");
        string head = $"HTTP/1.1 {code} {status}\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n";
        s.Write(Encoding.UTF8.GetBytes(head));
        s.Write(body);
    }

    public static void SendStaticError(Stream s, int code, string status, string msg)
    {
        byte[] body = Encoding.UTF8.GetBytes($"<h1>{code} {status}</h1><p>{msg}</p>");
        string head = $"HTTP/1.1 {code} {status}\r\nContent-Type: text/html\r\nContent-Length: {body.Length}\r\n\r\n";
        s.Write(Encoding.UTF8.GetBytes(head));
        s.Write(body);
    }
}

class BinaryResponseFactory(string mime) : ResponseFactory
{
    public override string ContentType => mime;
    protected override byte[] PrepareData(string filePath) => File.ReadAllBytes(filePath);
}

class TextResponseFactory(string mime) : ResponseFactory
{
    public override string ContentType => $"{mime}; charset=utf-8";
    protected override byte[] PrepareData(string filePath)
    {
        string textContent = File.ReadAllText(filePath);
        return Encoding.UTF8.GetBytes(textContent);
    }
}