using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace MESSENGER
{
    /// <summary>
    /// Логика взаимодействия для Messenger.xaml
    /// </summary>
    public partial class Messenger : Window
    {
        Socket socket;
        string ip;
        string name;
        public Messenger(string ip, string name)
        {
            InitializeComponent();
            this.name = name;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(IPAddress.Parse(ip), 6464);
                Task.Run(() => RecieveMessage());
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}");
                Close();
            }
        }

        private async void SendMessage(string message)
        {
            string formattedMessage = $"[{DateTime.Now.ToString("HH:mm:ss")}] {name}: {message}";
            var data = Encoding.UTF8.GetBytes(formattedMessage);
            await socket.SendAsync(data, SocketFlags.None);
        }

        private void Otpravka_Click(object sender, RoutedEventArgs e)
        {
            string message = Soobsheniya.Text;
            if (message == "/disconnect")
            {
                DisconnectFromServer();
            }
            else
            {
                SendMessage(message);
                Soobsheniya.Text = string.Empty;
            }
        }
        private void DisconnectFromServer()
        {
            SendMessage("[SYSTEM] Отключился от сервака.");
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            Close();
        }
        private void Vyhod_Click(object sender, RoutedEventArgs e)
        {
            DisconnectFromServer();
        }

        private async void RecieveMessage()
        {
            while (true)
            {
                var data = new byte[65452];
                int bytesReceived = await socket.ReceiveAsync(data, SocketFlags.None);
                if (bytesReceived == 0)
                {
                    break;
                }
                string message = Encoding.UTF8.GetString(data, 0, bytesReceived);
                AllMesseg.Dispatcher.Invoke(() =>
                {
                    AllMesseg.Items.Add(message);
                });
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}
