using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

public class Client
{
    private TcpClient? client;
    private NetworkStream? stream;
    private ObservableCollection<string> Messages;

    public bool IsConnected => client?.Connected ?? false;
    private bool isReceiving; // Flag to track receiving status

    public Client(ObservableCollection<string> messages)
    {
        Messages = messages;
    }

    public void Connect(string ipAddress, int port, int localPort = 0)
    {
        try
        {
            TerminateConnection();
            if (localPort > 0)
            {
                var localEndPoint = new IPEndPoint(IPAddress.Any, localPort);
                client = new TcpClient(localEndPoint); // Bind the client to the specified local port

            }
            else
            {
                client = new TcpClient(); // Default behavior without specifying a local port
            }
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Connect(ipAddress, port);
            stream = client.GetStream();
            isReceiving = true;
            Messages.Add($"Connected to the server from local port {((IPEndPoint)client.Client.LocalEndPoint).Port}.");
            Console.WriteLine($"Connected to the server from local port {((IPEndPoint)client.Client.LocalEndPoint).Port}.");

            Task.Run(async () =>
            {
                await ReceiveMessagesAsync();
            });
        }
        catch (Exception ex)
        {
            Messages.Add($"Connection error: {ex.Message}");
            Console.WriteLine($"Connection error: {ex.Message}");
        }
    }

    public async Task ConnectAndSendUsernameAsync(string ipAddress, int port, int localPort, string username)
    {
        try
        {
            Connect(ipAddress, port, localPort);

            // Odoslanie užívateľského mena
            await SendUsernameAsync(username);

            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add($"Successfully connected and username \"{username}\" sent to server.");
            });
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add($"Error during connect and send username: {ex.Message}");
            });
            Console.WriteLine($"Error during connect and send username: {ex.Message}");
        }
    }

    public async Task SendUsernameAsync(string username)
    {
        if (client != null && client.Connected)
        {
            try
            {
                string message = $"USERNAME:{username}";
                byte[] data = Encoding.ASCII.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add($"Username sent: {username}");
                });

                Console.WriteLine($"Username sent: {username}");
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add($"Error sending username: {ex.Message}");
                });
                Console.WriteLine($"Error sending username: {ex.Message}");
            }
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add("Unable to send username. Not connected to the server.");
            });
            Console.WriteLine("Unable to send username. Not connected to the server.");
        }
    }
    private async Task ReceiveMessagesAsync()
    {
        try
        {
            while (isReceiving && IsConnected)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Messages.Add(response.Trim());
                    });

                    Console.WriteLine($"Received from server: {response.Trim()}");
                }
            }
        }
        catch (Exception ex)
        {
            if (isReceiving) // Only log errors if still receiving
            {
                Console.WriteLine($"Error receiving messages: {ex.Message}");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add($"Error receiving messages: {ex.Message}");
                });
            }
        }
    }





    public async Task SendMessageAsync(string message)
    {
        if (client != null && client.Connected)
        {
            try
            {
                byte[] data = Encoding.ASCII.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add($"Sent: {message}");
                });
                Console.WriteLine($"Message sent: {message}");
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add($"Error sending message: {ex.Message}");
                });
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add("Unable to send message. Not connected to the server.");
            });
            Console.WriteLine("Unable to send message. Not connected to the server.");
        }
    }

    public void TerminateConnection()
    {
        if (client != null)
        {
            this.isReceiving = false;
            try
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                    stream = null;
                }

                client.Close();
                client.Dispose();
                client = null;

                Console.WriteLine("Client connection terminated successfully.");
                Messages.Add("Disconnected from the server.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during client termination: {ex.Message}");
                Messages.Add($"Error during client termination: {ex.Message}");
            }
        }
    }




}
