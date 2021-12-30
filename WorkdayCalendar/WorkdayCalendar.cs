using System;

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
        public void SetHoliday(DateTime date)
        {
            throw new NotImplementedException();
        }

        public void SetRecurringHoliday(int month, int day)
        {
            throw new NotImplementedException();
        }

        public void SetWorkdayStartAndStop(int startHours, int startMinutes, int stopHours, int stopMinutes)
        {
            throw new NotImplementedException();
        }

        public DateTime GetWorkdayIncrement(DateTime startDate, decimal incrementInWorkDays)
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            throw new NotImplementedException();
        }
    }
}