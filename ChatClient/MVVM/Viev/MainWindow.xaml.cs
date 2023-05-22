using ChatClient.MVVM.ViewModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatClient
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

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                connect_button.IsEnabled = false;
                username_text.IsEnabled = false;
                messageBox.IsEnabled = true;
                send_button.IsEnabled = true;
                var command = button.Tag as ICommand;
                command?.Execute(button.CommandParameter);
            }
        }

        private void username_text_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(username_text.Text.Length > 0 && !username_text.Text.StartsWith(" "))
                connect_button.IsEnabled = true;
            else
                connect_button.IsEnabled = false;
        }
    }
}
