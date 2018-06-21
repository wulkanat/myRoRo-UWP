using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myRoRo
{
    public class ScheduleEntry
    {
        public string ClassName
        {
            get;
            set;
        }

        public string Entries
        {
            get;
            set;
        }

        public int Height
        {
            get;
            set;
        }
    }

    public class Schedule
    {
        public string DayName
        {
            get;
            set;
        }

        public List<ScheduleEntry> ScheduleEntries
        {
            get;
            set;
        }
    }

    public class ScheduleManager
    {
        public static ObservableCollection<Schedule> GetSchedules()
        {
            Windows.Storage.ApplicationDataContainer localSettings =
                Windows.Storage.ApplicationData.Current.LocalSettings;

            #if DEBUG
                Debug.WriteLine("Creating ScheduleManager");
            #endif

            ObservableCollection<Schedule> outList = new ObservableCollection<Schedule>();

            int pagesCount = (int) localSettings.Values[ScheduleNetwork.PAGES_COUNT];

            for (int i = 1; i <= pagesCount; i++)
            {
                List<ScheduleEntry> entries = new List<ScheduleEntry>();
                string date = ScheduleNetwork.GetDate(i);

                ScheduleHandler handler = new ScheduleHandler(i);

                List<string> classes = handler.getClassList();

                foreach (string classStr in classes)
                {
                    entries.Add(new ScheduleEntry
                    {
                        ClassName = classStr,
                        Entries = handler.GetClassInfo(classStr)
                    });
                }

                outList.Add(new Schedule
                {
                    DayName = date,
                    ScheduleEntries = entries
                });
            }

            return outList;
        }
    }
}