using System;
using System.Collections.Generic;

namespace WorkdayCalendar
{
    public interface IWorkdayCalendar
    {
        void SetHoliday(DateTime date);
        void SetRecurringHoliday(int month, int day);
        void SetWorkdayStartAndStop(int startHours, int startMinutes, int stopHours, int stopMinutes);
        DateTime GetWorkdayIncrement(DateTime startDate, decimal incrementInWorkDays);
        string GetName();
    }

    public class WorkdayCalendar : IWorkdayCalendar
    {
        public int StartHours { get; set; }
        public int StartMinutes { get; set; }
        public int StopHours { get; set; }
        public int StopMinutes { get; set; }
        public int MinutesInAWorkday { get; set; }
        public HashSet<DateTime> Holidays = new ();
        public HashSet<RecurringHoliday> RecurringHolidays = new ();
        
        
        public void SetHoliday(DateTime date)
        {
            Holidays.Add(date);
        }

        public void SetRecurringHoliday(int month, int day)
        {
            // using 2020 as it is a leap year, making 29th of february a valid combination
            if (DateTime.TryParse($"{day}/{month}/2020", out DateTime date))
            {
                RecurringHolidays.Add(new RecurringHoliday(month, day));
            }
            else
            {
                throw new ArgumentException("Invalid value of month and day");
            }
        }

        public void SetWorkdayStartAndStop(int startHours, int startMinutes, int stopHours, int stopMinutes)
        {
            if (startHours < 24 && startMinutes < 59)
            {
                if (stopHours < 24 && stopMinutes <= 59)
                {
                    var startMinutesFromMidnight = startHours * 60 + startMinutes;
                    var stopMinutesFromMidnight = stopHours * 60 + stopMinutes;
                    if (startMinutesFromMidnight < stopMinutesFromMidnight)
                    {
                        StartHours = startHours;
                        StartMinutes = startMinutes;
                        StopHours = stopHours;
                        StopMinutes = stopMinutes;
                        MinutesInAWorkday = stopMinutesFromMidnight - startMinutesFromMidnight;
                    }
                    else
                    {
                        throw new ArgumentException("The start time must be less than the stop time");
                    }
                }
                else
                {
                    throw new ArgumentException("Stop-time is not valid");
                }
            }
            else
            {
                throw new ArgumentException("Start-time is not valid");
            }
        }

        public DateTime GetWorkdayIncrement(DateTime startDate, decimal incrementInWorkDays)
        {
            if (RecurringHolidays.Count < 366)
            {
                var parsedStartDate = GetStartDateWithValidWorkdayTimes(startDate);
                var (workdaysToAdd, minutesToAdd) = GetWorkdaysAndMinutesToAdd(parsedStartDate, incrementInWorkDays);
                
                var workdaysIncremented = 0;
                var daysIncremented = 0;
                var stepSize = Math.Abs(workdaysToAdd) / workdaysToAdd;
                while (workdaysIncremented != workdaysToAdd + stepSize)
                {
                    var dateCursor = parsedStartDate.AddDays(daysIncremented);
                    if (Holidays.Contains(dateCursor))
                    {
                        daysIncremented += stepSize;
                    }
                    else if (RecurringHolidays.Contains(new RecurringHoliday(dateCursor.Month, dateCursor.Day)))
                    {
                        daysIncremented += stepSize;
                    }
                    else if (dateCursor.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    {
                        daysIncremented += stepSize;
                    }
                    else
                    {
                        daysIncremented += stepSize;
                        workdaysIncremented += stepSize;
                    }
                }

                return parsedStartDate.AddDays(daysIncremented - stepSize).AddMinutes(minutesToAdd);
            }
            else
            {
                throw new Exception("Enjoy your retirement");
            }
        }

        private Tuple<int, int> GetWorkdaysAndMinutesToAdd(DateTime startDate, decimal incrementInWorkDays)
        {
            var workdaysToAdd = Math.Truncate(incrementInWorkDays);
            var minutesToAdd = Decimal.Round((incrementInWorkDays - workdaysToAdd) * MinutesInAWorkday);
            var minutesFromStartTimeOnStartDate = (int)(startDate - new DateTime(startDate.Year, startDate.Month,
                startDate.Day, StartHours, StartMinutes, 0)).TotalMinutes;

            if (minutesToAdd + minutesFromStartTimeOnStartDate < 0)
            {
                workdaysToAdd -= 1;
                minutesToAdd += MinutesInAWorkday;
            }
            
            if (minutesToAdd + minutesFromStartTimeOnStartDate > MinutesInAWorkday)
            {
                workdaysToAdd += 1;
                minutesToAdd -= MinutesInAWorkday;
            }

            return Tuple.Create((int)workdaysToAdd, (int)minutesToAdd);
        }

        private DateTime GetStartDateWithValidWorkdayTimes(DateTime startDate)
        {
            if (startDate.Hour < StartHours || (startDate.Hour == StartHours && startDate.Minute < StartMinutes))
            {
                return new DateTime(startDate.Year, startDate.Month, startDate.Day, StartHours, StartMinutes, 0);
            }
            
            if (startDate.Hour > StopHours || (startDate.Hour == StopHours && startDate.Minute > StopMinutes))
            {
                return new DateTime(startDate.Year, startDate.Month, startDate.Day, StopHours, StopMinutes, 0);
            }

            return startDate;
        }

        public string GetName()
        {
            return "Rasmus Stene's Ultimate Workday Calendar";
        }
    }

    public record RecurringHoliday(int Month, int Day);
}