using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MESSENGER
{
    public partial class Server : Window
    {
        private TcpListener server;
        private CancellationTokenSource cancellationTokenSource;
        private List<TcpClient> clients;
        private List<string> log;

        public Server()
        {
            InitializeComponent();
            server = new TcpListener(IPAddress.Any, 6464);
            clients = new List<TcpClient>();
            log = new List<string>();
            cancellationTokenSource = new CancellationTokenSource();
        }

        public async void Start()
        {
            server.Start();
            await Task.Run(() => ListenForClients(cancellationTokenSource.Token), cancellationTokenSource.Token);
        }

        private async Task ListenForClients(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                clients.Add(client);
                log.Add($"{DateTime.Now}: Пользователь подключился.");
                UpdateUserList();
                UpdateLog();
                await Task.Run(() => HandleClientComm(client, cancellationToken), cancellationToken);
            }
        }

        private async Task HandleClientComm(TcpClient client, CancellationToken cancellationToken)
        {
            NetworkStream clientStream = client.GetStream();
            byte[] message = new byte[65452];
            int bytesRead;

            while (!cancellationToken.IsCancellationRequested)
            {
                bytesRead = 0;

                try
                {
                    bytesRead = await clientStream.ReadAsync(message, 0, 65452, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }

                ASCIIEncoding encoder = new ASCIIEncoding();
                string receivedMessage = encoder.GetString(message, 0, bytesRead);
                log.Add(receivedMessage);

                if (receivedMessage.StartsWith("[SYSTEM]"))
                {
                    continue;
                }

                BroadcastMessage(receivedMessage);
            }

            client.Close();
            clients.Remove(client);
            log.Add($"{DateTime.Now}: Пользователь отключился.");
            UpdateUserList();
            UpdateLog();
        }

        private void BroadcastMessage(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            foreach (var client in clients)
            {
                client.GetStream().Write(data, 0, data.Length);
            }
        }

        private void UpdateUserList()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SpisokClientov.Text = string.Join("\n", clients.Select(c => c.Client.RemoteEndPoint.ToString()));
            });
        }

        private void UpdateLog()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Soobshenia.Text = string.Join("\n", log);
            });
        }

        public List<string> GetLog()
        {
            return log;
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            server.Stop();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Stop();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}