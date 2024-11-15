using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace server_client_msg
{
    public class Server
    {
        private TcpListener? listener;
        private bool isRunning;
        private Thread? listenThread; // Vlákno na počúvanie klientov
        private List<Thread> clientThreads; // Zoznam vlákien pre jednotlivých klientov

        public ObservableCollection<string> Messages { get; set; }

        public Server(ObservableCollection<string> messages)
        {
            this.Messages = messages;
            this.clientThreads = new List<Thread>(); // Inicializácia zoznamu klientských vlákien
        }

        public void Start(int port)
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                isRunning = true;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add($"Server started on port {port}.");
                });


                listenThread = new Thread(ListenForClients);
                listenThread.Start();
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add($"Error starting the server: {ex.Message}");
                });
            }
        }

        private void ListenForClients()
        {
            while (isRunning)
            {
                try
                {
                    TcpClient client = listener?.AcceptTcpClient() ?? throw new InvalidOperationException("Listener is null.");

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Messages.Add("Client connected.");
                    });

                   
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThreads.Add(clientThread);
                    clientThread.Start();
                }
                catch (Exception ex)
                {
                    if (isRunning) 
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Messages.Add($"Error accepting client: {ex.Message}");
                        });
                    }
                }
            }
        }

        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (client.Connected && isRunning)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; 

                    string clientMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Messages.Add($"Received from client: {clientMessage}");
                    });

                    string responseMessage = $"{clientMessage} + {DateTime.Now:HH:mm:ss} Stano Adam";
                    byte[] responseBytes = Encoding.ASCII.GetBytes(responseMessage);

                    stream.Write(responseBytes, 0, responseBytes.Length);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Messages.Add($"Sent to client: {responseMessage}");
                    });
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add($"Error communicating with client: {ex.Message}");
                });
            }
            
        }

        public void Stop()
        {
            isRunning = false;

            
            listener?.Stop();

         
            if (listenThread != null && listenThread.IsAlive)
            {
                if (!listenThread.Join(10)) 
                {
                    listenThread.Interrupt(); 
                }
            }

            
            foreach (var thread in clientThreads)
            {
                if (thread.IsAlive)
                {
                    if (!thread.Join(10)) 
                    {
                        thread.Interrupt(); 
                    }
                }
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add("Server stopped.");
            });
        }

    }
}
