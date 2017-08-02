using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPTI_Experiment
{
    public class SessionManager
    {
        public static SessionManager Instance { get; private set; }

        public List<String> SamplePhrases = new List<String>();

        public readonly TimeSpan ReadyTimeSpan = new TimeSpan(0, 0, 0, 11, 0); // 처음 준비 시간
        public readonly TimeSpan TaskTimeSpan = new TimeSpan(0, 0, 0, 11, 0); // Task 수행 시간
        public readonly TimeSpan RestTimeSpan = new TimeSpan(0, 0, 0, 11, 0); // 중간 휴식 시간

        public Int32 LetterNum;
        public Int32 ErrorLetterNum;
        public Double LetterPerMinute;

        public List<Int32> UsedIndices;

        static SessionManager()
        {
            Instance = new SessionManager();
        }

        private SessionManager()
        {
        }

        public void ReadSamples()
        {
            String line;
            StreamReader file = new StreamReader("SamplePhrases.txt");
            while ((line = file.ReadLine()) != null)
            {
                line = line.ToUpper();
                Console.WriteLine(line);
                SamplePhrases.Add(line);
            }
            file.Close();
        }

        public void InitializeSession()
        {
            LetterNum = 0;
            ErrorLetterNum = 0;
            UsedIndices = new List<Int32>();
        }

        public String GetSamplePhrase()
        {
            Int32 min = 0;
            Int32 max = SamplePhrases.Count;
            Random rand = new Random();

            Int32 pick;
            do
            {
                pick = rand.Next(min, max);
            }
            while (UsedIndices.Contains(pick) == true);
            
            UsedIndices.Add(pick);

            // 모든 문장이 한 번씩 등장했으면, 다시 UsedIndices를 비워준다.
            if (UsedIndices.Count == SamplePhrases.Count)
                UsedIndices.Clear();

            return SamplePhrases[pick];
        }

        public Double GetWordPerMinute()
        {
            Double res = LetterNum / (Double)TaskTimeSpan.Minutes;
            return Math.Round(res / 4, 2);
        }

        public Double GetErrorRate()
        {
            Double res = ErrorLetterNum / (Double)LetterNum;
            return Math.Round(res * 100, 1);
        }
    }
}
