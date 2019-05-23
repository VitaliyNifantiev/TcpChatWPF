using MessageLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace ChatServer.Models
{
    class MyClient
    {
        public string Msg { get; set; }
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        public string userName { get; set; }
        TcpClient client;
        MyServer server; // объект сервера
        List<ActiveClient> activeClients = new List<ActiveClient>();
        public MyClient(TcpClient tcpClient, MyServer serverObject, List<ActiveClient> actClients)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
            activeClients = actClients;
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();

                // получаем имя пользователя
                MyMessage message = GetMessage();
                if (message==null)
                {
                    return;
                }
                userName = message.Name;
                var t= $"{userName}   вошел в чат";
                // посылаем сообщение о входе в чат всем подключенным пользователям
                server.BroadcastMessage($"\t\t\t{DateTime.Now.ToLocalTime()} {t}", Id,"");
                var newActClient = new ActiveClient() { Id = this.Id, Name = message.Name };
                activeClients.Add(newActClient);
                server.BroadcastMessage("", "","", activeClients);
                Console.WriteLine($"{DateTime.Now.ToLocalTime()} {t}");
                // в бесконечном цикле получаем сообщения от клиента
                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        if (message.ReceiverId!="")
                        {
                            message.Msg = String.Format("Private  {0}: {1}", message.Name, message.Msg);
                            Console.WriteLine($"{DateTime.Now.ToLocalTime()} {message.Msg}");
                            server.BroadcastMessage($"{DateTime.Now.ToLocalTime()} {message.Msg}", this.Id, message.ReceiverId);
                        }
                        else
                        {
                            message.Msg = String.Format("{0}: {1}", message.Name, message.Msg);
                            Console.WriteLine($"{DateTime.Now.ToLocalTime()} {message.Msg}");
                            server.BroadcastMessage($"{DateTime.Now.ToLocalTime()} {message.Msg}", this.Id, "");
                        }
                        
                    }
                    catch(Exception ex)
                    {
                        message.Msg = String.Format("\t\t\t{0}: покинул чат", message.Name);
                        Console.WriteLine(message.Msg);
                        server.BroadcastMessage(message.Msg, this.Id,"");
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // в случае выхода из цикла закрываем ресурсы
                server.RemoveConnection(this.Id);
                Close();
            }
        }

        // чтение входящего сообщения и преобразование в строку
        private MyMessage GetMessage()
        {
            var bf = new BinaryFormatter();
            var t = bf.Deserialize(Stream);
            if (t is MyMessage)
            {
                return (MyMessage)t;
            }
            else
                return null;
        }

        // закрытие подключения
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}
