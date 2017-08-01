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
        private Boolean IsQWERTY_First = true;
        private Boolean IsOPTI_Turn;

        private DispatcherTimer TaskTimer = new DispatcherTimer();
        private Stopwatch TaskStopWatch = new Stopwatch();
        private String CurrTimeLabel = String.Empty;
        private TimeSpan CurrTimeSpan;

        private Int32 Stage = 0;
        private Int32 CurrInputIndex = 0;
        private String CurrPhrase;

        private MediaPlayer MP_tick;
        private MediaPlayer MP_beep;

        public MainWindow()
        {
            InitializeComponent();

            MainController.Instance._MainWindow = this;
            MainController.Instance._OPTI = OPTI_Control;
            MainController.Instance._QWERTY = QWERTY_Control;

            SessionManager.Instance.ReadSamples();
            InitializeMediaPlayers();

            TaskTimer.Tick += new EventHandler(TaskTimer_Tick);
            TaskTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        private void InitializeMediaPlayers()
        {
            MP_tick = new MediaPlayer();
            MP_beep = new MediaPlayer();

            MP_tick.Open(new Uri("./tick.mp3", UriKind.Relative));
            MP_beep.Open(new Uri("./beep.mp3", UriKind.Relative));
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            WindowStyleHelper.RemoveIcon(this);
        }

        public void ApplyKeyStrokeInput(String input)
        {
            MP_tick.Stop();
            MP_beep.Stop();
            MP_tick.Position = TimeSpan.Zero;
            MP_beep.Position = TimeSpan.Zero;

            if (input == "SPACE") 
                input = " ";
            InputText.Text += input;

            // 준비 상태이면 글자 체크 및 수치 계산 등을 하지 않음
            if (Stage == 0)
                return;

            String answer = CurrPhrase.Substring(CurrInputIndex, 1);
            if (input != answer) 
            {
                // 틀린 글자 입력시
                SessionManager.Instance.ErrorLetterNum++;
                MP_beep.Play();
            }
            else 
            {
                // 알맞은 글자 입력시
                MP_tick.Play();
            }

            SessionManager.Instance.LetterNum++;
            CurrInputIndex++;

            if (InputText.Text.Length == TaskText.Text.Length)
            {
                CurrPhrase = SessionManager.Instance.GetSamplePhrase();
                InputText.Text = "";
                CurrInputIndex = 0;
            }

            RefreshTaskPhrase();
        }

        public void RefreshTaskPhrase()
        {
            String first = CurrPhrase.Substring(0, CurrInputIndex);
            String highlight = CurrPhrase.Substring(CurrInputIndex, 1);
            if (highlight == " ") highlight = "_";
            String last = CurrPhrase.Substring(CurrInputIndex + 1, CurrPhrase.Length - CurrInputIndex - 1);
            TaskText.Inlines.Clear();
            TaskText.Inlines.Add(new Run(first));
            Run highlightRun = new Run();
            highlightRun.Foreground = Brushes.Red;
            highlightRun.Text = highlight;
            highlightRun.FontWeight = FontWeights.Bold;
            TaskText.Inlines.Add(highlightRun);
            TaskText.Inlines.Add(new Run(last));
        }

        public void RefreshStage()
        {
            if (Stage == 0) // READY
            {
                QWERTYRecord.Content = "QWERTY : ";
                OPTIRecord.Content = "OPTI : ";
                QWERTYError.Content = "QWERTY : ";
                OPTIError.Content = "OPTI : ";

                InputText.Text = "";

                InfoText.Visibility = Visibility.Visible;
                CurrTimeSpan = SessionManager.Instance.ReadyTimeSpan;
            }
            else if (Stage == 1) // TASK 1
            {
                InfoText.Visibility = Visibility.Collapsed;
                InitializeKeyBoardLayout(true);
                SessionManager.Instance.InitializeSession();
                CurrInputIndex = 0;
                CurrPhrase = SessionManager.Instance.GetSamplePhrase();
                RefreshTaskPhrase();
                CurrTimeSpan = SessionManager.Instance.TaskTimeSpan;
                
                TaskStopWatch.Restart();
            }
            else if (Stage == 2) // REST
            {
                Double record = SessionManager.Instance.GetWordPerMinute();
                Double record2 = SessionManager.Instance.GetErrorRate();
                MessageBox.Show("Entry Speed: " + record + " wpm\n" + "Error Rate : " + record2 + "%", "Session Result");
                if (IsOPTI_Turn == true)
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
                CurrTimeSpan = SessionManager.Instance.RestTimeSpan;

                TaskStopWatch.Restart();
            }
            else if (Stage == 3) // TASK 2
            {
                InfoText.Visibility = Visibility.Collapsed;
                InitializeKeyBoardLayout(false);
                SessionManager.Instance.InitializeSession();
                CurrInputIndex = 0;
                CurrPhrase = SessionManager.Instance.GetSamplePhrase();
                RefreshTaskPhrase();
                CurrTimeSpan = SessionManager.Instance.TaskTimeSpan;

                TaskStopWatch.Restart();
            }
            else
            {
                Double record = SessionManager.Instance.GetWordPerMinute();
                Double record2 = SessionManager.Instance.GetErrorRate();
                MessageBox.Show("Entry Speed: " + record + " wpm\n" + "Error Rate : " + record2 + "%", "Session Result");
                if (IsOPTI_Turn == true)
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

                TaskStopWatch.Stop();
            }
        }

        private void TaskTimer_Tick(object sender, EventArgs e)
        {
            if (TaskStopWatch.IsRunning == true)
            {
                TimeSpan ts = CurrTimeSpan - TaskStopWatch.Elapsed;
                StopWatchLabel.Content = CurrTimeLabel;
                CurrTimeLabel = String.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);

                if (CurrTimeLabel == "00:00")
                {
                    InfoText.Content = "START!";
                }
                else
                {
                    InfoText.Content = "READY ( " + CurrTimeLabel + " )";
                }

                if (ts.Ticks < 0)
                {
                    Stage++;
                    RefreshStage();
                }
            }
        }

        private void InitializeKeyBoardLayout(Boolean isFirst)
        {
            IsOPTI_Turn = IsQWERTY_First ^ isFirst;

            if (IsOPTI_Turn == true)
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

        private void Clear_Practice_Activate(object sender, RoutedEventArgs e)
        {
            InputText.Text = "";
            ClearKeyBoardLayout();
        }

        private void Session_Start(object sender, RoutedEventArgs e)
        {
            QWERTY_Practice.IsEnabled = false;
            OPTI_Practice.IsEnabled = false;
            Clear_Practice.IsEnabled = false;

            TaskStopWatch.Start();
            
            Stage = 0;
            ClearKeyBoardLayout();
            RefreshStage();

            TaskTimer.Start();
        }

        private void Session_Stop(object sender, RoutedEventArgs e)
        {
            if (TaskStopWatch.IsRunning)
            {
                TaskStopWatch.Stop();
                TaskStopWatch.Reset();

                InfoText.Visibility = Visibility.Collapsed;
                ClearKeyBoardLayout();
                TaskText.Text = "";
                InputText.Text = "";
            }

            QWERTY_Practice.IsEnabled = true;
            OPTI_Practice.IsEnabled = true;
            Clear_Practice.IsEnabled = true;
        }

        private void Layout_First_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            String id = (String) rb.Tag;
            if (id == "0" || id == null)
            {
                IsQWERTY_First = true;
            }
            else
            {
                IsQWERTY_First = false;
            }
        }

    }
}
