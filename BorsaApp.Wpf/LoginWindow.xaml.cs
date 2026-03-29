using BorsaApp.BLL.Services;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

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
            Msg.Text = "";
            var res = await _auth.LoginAsync(UsernameBox.Text.Trim(), PasswordBox.Password);

            if (!res.Success)
            {
                Msg.Text = res.Message;
                return;
            }

            new MainWindow(res.Role, res.UserId).Show();
            this.Close();
        }

        private void ShowRegister_Click(object sender, MouseButtonEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
        }

        private void ShowLogin_Click(object sender, MouseButtonEventArgs e)
        {
            RegisterPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
        }

        private async void OnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegMsg.Text = "";
            
            string username = RegUsernameBox.Text.Trim();
            string password = RegPasswordBox.Password;
            string role = (RegRoleBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString() ?? "Customer";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                RegMsg.Text = "Lütfen tüm alanları doldurun.";
                return;
            }

            var res = await _auth.RegisterAsync(username, password, role);

            if (!res.Success)
            {
                RegMsg.Text = res.Message;
                return;
            }

            MessageBox.Show("Kayıt başarılı! Şimdi giriş yapabilirsiniz.");
            ShowLogin_Click(null!, null!);
        }
    }
}
