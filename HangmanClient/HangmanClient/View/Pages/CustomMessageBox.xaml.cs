using System;
using System.Collections.Generic;
using System.Linq;
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

namespace HangmanClient.View.Pages
{
    /// <summary>
    /// Interaction logic for CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public bool Resultado { get; private set; }

        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public string TextoAceptar { get; set; } = "Aceptar";
        public string TextoCancelar { get; set; } = "Cancelar";

        public CustomMessageBox(string titulo, string mensaje, string textoAceptar = "Aceptar", string textoCancelar = "Cancelar")
        {
            InitializeComponent();
            Titulo = titulo;
            Mensaje = mensaje;
            TextoAceptar = textoAceptar;
            TextoCancelar = textoCancelar;
            DataContext = this;
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            Resultado = true;
            Close();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Resultado = false;
            Close();
        }
    }
}
