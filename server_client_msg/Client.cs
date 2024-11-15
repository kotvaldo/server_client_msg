using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace server_client_msg
{
    
    public class Client
    {
        private TcpClient? client;
        private NetworkStream? stream;
        private ObservableCollection<string> Messages;


        public bool IsConnected => client?.Connected ?? false;
        public Client(ObservableCollection<string> messages)
        {
            Messages = messages;
        }

        public void Connect(string ipAddress, int port)
        {
            try
            {
                client = new TcpClient(ipAddress, port);
                stream = client.GetStream();
                Messages.Add("Connected to the server.");
            }
            catch (Exception ex)
            {
                Messages.Add($"Connection error: {ex.Message}");
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (client != null && client.Connected)
            {
                byte[] data = Encoding.ASCII.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add($"Sent: {message}");
                });
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add("Unable to send message. Not connected to the server.");
                });
            }
        }

        public async Task<string> ReceiveMessageAsync()
        {
            if (client != null && client.Connected)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add($"Received from server: {response}");
                });

                return response;
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add("Unable to receive message. Not connected to the server.");
                });
                return string.Empty;
            }
        }


        public void TerminateConnection()
        {
            if (client != null)
            {
                client.Close();
                stream?.Close();
                Messages.Add("Disconnected from the server.");
            }
        }


    }

}
