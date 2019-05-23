using MessageLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

namespace ChatServer.Models
{
    class MyServer
    {
        static TcpListener tcpListener; // сервер для прослушивания
        List<MyClient> clients = new List<MyClient>(); // все подключения
        List<ActiveClient> activeClients = new List<ActiveClient>();
        protected internal void AddConnection(MyClient clientObject)
        {
            clients.Add(clientObject);
        }
       
        protected internal void RemoveConnection(string id)
        {
            // получаем по id закрытое подключение
            MyClient client = clients.FirstOrDefault(c => c.Id == id);
            activeClients.Remove(activeClients.First(x => x.Id == client.Id));
            if (activeClients.Count!=0)
                Task.Factory.StartNew(()=>BroadcastMessage("", "","", activeClients)) ;
           
            // и удаляем его из списка подключений
            if (client != null)
                clients.Remove(client);
        }
        // прослушивание входящих подключений
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 23333);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    MyClient clientObject = new MyClient(tcpClient, this, activeClients);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        // трансляция сообщения подключенным клиентам
        protected internal void BroadcastMessage(string msg, string id,string receiverId, List<ActiveClient> activeClient=null)
        {
            if (activeClient != null&&msg==""&&id=="")
            {
                var bf1 = new BinaryFormatter();
                for (int i = 0; i < clients.Count; i++)
                        bf1.Serialize(clients[i].Stream, activeClient);//передача данных
            }
            else
            {
                var bf = new BinaryFormatter();
                if (receiverId!="")
                {
                    for (int i = 0; i < clients.Count; i++)
                        if (clients[i].Id == receiverId)
                        {
                            // сравниваем ID получателя
                            bf.Serialize(clients[i].Stream, new MyMessage() { Msg = msg, Id = id });//передача данных
                            break;
                        }
                }
                else
                {
                    for (int i = 0; i < clients.Count; i++)
                        if (clients[i].Id != id) // если id клиента не равно id отправляющего
                            bf.Serialize(clients[i].Stream, new MyMessage() { Msg = msg, Id = id });//передача данных
                }
                    
            }
       }
        // отключение всех клиентов
        protected internal void Disconnect()
        {
            tcpListener.Stop(); //остановка сервера

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //отключение клиента
            }
            Environment.Exit(0); //завершение процесса
            
        }
    }
}
