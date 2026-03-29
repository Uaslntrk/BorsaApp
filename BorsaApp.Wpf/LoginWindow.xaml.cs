using BorsaApp.BLL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace BorsaApp.Wpf
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService _auth = new();

        public LoginWindow()
        {
            InitializeComponent();
            UsernameBox.Text = "admin";
        }

        private async void OnLogin(object sender, RoutedEventArgs e)
        {
            Msg.Text = "s";
            var res = await _auth.LoginAsync(UsernameBox.Text.Trim(), PasswordBox.Password);

            if (!res.Success)
            {
                Msg.Text = res.Message;
                return;
            }
            new MainWindow(res.Role, res.UserId).Show();
            this.Close();

            MessageBox.Show($"Hoş geldin {res.Username} ({res.Role})");
        }
    }
}
