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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Diagnostics;

namespace OPTI_Experiment
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Boolean IsQWERTYFirst = true;
        private Boolean IsOPTIGo;

        private DispatcherTimer dt = new DispatcherTimer();
        private Stopwatch sw = new Stopwatch();
        private String currentTime = String.Empty;
        
        private TimeSpan currFallBackTime;

        private MediaPlayer mediaPlayer_tick = new MediaPlayer();
        private MediaPlayer mediaPlayer_beep = new MediaPlayer();

        private Int32 stage = 0;
        private Int32 currInputIndex = 0;
        private String currPhrase;

        public MainWindow()
        {
            InitializeComponent();

            MainController.Instance._MainWindow = this;
            MainController.Instance._OPTI = OPTI_Control;
            MainController.Instance._QWERTY = QWERTY_Control;

            SessionManager.Instance.ReadSamples();

            mediaPlayer_tick.Open(new Uri("./tick.mp3", UriKind.Relative));
            mediaPlayer_beep.Open(new Uri("./beep.mp3", UriKind.Relative));

            dt.Tick += new EventHandler(dt_Tick);
            dt.Interval = new TimeSpan(0, 0, 0, 0, 1);  
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            WindowStyleHelper.RemoveIcon(this);
        }

        public void ApplyInput(String input)
        {
            mediaPlayer_tick.Stop();
            mediaPlayer_beep.Stop();
            mediaPlayer_tick.Position = TimeSpan.Zero;
            mediaPlayer_beep.Position = TimeSpan.Zero;

            if (input == "SPACE") input = " ";
            InputText.Text += input;

            // 틀릴 경우 처리
            String answer = currPhrase.Substring(currInputIndex, 1);
            if (input != answer)
            {
                SessionManager.Instance.ErrorWordNums++;
                mediaPlayer_beep.Play();
            }
            else
            {
                mediaPlayer_tick.Play();
            }

            SessionManager.Instance.WordNums++;
            currInputIndex++;

            if (InputText.Text.Length == TaskText.Text.Length)
            {
                currPhrase = SessionManager.Instance.GetSamplePhrase();
                InputText.Text = "";
                currInputIndex = 0;
            }

            RefreshTastText();
        }

        public void RefreshTastText()
        {
            String first = currPhrase.Substring(0, currInputIndex);
            String highlight = currPhrase.Substring(currInputIndex, 1);
            if (highlight == " ") highlight = "_";
            String last = currPhrase.Substring(currInputIndex + 1, currPhrase.Length - currInputIndex - 1);
            TaskText.Inlines.Clear();
            TaskText.Inlines.Add(new Run(first));
            Run highlightRun = new Run();
            highlightRun.Foreground = Brushes.Red;
            highlightRun.Text = highlight;
            TaskText.Inlines.Add(highlightRun);
            TaskText.Inlines.Add(new Run(last));
        }

        public void InitializeStage()
        {
            if (stage == 0) // READY
            {
                QWERTYRecord.Content = "QWERTY : ";
                OPTIRecord.Content = "OPTI : ";
                QWERTYError.Content = "QWERTY : ";
                OPTIError.Content = "OPTI : ";

                InfoText.Visibility = Visibility.Visible;
                currFallBackTime = SessionManager.Instance.termFallBackTime;
            }
            else if (stage == 1) // TASK 1
            {
                InfoText.Visibility = Visibility.Collapsed;
                InitializeKeyBoardLayout(true);
                SessionManager.Instance.InitializeSession();
                currInputIndex = 0;
                currPhrase = SessionManager.Instance.GetSamplePhrase();
                RefreshTastText();
                currFallBackTime = SessionManager.Instance.mainFallBackTime;
                
                sw.Restart();
            }
            else if (stage == 2) // REST
            {
                Double record = SessionManager.Instance.GetWordPerMinute();
                Double record2 = SessionManager.Instance.GetErrorRate();
                MessageBox.Show("Result :\nWPM: " + record + " / Error Rate : " + record2, "QWERTY vs OPTI");
                if (IsOPTIGo == true)
                {
                    OPTIRecord.Content = "OPTI : " + record.ToString();
                    OPTIError.Content = "OPTI : " + record2.ToString();
                }
                else
                {
                    QWERTYRecord.Content = "QWERTY : " + record.ToString();
                    QWERTYError.Content = "QWERTY : " + record2.ToString();
                }

                InfoText.Visibility = Visibility.Visible;
                ClearKeyBoardLayout();
                TaskText.Text = "";
                InputText.Text = "";
                currFallBackTime = SessionManager.Instance.restFallBackTime;

                sw.Restart();
            }
            else if (stage == 3) // TASK 2
            {
                InfoText.Visibility = Visibility.Collapsed;
                InitializeKeyBoardLayout(false);
                SessionManager.Instance.InitializeSession();
                currInputIndex = 0;
                currPhrase = SessionManager.Instance.GetSamplePhrase();
                RefreshTastText();
                currFallBackTime = SessionManager.Instance.mainFallBackTime;

                sw.Restart();
            }
            else
            {
                Double record = SessionManager.Instance.GetWordPerMinute();
                Double record2 = SessionManager.Instance.GetErrorRate();
                MessageBox.Show("Result :\nWPM: " + record + " / Error Rate : " + record2, "QWERTY vs OPTI");
                if (IsOPTIGo == true)
                {
                    OPTIRecord.Content = "OPTI : " + record.ToString();
                    OPTIError.Content = "OPTI : " + record2.ToString();
                }
                else
                {
                    QWERTYRecord.Content = "QWERTY : " + record.ToString();
                    QWERTYError.Content = "QWERTY : " + record2.ToString();
                }

                ClearKeyBoardLayout();
                TaskText.Text = "";
                InputText.Text = "";

                sw.Stop();
            }
        }

        private void dt_Tick(object sender, EventArgs e)
        {
            if (sw.IsRunning)
            {
                TimeSpan ts = currFallBackTime - sw.Elapsed;
                StopWatchLabel.Content = currentTime;
                currentTime = String.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);

                if (currentTime == "00:00")
                {
                    InfoText.Content = "START!!!";
                }
                else
                {
                    InfoText.Content = "READY ( " + currentTime + " )";
                }

                if (ts.Ticks < 0)
                {
                    stage++;
                    InitializeStage();
                }
            }
        }

        private void InitializeKeyBoardLayout(Boolean isFirst)
        {
            IsOPTIGo = IsQWERTYFirst ^ isFirst;

            if (IsOPTIGo == true)
            {
                QWERTY_Control.Visibility = Visibility.Collapsed;
                OPTI_Control.Visibility = Visibility.Visible;
            }
            else
            {
                QWERTY_Control.Visibility = Visibility.Visible;
                OPTI_Control.Visibility = Visibility.Collapsed;
            }
        }

        private void ClearKeyBoardLayout()
        {
            QWERTY_Control.Visibility = Visibility.Collapsed;
            OPTI_Control.Visibility = Visibility.Collapsed;
        }

        private void QWERTY_Practice_Activate(object sender, RoutedEventArgs e)
        {
            QWERTY_Control.Visibility = Visibility.Visible;
            OPTI_Control.Visibility = Visibility.Collapsed;
        }

        private void OPTI_Practice_Activate(object sender, RoutedEventArgs e)
        {
            QWERTY_Control.Visibility = Visibility.Collapsed;
            OPTI_Control.Visibility = Visibility.Visible;
        }

        private void Session_Start(object sender, RoutedEventArgs e)
        {
            sw.Start();
            
            stage = 0;
            InitializeStage();

            dt.Start();
        }

        private void Session_Stop(object sender, RoutedEventArgs e)
        {
            if (sw.IsRunning)
            {
                sw.Stop();
                sw.Reset();

                InfoText.Visibility = Visibility.Collapsed;
                ClearKeyBoardLayout();
                TaskText.Text = "";
                InputText.Text = "";
            }
        }

        private void Layout_First_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            String id = (String) rb.Tag;
            if (id == "0" || id == null)
            {
                IsQWERTYFirst = true;
            }
            else
            {
                IsQWERTYFirst = false;
            }
        }
    }
}
