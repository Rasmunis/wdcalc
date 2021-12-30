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
            workdayCalendar.SetHoliday(tuesday.Date);
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
        
        [Theory]
        [InlineData(5, 24, 19, 3, 7, 27, 13, 47, 44.723656)]
        [InlineData(5, 24, 18, 3, 5, 13, 10, 2, -6.7470217)]
        [InlineData(5, 24, 8, 3, 6, 10, 14, 18, 12.782709)]
        [InlineData(5, 24, 7, 3, 6, 4, 10, 12, 8.276628)]
        public void Should_handle_values_from_vivende_document(int sM, int sd, int sh, int sm, int eM, int ed, int eh, int em, decimal increment)
        {
            workdayCalendar.SetRecurringHoliday(5, 17);
            workdayCalendar.SetHoliday(new DateTime(2004, 5, 27));
            var startDate = new DateTime(2004, sM, sd, sh, sm, 0);
            var incrementedDate = new DateTime(2004, eM, ed, eh, em, 0);
            Assert.Equal(workdayCalendar.GetWorkdayIncrement(startDate, increment), incrementedDate);
        }
        
        [Fact]
        public void Should_throw_on_invalid_recurring_holiday()
        {
            Assert.Throws<ArgumentException>(() => workdayCalendar.SetRecurringHoliday(13, 11));
        }
        
        [Fact]
        public void Should_throw_on_invalid_workday_start_and_stop()
        {
            ArgumentException startTimeException = Assert.Throws<ArgumentException>(() => workdayCalendar.SetWorkdayStartAndStop(100, 10, 12, 10));
            Assert.Equal("Start-time is not valid", startTimeException.Message);
            ArgumentException stopTimeException = Assert.Throws<ArgumentException>(() => workdayCalendar.SetWorkdayStartAndStop(12, 10, 100, 10));
            Assert.Equal("Stop-time is not valid", stopTimeException.Message);
            ArgumentException startLessThanStopException = Assert.Throws<ArgumentException>(() => workdayCalendar.SetWorkdayStartAndStop(16, 0, 12, 0));
            Assert.Equal("The start time must be less than the stop time", startLessThanStopException.Message);
        }
    }
}