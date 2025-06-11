using HangmanClient.Model.Singleton;
using HangmanClient.Util;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace HangmanClient.View.Pages
{
    public partial class ProfileForm : Page
    {
        private readonly bool isEditMode;
        private byte[] profileImageBytes;

        public ProfileForm(bool editMode = false)
        {
            InitializeComponent();
            isEditMode = editMode;

            if (isEditMode)
            {
                CargarDatosUsuario();
            }
        }

        private void CargarDatosUsuario()
        {
            var currentPlayer = SessionManager.Instance.CurrentPlayer;
            if (currentPlayer != null)
            {
                UsernameTextBox.Text = currentPlayer.Username;
                NicknameTextBox.Text = currentPlayer.Nickname;
                PasswordBox.Password = currentPlayer.Password;
                EmailTextBox.Text = currentPlayer.Email;

                if (currentPlayer.Birthdate.HasValue)
                    BirthdatePicker.SelectedDate = currentPlayer.Birthdate.Value;

                if (!string.IsNullOrWhiteSpace(currentPlayer.PhoneNumber))
                    PhoneNumberTextBox.Text = currentPlayer.PhoneNumber;

                UsernameTextBox.IsEnabled = false;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos(out NotificationContent content))
            {
                var window = new NotificationWindow(content);
                window.ShowDialog();
                return;
            }

            var hangmanService = new HangmanServiceReference.HangmanServiceClient();

            var newPlayer = new HangmanServiceReference.PlayerDTO
            {
                Username = UsernameTextBox.Text.Trim(),
                Nickname = NicknameTextBox.Text.Trim(),
                Password = PasswordBox.Password,
                Email = EmailTextBox.Text.Trim(),
                Birthdate = BirthdatePicker.SelectedDate,
                PhoneNumber = PhoneNumberTextBox.Text.Trim(),
                ImgBytes = profileImageBytes 
            };

            try
            {
                bool registrado = hangmanService.RegisterPlayer(newPlayer);

                if (registrado)
                {
                    var successContent = new NotificationContent
                    {
                        NotificationTitle = Literals.RegistrationSuccessful,
                        NotificationMessage = Literals.Welcome,
                        Type = NotificationType.Confirmation,
                        AcceptButtonText = Literals.Accept
                    };
                    var window = new NotificationWindow(successContent);
                    window.ShowDialog();

                    NavigationService.GoBack();
                }
                else
                {
                    MessageBox.Show("No se pudo completar el registro (duplicado o error en datos).");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar: {ex.Message}");
            }
        }

        private bool ValidarCampos(out NotificationContent mensajeError)
        {
            mensajeError = new NotificationContent();

            string username = UsernameTextBox.Text.Trim();
            string nickname = NicknameTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string email = EmailTextBox.Text.Trim();
            string phoneNumber = PhoneNumberTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(nickname) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(email))
            {
                mensajeError.NotificationTitle = Literals.EmptyFields;
                mensajeError.NotificationMessage = Literals.EmptyFieldsDescription;
                mensajeError.Type = NotificationType.Error;
                mensajeError.AcceptButtonText = Literals.Accept;
                return false;
            }

            if (username.Length > 60 ||
                nickname.Length > 12 ||
                password.Length > 20 ||
                email.Length > 45)
            {
                mensajeError.NotificationTitle = Literals.InvalidFields;
                mensajeError.NotificationMessage = Literals.FieldsTooLong;
                mensajeError.Type = NotificationType.Error;
                mensajeError.AcceptButtonText = Literals.Accept;
                return false;
            }

            var regex = new Regex("^[a-zA-Z0-9]+$");
            if (!regex.IsMatch(username) || !regex.IsMatch(nickname) || !regex.IsMatch(password))
            {
                mensajeError.NotificationTitle = Literals.InvalidFields;
                mensajeError.NotificationMessage = Literals.CharacterFields;
                mensajeError.Type = NotificationType.Error;
                mensajeError.AcceptButtonText = Literals.Accept;
                return false;
            }

            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(email))
            {
                mensajeError.NotificationTitle = Literals.InvalidFields;
                mensajeError.NotificationMessage = Literals.NotVaildEmail;
                mensajeError.Type = NotificationType.Error;
                mensajeError.AcceptButtonText = Literals.Accept;
                return false;
            }

            if (!string.IsNullOrWhiteSpace(phoneNumber) && !Regex.IsMatch(phoneNumber, @"^\d{0,10}$"))
            {
                mensajeError.NotificationTitle = Literals.InvalidFields;
                mensajeError.NotificationMessage = Literals.PhoneNumberLength;
                mensajeError.Type = NotificationType.Error;
                mensajeError.AcceptButtonText = Literals.Accept;
                return false;
            }

            return true;
        }

        private void PasswordBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, "^[a-zA-Z0-9]+$"))
            {
                e.Handled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var email = EmailTextBox.Text.Trim();
            if (string.IsNullOrEmpty(email))
            {
                EmailFormatStatusLabel.Visibility = Visibility.Collapsed;
                return;
            }

            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(email))
            {
                EmailFormatStatusLabel.Visibility = Visibility.Visible;
                EmailFormatStatusLabel.Content = "El email no tiene un formato válido.";
            }
            else
            {
                EmailFormatStatusLabel.Visibility = Visibility.Collapsed;
            }
        }
        private async void PhoneNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var phoneNumber = PhoneNumberTextBox.Text.Trim();
            if (string.IsNullOrEmpty(phoneNumber))
            {
                PhoneStatusLabel.Visibility = Visibility.Collapsed;
                return;
            }

            if (!Regex.IsMatch(phoneNumber, @"^\d{0,15}$"))
            {
                PhoneStatusLabel.Visibility = Visibility.Visible;
                PhoneStatusLabel.Content = "El número de teléfono debe ser numérico.";
                return;
            }

            try
            {
                var hangmanService = new HangmanServiceReference.HangmanServiceClient();
                bool phoneEnUso = await hangmanService.PhoneNumberExisteAsync(phoneNumber);

                PhoneStatusLabel.Visibility = phoneEnUso ? Visibility.Visible : Visibility.Collapsed;
                PhoneStatusLabel.Content = phoneEnUso ? "Este número de teléfono ya está en uso." : "";
            }
            catch (Exception ex)
            {
                PhoneStatusLabel.Visibility = Visibility.Visible;
                PhoneStatusLabel.Content = $"Error al validar número de teléfono: {ex.Message}";
            }
        }
        private async void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var username = UsernameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(username))
            {
                UsernameStatusLabel.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                var hangmanService = new HangmanServiceReference.HangmanServiceClient();
                bool usernameEnUso = await hangmanService.UsernameExisteAsync(username);

                UsernameStatusLabel.Visibility = usernameEnUso ? Visibility.Visible : Visibility.Collapsed;
                UsernameStatusLabel.Content = usernameEnUso ? "Este nombre de usuario ya está en uso." : "";
            }
            catch (Exception ex)
            {
                UsernameStatusLabel.Visibility = Visibility.Visible;
                UsernameStatusLabel.Content = $"Error al validar usuario: {ex.Message}";
            }
        }
        private async void NicknameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var nickname = NicknameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(nickname))
            {
                NicknameStatusLabel.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                var hangmanService = new HangmanServiceReference.HangmanServiceClient();
                bool nicknameEnUso = await hangmanService.NicknameExisteAsync(nickname);

                NicknameStatusLabel.Visibility = nicknameEnUso ? Visibility.Visible : Visibility.Collapsed;
                NicknameStatusLabel.Content = nicknameEnUso ? "Este apodo ya está en uso." : "";
            }
            catch (Exception ex)
            {
                NicknameStatusLabel.Visibility = Visibility.Visible;
                NicknameStatusLabel.Content = $"Error al validar apodo: {ex.Message}";
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}