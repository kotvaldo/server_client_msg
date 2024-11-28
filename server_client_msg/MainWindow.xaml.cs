using System;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;

namespace server_client_msg
{
    public partial class MainWindow : Window
    {
        ObservableCollection<string> messages;
        Client client;

        public MainWindow()
        {
            InitializeComponent();
            messages = new ObservableCollection<string>();
            list_msgs.ItemsSource = messages;
            client = new(messages);
        }

        private async void btn_connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = edit_username.Text.Trim();

                if (string.IsNullOrWhiteSpace(username))
                {
                    messages.Add("Username cannot be empty. Please enter a valid username.");
                    return;
                }

                // Použitie novej metódy
                await client.ConnectAndSendUsernameAsync("127.0.0.1", 8080, 0, username);
            }
            catch (Exception ex)
            {
                messages.Add($"Error during connection: {ex.Message}");
            }
        }


        private async void btn_send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = edit_msg.Text;

                if (!string.IsNullOrWhiteSpace(message))
                {
                    await client.SendMessageAsync(message);
                    edit_msg.Clear();
                }
                else
                {
                    messages.Add("Message cannot be empty.");
                }
            }
            catch (Exception ex)
            {
                messages.Add($"Error sending message: {ex.Message}");
            }
        }

        private void btn_terminate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                client.TerminateConnection();
                messages.Add("Client connection terminated.");
            }
            catch (Exception ex)
            {
                messages.Add($"Error during termination: {ex.Message}");
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            client.TerminateConnection();
        }
    }
}
