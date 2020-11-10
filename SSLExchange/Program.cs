using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace ConsoleServer
{
    class ReadWrite
    {
        public static string Read(NetworkStream stream)
        {
            StringBuilder builder = new StringBuilder();
            byte[] data = new byte[64];
            int dataLength;
            do
            {
                dataLength = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.UTF8.GetString(data, 0, dataLength));
            }
            while (stream.DataAvailable);
            string message = builder.ToString();
            return message;
        }
        public static void Write(NetworkStream stream, string message)
        {
            StringBuilder builder = new StringBuilder();
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }
        public static string Read(TcpClient client, SslStream stream)
        {
            StringBuilder builder = new StringBuilder();
            byte[] buffer = new byte[client.ReceiveBufferSize];
            int bytesRead = -1;
            while (bytesRead != 0)
            {
                //Console.WriteLine("Ego sum Roman.");
                bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);
                builder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                if (builder.ToString().IndexOf("\r\n") != -1)
                {
                    break;
                }
            }
            string message = builder.ToString();
            return message;
        }
        public static void Write(SslStream stream, string message)
        {
            StringBuilder builder = new StringBuilder();
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }
    }
    public class ClientObject
    {
        public TcpClient client;
        public ClientObject(TcpClient tcpClient)
        {
            client = tcpClient;
        }
        
        public void Process()
        {
            NetworkStream stream_unsecured = null;
            stream_unsecured = client.GetStream();

            string init = "220 --proxy server\r\n";
            ReadWrite.Write(stream_unsecured, init);

            string response = ReadWrite.Read(stream_unsecured);
            if (response.StartsWith("EHLO"))
            {
                response = "250 OK 192.168.1.44 hello STARTTLS\r\n";
                ReadWrite.Write(stream_unsecured, response);
            }
            response = ReadWrite.Read(stream_unsecured);
            if (response.StartsWith("STARTTLS"))
            {
                response = "220 OK\r\n";
                ReadWrite.Write(stream_unsecured, response);
            }
            SslStream stream = new SslStream(stream_unsecured, false);
            var certificate = new X509Certificate2("server.pfx", "password");
            stream.AuthenticateAsServer(certificate,
                                                false,
                                                System.Security.Authentication.SslProtocols.Default,
                                                false);
            
            bool bol = false;
            try
            {
                
                byte[] init_byte = new byte[64]; // буфер для получаемых данных
               
                
                
                while (true)
                {
                    byte[] data = new byte[64];
                    StringBuilder builder = new StringBuilder();
                    builder.Clear();
                    // получаем сообщение
                    int bytes = 0;
                    string message = ReadWrite.Read(client, stream);

                    Console.WriteLine(message);
                    
                    
                    if (message.StartsWith("MAIL FROM"))
                    {
                        message = "250 OK\r\n";
                        ReadWrite.Write(stream, message);
                    }
                    if (message.StartsWith("MAIL FROM"))
                    {
                        message = "250 OK\r\n";
                        ReadWrite.Write(stream, message);
                    }
                    if (message.StartsWith("RCPT TO"))
                    {
                        message = "250 OK\r\n";
                        ReadWrite.Write(stream, message);
                    }
                    if (message.StartsWith("DATA"))
                    {
                        message = "354 Start E-MAIL\r\n";
                        ReadWrite.Write(stream, message);

                        string dataBody = ReadWrite.Read(client, stream);
                        Console.WriteLine(dataBody);
                        message = "250 OK\r\n";
                        ReadWrite.Write(stream, message);

                        dataBody = ReadWrite.Read(client, stream);
                        Console.WriteLine(dataBody);
                        message = "250 OK\r\n";
                        ReadWrite.Write(stream, message);
                    }
                    if (message.StartsWith("QUIT"))
                    {
                        message = "221 END\r\n";
                        ReadWrite.Write(stream, message);
                        client.Close();
                        
                        break;
                    }
                    builder.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
            }
        }
    }
    class Program
    {
        const int port = 587;
        static TcpListener listener;
        static void Main(string[] args)
        {
            try
            {
                listener = new TcpListener(IPAddress.Parse("192.168.1.44"), port);
                listener.Start();
                Console.WriteLine("Ожидание подключений...");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ClientObject clientObject = new ClientObject(client);

                    // создаем новый поток для обслуживания нового клиента
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }
    }
}