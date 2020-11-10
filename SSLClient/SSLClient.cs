using System;
using System.Net.Sockets;
using System.Text;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace ConsoleClient
{
    class ReadWrite{
        public static string Read(NetworkStream stream){
            StringBuilder builder = new StringBuilder();
            byte[] data = new byte[64];
            int dataLength;
            do{
                dataLength = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.UTF8.GetString(data, 0, dataLength));
            }
            while(stream.DataAvailable);
            string message= builder.ToString();
            return message;
        }
        public static void Write(NetworkStream stream, string message){
            StringBuilder builder = new StringBuilder();
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        public static string Read(TcpClient client, SslStream stream){
            StringBuilder builder = new StringBuilder();
            byte[] buffer = new byte[client.ReceiveBufferSize];
            string message;
            int bytesRead=-1;
            while (bytesRead != 0)
            {
                //Console.WriteLine("Ego sum Roman");
                bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);
                builder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                //message = builder.ToString();
                //Console.WriteLine(message);
                if(builder.ToString().IndexOf("\r\n")!=-1){
                    break;
                }
            }
            message= builder.ToString();
            return message;
        }
        public static void Write(SslStream stream, string message){
            StringBuilder builder = new StringBuilder();
            builder = new StringBuilder();
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }
    }
    class Program
    {
        const int port = 587;
        const string address = "192.168.1.44";
        static void Main(string[] args)
        {
            Console.Write("Введите свое имя:");

            TcpClient client = null;
            try
            {
                client = new TcpClient(address, port);
                var stream_unsecured = client.GetStream();
                //ReadWrite.Write(stream_unsecured, "Hello, this is food review!");

                string message;
                string telegramm;
                telegramm = ReadWrite.Read(stream_unsecured);
                Console.WriteLine(telegramm);
                if(telegramm.StartsWith("220")){
                    message="EHLO LAPTOP STARTTLS\r\n";
                    ReadWrite.Write(stream_unsecured, message);

                    telegramm=ReadWrite.Read(stream_unsecured);
                    if(telegramm.StartsWith("250")){
                        message="STARTTLS\r\n";
                        ReadWrite.Write(stream_unsecured, message);

                        telegramm=ReadWrite.Read(stream_unsecured);
                    }
                }
                SslStream stream = new SslStream(stream_unsecured, false, new RemoteCertificateValidationCallback(CertificateValidationCallback));

                //Authenticate
                stream.AuthenticateAsClient("localhost");
                //string userName = Console.ReadLine();

                        if(telegramm.StartsWith("220")){
                            message="MAIL FROM:<user1@localhost.com>\r\n";
                            ReadWrite.Write(stream, message);

                            telegramm=ReadWrite.Read(client, stream);
                            if(telegramm.StartsWith("250")){
                                message="RCPT TO:<user2@localhost.com>\r\n";
                                ReadWrite.Write(stream, message);

                                telegramm=ReadWrite.Read(client, stream);

                                if(telegramm.StartsWith("250")){
                                    message="DATA\r\n";
                                    ReadWrite.Write(stream, message);

                                    telegramm=ReadWrite.Read(client, stream);
                                    if(telegramm.StartsWith("354")){
                                        message="MIME-Version:1.0\r\nFrom:'Tom'<user1@localhost.com>\r\nTo:user1@localhost.com\r\nDate:"+DateTime.Now+"\r\nSubject:Thema\r\nContent-Type:text/plain;charset=us-ascii\r\nContent-Transfer:Encodng:quoted-printable\r\n\r\n";
                                        ReadWrite.Write(stream, message);

                                        telegramm=ReadWrite.Read(client, stream);
                                        Console.WriteLine(telegramm);

                                        if(telegramm.StartsWith("250")){
                                            message="HelloWorld!HelloWorld!HelloWorld!HelloWorld!HelloWorld!HelloWorl=\r\nHelloWorld!HelloWorld!HelloWorld!HelloWorld!HelloWorld!HelloWorl=\r\nHelloWorld!HelloWorld!HelloWorld!HelloWorld!HelloWorld!HelloWorl=\r\nHelloWorld!HelloWorld!HelloWorld!HelloWorld!HelloWorld!HelloWorl=\r\n.\r\n";
                                            ReadWrite.Write(stream, message);

                                            telegramm=ReadWrite.Read(client,stream);
                                            if(telegramm.StartsWith("250")){
                                                message="QUIT\r\n";
                                                ReadWrite.Write(stream, message);

                                                telegramm=ReadWrite.Read(client,stream);
                                                if(telegramm.StartsWith("221")){
                                                    client.Close();
                                                }

                                            }
                                        }

                                    }
                                }
                            }
                        }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Close();
            }
        }
        static bool CertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors.Equals(SslPolicyErrors.RemoteCertificateChainErrors))
                return true;

            Console.WriteLine(sslPolicyErrors);
            return false;
        }
    }
}
