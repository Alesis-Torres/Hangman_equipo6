﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HangmanClient.View.Pages
{
    /// <summary>
    /// Interaction logic for JoinByCode.xaml
    /// </summary>
    public partial class JoinByCode : Page
    {
        public JoinByCode()
        {
            InitializeComponent();
        }

        private void EnterMatchButton_Click(object sender, RoutedEventArgs e)
        {
            var code = CodeTextBox.Text.Trim();
            // Manejar logica para entrar a la partida con el codigo
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
