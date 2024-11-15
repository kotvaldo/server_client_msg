using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace server_client_msg
{
    
    public partial class MainWindow : Window
    {
        ObservableCollection<string> messages;
        Client client;
        Server server;

        
        public MainWindow()
        {
            InitializeComponent();
            messages = new ObservableCollection<string>();
            list_msgs.ItemsSource = messages;
            client = new(messages);
            server = new(messages);
        }

     

        private void btn_connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                server.Start(8080);
               

                client.Connect("127.0.0.1", 8080);
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

                    string response = await client.ReceiveMessageAsync();
                   

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
               

                server.Stop();
               
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
            server.Stop();
        }


    }
}