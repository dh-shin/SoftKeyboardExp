using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPTI_Experiment
{
    public class MainController
    {
        public MainWindow _MainWindow;
        public OPTI _OPTI;
        public QWERTY _QWERTY;

        public static MainController Instance { get; private set; }

        static MainController()
        {
            Instance = new MainController();
        }
        private MainController()
        {
        }

        public void SendingInput(String input)
        {
            _MainWindow.ApplyKeyStrokeInput(input);
        }
    }
}
