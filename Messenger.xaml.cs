using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;

namespace MESSENGER
{
    public partial class Messenger : Window
    {
        Socket socket;
        string ip;
        string name;
        List<string> log; 

        private void UpdateLog()
        {
            if (Soobsheniya != null) 
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Soobsheniya.Text = string.Join("\n", log);
                });
            }
        }

        public Messenger(string ip, string name)
        {
            InitializeComponent();
            this.name = name;
            this.ip = ip;
            log = new List<string>(); 
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(IPAddress.Parse(ip), 6464);
                Task.Run(() => ReceiveMessage());
            }
            catch (SocketException ex)
            {
                log.Add($"{DateTime.Now}: Ошибка подключения: {ex.Message}");
                UpdateLog(); 
                Close();
            }
        }

        private async Task SendMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message) && socket.Connected)
            {
                string formattedMessage = $"[{DateTime.Now.ToString("HH:mm:ss")}] {name}: {message}";
                var data = Encoding.UTF8.GetBytes(formattedMessage);
                try
                {
                    await socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    log.Add($"{DateTime.Now}: Ошибка отправки сообщения: {ex.Message}");
                    UpdateLog();
                }
                catch (ObjectDisposedException ex)
                {
                    log.Add($"{DateTime.Now}: Ошибка отправки сообщения: {ex.Message}");
                    UpdateLog();
                }
            }
        }

        private async void Otpravka_Click(object sender, RoutedEventArgs e)
        {
            string message = Soobsheniya.Text;
            if (message == "/disconnect")
            {
                await DisconnectFromServer();
            }
            else
            {
                await SendMessage(message);
                Soobsheniya.Text = string.Empty;
            }
        }

        private async Task DisconnectFromServer()
        {
            if (socket.Connected)
            {
                await SendMessage("[SYSTEM] Отключился от сервера.");
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                Close();
            }
        }

        private void Vyhod_Click(object sender, RoutedEventArgs e)
        {
            DisconnectFromServer().Wait(); 
        }

        private async void ReceiveMessage()
        {
            while (socket.Connected)
            {
                var data = new byte[65452];
                int bytesRead;
                try
                {
                    bytesRead = await socket.ReceiveAsync(new ArraySegment<byte>(data), SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    log.Add($"{DateTime.Now}: Ошибка при получении сообщения: {ex.Message}");
                    UpdateLog();
                    break;
                }
                catch (ObjectDisposedException ex)
                {
                    log.Add($"{DateTime.Now}: Ошибка при получении сообщения: {ex.Message}");
                    UpdateLog();
                    break;
                }
                if (bytesRead == 0)
                {
                    break;
                }
                string message = Encoding.UTF8.GetString(data, 0, bytesRead);
                AllMesseg.Dispatcher.Invoke(() =>
                {
                    AllMesseg.Items.Add(message);
                });
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }
    }
}