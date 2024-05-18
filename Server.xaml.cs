using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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

        public async Task Start()
        {
            if (!server.Server.IsBound)
            {
                server.Start();
            }
            await ListenForClients(cancellationTokenSource.Token);
        }

        private async Task ListenForClients(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await server.AcceptTcpClientAsync();
                    clients.Add(client);
                    log.Add($"{DateTime.Now}: Пользователь подключился.");
                    UpdateUserList();
                    UpdateLog();
                    _ = HandleClientComm(client, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    log.Add($"{DateTime.Now}: Ошибка: {ex.Message}");
                    UpdateLog();
                }
            }
        }

        private async Task HandleClientComm(TcpClient client, CancellationToken cancellationToken)
        {
            NetworkStream clientStream = client.GetStream();
            byte[] message = new byte[65452];
            int bytesRead;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    bytesRead = 0;
                    bytesRead = await clientStream.ReadAsync(message, 0, 65452, cancellationToken);

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
            }
            catch (OperationCanceledException)
            {
                // Операция отменена, просто выходим из метода
            }
            catch (Exception ex)
            {
                log.Add($"{DateTime.Now}: Ошибка при обработке сообщения: {ex.Message}");
                UpdateLog();
            }
            finally
            {
                if (client.Connected)
                {
                    client.Close();
                }
                clients.Remove(client);
                log.Add($"{DateTime.Now}: Пользователь отключился.");
                UpdateUserList();
                UpdateLog();
            }
        }

        private void BroadcastMessage(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            foreach (var client in clients.ToList()) // ToList() для создания моментального снимка коллекции
            {
                try
                {
                    client.GetStream().Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    log.Add($"{DateTime.Now}: Ошибка при отправке сообщения: {ex.Message}");
                    UpdateLog();
                }
            }
        }

        private void UpdateUserList()
        {
            if (SpisokClientov != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SpisokClientov.Text = string.Join("\n", clients.Select(c => c.Client.RemoteEndPoint.ToString()));
                });
            }
        }

        private void UpdateLog()
        {
            if (Soobshenia != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Soobshenia.Text = string.Join("\n", log);
                });
            }
        }

        public List<string> GetLog()
        {
            return log;
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            foreach (var client in clients)
            {
                if (client.Connected)
                {
                    client.Close();
                }
            }
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