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

namespace OPTI_Experiment
{
    /// <summary>
    /// Interaction logic for OPTI.xaml
    /// </summary>
    public partial class OPTI : UserControl
    {
        public OPTI()
        {
            InitializeComponent();
        }

        private void OPTI_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            String currChar = button.Content.ToString();
            //MessageBox.Show("Input : " + currChar, "QWERTY vs OPTI");
            MainController.Instance.SendingInput(currChar);
        }
    }
}
