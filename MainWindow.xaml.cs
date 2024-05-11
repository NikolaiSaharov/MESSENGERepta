using System.Net;
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

namespace MESSENGER
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Server server = new Server();
            server.Show();
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string ip = IpBox.Text;
            string name = NameBox.Text;

            if (IsValidIp(ip) && IsValidName(name))
            {
                Messenger messenger = new Messenger(ip, name);
                messenger.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный IP или имя пользователя.");
            }
        }
        private bool IsValidIp(string ip)
        {
            IPAddress address;
            return IPAddress.TryParse(ip, out address);
        }
        private bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name);
        }

        private void IpBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void NameBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}