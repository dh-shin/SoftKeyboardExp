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

        public List<String> Samples = new List<String>();

        public readonly TimeSpan termFallBackTime = new TimeSpan(0, 0, 0, 11, 0); // 처음 준비 시간
        public readonly TimeSpan mainFallBackTime = new TimeSpan(0, 0, 3, 1, 0); // Task 수행 시간
        public readonly TimeSpan restFallBackTime = new TimeSpan(0, 0, 1, 1, 0); // 중간 휴식 시간

        public Int32 WordNums;
        public Int32 ErrorWordNums;
        public Double WordPerMinute;

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
                Samples.Add(line);
            }
            file.Close();
        }

        public void InitializeSession()
        {
            WordNums = 0;
            ErrorWordNums = 0;
            UsedIndices = new List<Int32>();
        }

        public String GetSamplePhrase()
        {
            Int32 min = 0;
            Int32 max = Samples.Count - 1;
            Random rand = new Random();

            Int32 pick;
            do
            {
                pick = rand.Next(min, max);
            }
            while (UsedIndices.Contains(pick) == true);

            UsedIndices.Add(pick);
            return Samples[pick];
        }

        public Double GetWordPerMinute()
        {
            Double res = WordNums / (Double)mainFallBackTime.Minutes;
            return Math.Round(res, 2);
        }

        public Double GetErrorRate()
        {
            Double res = ErrorWordNums / (Double)WordNums;
            return Math.Round(res, 2);
        }
    }
}
