using System;
using Xunit;

namespace Tests
{
    public class WorkdayCalendarTests
    {
        private readonly DateTime monday = new DateTime(2021, 12, 6, 12, 0, 0);
        private readonly DateTime tuesday = new DateTime(2021, 12, 7, 12, 0, 0);
        private readonly DateTime wednesday = new DateTime(2021, 12, 8, 12, 0, 0);
        private readonly DateTime thursday = new DateTime(2021, 12, 9, 12, 0, 0);
        private readonly DateTime friday = new DateTime(2021, 12, 10, 12, 0, 0);
        
        private WorkdayCalendar.WorkdayCalendar workdayCalendar;
        public WorkdayCalendarTests()
        {
            workdayCalendar = new WorkdayCalendar.WorkdayCalendar();
            // default workday start and stop
            workdayCalendar.SetWorkdayStartAndStop(8, 0, 16, 0);
        }

        [Fact]
        public void Should_skip_weekends()
        {
            Assert.Equal(DayOfWeek.Friday, friday.DayOfWeek);
            Assert.Equal(friday.AddDays(3), workdayCalendar.GetWorkdayIncrement(friday, 1));
        }
        
        [Fact]
        public void Should_skip_holidays()
        {
            workdayCalendar.SetHoliday(tuesday);
            Assert.Equal(workdayCalendar.GetWorkdayIncrement(monday, 1), wednesday);
        }
        
        [Fact]
        public void Should_skip_recurring_holidays_regardless_of_year()
        {
            workdayCalendar.SetRecurringHoliday(tuesday.Month, tuesday.Day);
            Assert.Equal(workdayCalendar.GetWorkdayIncrement(monday, 1), wednesday);
            var mondayNextYear = monday.AddYears(1);
            var wednesdayNextYear = wednesday.AddYears(1);
            Assert.Equal(workdayCalendar.GetWorkdayIncrement(mondayNextYear, 1), wednesdayNextYear);
        }
        
        [Fact]
        public void Should_handle_negative_increments()
        {
            Assert.Equal(workdayCalendar.GetWorkdayIncrement(thursday, -1), wednesday);
        }
        
        [Fact]
        public void Should_handle_decimal_increments()
        {
            Assert.Equal(workdayCalendar.GetWorkdayIncrement(tuesday, 1.25m), wednesday.AddHours(2));
        }
        
        [Fact]
        public void Should_handle_negative_decimal_increments()
        {
            Assert.Equal(workdayCalendar.GetWorkdayIncrement(thursday, -1.25m), wednesday.AddHours(-2));
        }
        
        [Fact]
        public void Should_handle_invalid_start_date()
        {
            var startDate = new DateTime(2021, 12, 6, 1, 0, 0);
            var parsedAndIncrementedDate = new DateTime(2021, 12, 7, 8, 0, 0);
            Assert.Equal(workdayCalendar.GetWorkdayIncrement(startDate, 1), parsedAndIncrementedDate);
        }
        
        [Fact]
        public void Should_handle_the_fraction_decrementing_an_extra_workday()
        {
            var decrementedDay = monday.AddHours(2); // Monday at 14:00
            Assert.Equal(workdayCalendar.GetWorkdayIncrement(wednesday, -1.75m), decrementedDay);
        }
        
        [Fact]
        public void Should_handle_the_fraction_incrementing_an_extra_workday()
        {
            var incrementedDay = friday.AddHours(-2); // Friday at 10:00
            Assert.Equal(workdayCalendar.GetWorkdayIncrement(wednesday, 1.75m), incrementedDay);
        }
        
        [Fact]
        public void Should_handle_a_weirdly_sized_workday()
        {
            var startDate = new DateTime(2021, 12, 8, 8, 18, 0);
            var incrementedDate = new DateTime(2021, 12, 6, 8, 48, 0);
            workdayCalendar.SetWorkdayStartAndStop(8, 12, 8, 52);
            Assert.Equal(workdayCalendar.GetWorkdayIncrement(startDate, -1.25m), incrementedDate);
        }
    }
}