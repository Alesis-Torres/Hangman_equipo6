using HangmanClient.Model.Singleton;
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

                if (!string.IsNullOrWhiteSpace(currentPlayer.ImgRoute))
                {
                    try
                    {
                        var bitmap = new BitmapImage(new Uri(currentPlayer.ImgRoute));
                        ProfileImage.Source = bitmap;
                    }
                    catch
                    {
                    }
                }

                UsernameTextBox.IsEnabled = false;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos(out string error))
            {
                MessageBox.Show(error);
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
                    MessageBox.Show("¡Registro exitoso!");
                    NavigationService.Navigate(new Login());
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

        private bool ValidarCampos(out string mensajeError)
        {
            mensajeError = "";

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
                mensajeError = "Todos los campos obligatorios deben estar completos.";
                return false;
            }

            if (username.Length > 60 ||
                nickname.Length > 12 ||
                password.Length > 20 ||
                email.Length > 45)
            {
                mensajeError = "Uno o más campos exceden la longitud permitida.";
                return false;
            }

            var regex = new Regex("^[a-zA-Z0-9]+$");
            if (!regex.IsMatch(username) || !regex.IsMatch(nickname) || !regex.IsMatch(password))
            {
                mensajeError = "Username, Nickname y Password sólo deben contener caracteres A-Z, a-z, 0-9.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(phoneNumber) && !Regex.IsMatch(phoneNumber, @"^\d{0,15}$"))
            {
                mensajeError = "El número de teléfono debe contener solo números (hasta 15 dígitos).";
                return false;
            }

            if (profileImageBytes != null && profileImageBytes.Length > 5 * 1024 * 1024)
            {
                mensajeError = "La imagen excede el tamaño máximo de 5 MB.";
                return false;
            }

            return true;
        }

        private void UploadImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Seleccionar Imagen"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                var fileInfo = new FileInfo(filePath);

                if (fileInfo.Length > 5 * 1024 * 1024)
                {
                    MessageBox.Show("La imagen excede el tamaño máximo de 5 MB.");
                    return;
                }

                profileImageBytes = File.ReadAllBytes(filePath);
                ProfileImage.Source = new BitmapImage(new Uri(filePath));
            }
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
            bool result=false;
            if (SessionManager.Instance.CurrentLanguage == 1)
            {
                                var msgBox = new CustomMessageBox(
                    "Cancelar registro",
                    "Los datos ingresados se perderán permanentemente, ¿desea continuar?",
                    "Aceptar",
                    "Cancelar"
                );
                msgBox.ShowDialog();
                result = msgBox.Resultado;
            }
            if(SessionManager.Instance.CurrentLanguage == 2)
            {
                var msgBox = new CustomMessageBox(
                    "Cancel Registration",
                    "Entered data will be permanently lost. Do you wish to continue?",
                    "Yes",
                    "Cancel"
                );
                msgBox.ShowDialog();
                result = msgBox.Resultado;
            }
            if (result)
            {
                if (isEditMode)
                {
                    NavigationService.Navigate(new CreateMatch(false,""));
                }
                else
                {
                    NavigationService.Navigate(new Login());
                }
                
            }
            
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
    }
}