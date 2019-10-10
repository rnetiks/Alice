using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Alice.Discord.Add_ins
{
    class TaskScheduler
    {
        public static long TimeLeft;
        private int DaysLeft;
        private int HoursLeft;
        private int MinutesLeft;
        Timer timer = new Timer();
        Action GetAction;
        public TaskScheduler(int days, int hours, int minutes, Action action)
        {
            DaysLeft = days;
            GetAction = action;
            timer.Interval = TimeSpan.FromMinutes(1).TotalMilliseconds;
            timer.Start();
            timer.Elapsed += async delegate
            {
                if (DaysLeft > 0 || HoursLeft > 0 || MinutesLeft > 0)
                {
                    MinutesLeft--;
                    if (MinutesLeft == 0 && HoursLeft > 0)
                    {
                        MinutesLeft = 60;
                        HoursLeft--;
                        if (HoursLeft == 0 && DaysLeft > 0)
                        {
                            HoursLeft = 24;
                            DaysLeft--;
                        }
                    }

                    TimeLeft = (long)(new TimeSpan(DaysLeft, HoursLeft, MinutesLeft, 0).TotalMilliseconds);
                }
                else
                {
                    GetAction.Invoke();
                }
            };
        }
    }
}
