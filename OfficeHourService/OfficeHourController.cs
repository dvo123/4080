using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using static OfficeHourService.Controllers.OfficeHourController;

namespace OfficeHourService.Controllers
{
    [ApiController]
    [Route("office-hours")]
    public class OfficeHourController : ControllerBase
    {
        private readonly EmailService emailService; // Declare EmailService instance
        private List<OfficeHour> officeHours;

        public OfficeHourController()
        {
            emailService = new EmailService(); // Initialize EmailService instance
            officeHours = new List<OfficeHour>();
        }

        private static readonly string EMAIL_REGEX = "^[A-Za-z0-9+_.-]+@[A-Za-z0-9.-]+$";
        private static readonly Regex EMAIL_PATTERN = new Regex(EMAIL_REGEX);

        public class OfficeHour
        {
            public DayOfWeek Day { get; }
            public DateTime Time { get; }
            public string Instructor { get; }
            public string Location { get; }
            public string Email { get; }

            public OfficeHour(DayOfWeek day, DateTime time, string instructor, string location, string email)
            {
                Day = day;
                Time = time;
                Instructor = instructor;
                Location = location;
                Email = email;
            }
        }

        [HttpGet]
        public IActionResult GetOfficeHours([FromQuery] string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return BadRequest("File path is required.");
            }

            if (!System.IO.File.Exists(filePath))
            {
                return BadRequest("File not found. Please enter a valid file path.");
            }

            List<string> validInstructors = new List<string>();
            List<DayOfWeek> validDays = new List<DayOfWeek>();
            List<DateTime> validHours = new List<DateTime>();
            StringBuilder responseBuilder = new StringBuilder();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                CultureInfo culture = CultureInfo.InvariantCulture;

                List<List<string>> scheduleByDay = new List<List<string>>();
                for (int i = 0; i < 5; i++) // Updated to 5 for Monday to Friday only
                {
                    scheduleByDay.Add(new List<string>());
                }

                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 5)
                    {
                        string dayOfWeekStr = parts[0].Trim();
                        string timeStr = parts[1].Trim();
                        string instructor = parts[2].Trim();
                        string location = parts[3].Trim();
                        string email = parts[4].Trim();

                        DayOfWeek dayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), dayOfWeekStr, true);
                        DateTime time = DateTime.ParseExact(timeStr, "h:mm tt", CultureInfo.InvariantCulture); // Adjusted format
                        validInstructors.Add(instructor);
                        validDays.Add(dayOfWeek);
                        validHours.Add(time);

                        int dayIndex = (int)dayOfWeek;
                        if (dayIndex >= 1 && dayIndex <= 5) // Updated to include Monday to Friday only
                        {
                            string scheduleEntry = $"{time.ToString("h:mm tt", CultureInfo.InvariantCulture)} - {instructor} - {location}<br>";
                            scheduleByDay[dayIndex - 1].Add(scheduleEntry); // Adjusted index for Monday to Friday
                        }
                        OfficeHour officeHour = new OfficeHour(dayOfWeek, time, instructor, location, email);
                        officeHours.Add(officeHour);
                    }
                }

                Dictionary<string, string> instructorsWithEmail = new Dictionary<string, string>();
                foreach (OfficeHour officeHour in officeHours)
                {
                    if (!instructorsWithEmail.ContainsKey(officeHour.Instructor))
                    {
                        instructorsWithEmail.Add(officeHour.Instructor, officeHour.Email);
                    }
                }

                // Sort the instructors in alphabetical order by their names
                var sortedInstructors = instructorsWithEmail.OrderBy(pair => pair.Key);

                StringBuilder scheduleBuilder = new StringBuilder();
                scheduleBuilder.Append("<html><head><title>Office Hour Planner</title></head><body><style>td { padding: 5px; white-space: nowrap; }</style><h1>Office Hour Planner</h1><table>");

                // Build the table header
                scheduleBuilder.Append("<tr>");
                for (int dayIndex = 1; dayIndex <= 5; dayIndex++) // Updated to include Monday to Friday only
                {
                    DayOfWeek dayOfWeek = (DayOfWeek)dayIndex;
                    scheduleBuilder.Append("<th>").Append(dayOfWeek).Append("</th>");
                }
                scheduleBuilder.Append("</tr>");

                // Sort the schedule entries within each day by time
                foreach (List<string> daySchedule in scheduleByDay)
                {
                    daySchedule.Sort((s1, s2) =>
                    {
                        DateTime time1 = DateTime.ParseExact(s1.Split(" - ")[0], "h:mm tt", CultureInfo.InvariantCulture);
                        DateTime time2 = DateTime.ParseExact(s2.Split(" - ")[0], "h:mm tt", CultureInfo.InvariantCulture);
                        return time1.CompareTo(time2);
                    });
                }

                // Build the schedule table
                int maxDayScheduleSize = scheduleByDay.Max(d => d.Count);
                for (int i = 0; i < maxDayScheduleSize; i++)
                {
                    scheduleBuilder.Append("<tr>");
                    foreach (List<string> daySchedule in scheduleByDay)
                    {
                        if (i < daySchedule.Count)
                        {
                            scheduleBuilder.Append("<td>").Append(daySchedule[i]).Append("</td>");
                        }
                        else
                        {
                            scheduleBuilder.Append("<td></td>");
                        }
                    }
                    scheduleBuilder.Append("</tr>");
                }

                scheduleBuilder.Append("</table></body></html>");

                string schedule = scheduleBuilder.ToString();

                // Print the list of instructors and their email addresses
                StringBuilder instructorsListBuilder = new StringBuilder();
                instructorsListBuilder.Append("<html><body><h2>List of Instructors and Email Addresses</h2><ul>");
                foreach (var instructorWithEmaiL in sortedInstructors)
                {
                    instructorsListBuilder.Append("<li>").Append(instructorWithEmaiL.Key).Append(" - ").Append(instructorWithEmaiL.Value).Append("</li>");
                }
                instructorsListBuilder.Append("</ul></body></html>");

                string instructorsList = instructorsListBuilder.ToString();

                // Combine the schedule table and instructors list
                StringBuilder responseBuilder1 = new StringBuilder();
                responseBuilder1.Append(schedule).Append(instructorsList);

                // Return the content as HTML
                return Content(responseBuilder1.ToString(), "text/html");
            }
        }

        [HttpPost("appointment")]
        public IActionResult HandleAppointment([FromQuery] string name,
                                               [FromQuery] string id,
                                               [FromQuery] string email,
                                               [FromQuery] string instructor,
                                               [FromQuery] string dayOfWeekStr,
                                               [FromQuery] string timeStr,
                                               [FromQuery] string instructorEmail,
                                               [FromQuery] string appointmentLocation)
        {
            // Validate the name (letters and spaces only)
            if (!IsValidName(name))
            {
                return BadRequest("Invalid name. Please use letters and spaces only.");
            }

            // Validate the ID (numbers only)
            if (!IsValidID(id))
            {
                return BadRequest("Invalid ID. Please use numbers only.");
            }

            // Validate the email address
            if (!IsValidEmailAddress(email))
            {
                return BadRequest("Invalid email address. Please enter a valid email address.");
            }

            // Convert dayOfWeekStr to DayOfWeek using Enum.TryParse
            if (!Enum.TryParse(dayOfWeekStr, true, out DayOfWeek dayOfWeek))
            {
                return BadRequest("Invalid day of the week. Please try again.");
            }

            // Parse timeStr using "h:mm tt" format
            if (!DateTime.TryParseExact(timeStr, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime time))
            {
                return BadRequest("Invalid time format. Please use the h:mm tt format (e.g., 09:30 AM).");
            }

            // Send the email to Instructor
            emailService.SendAppointmentEmail(name, id, email, instructorEmail, instructor, dayOfWeek.ToString(), time.ToString("h:mm tt"), appointmentLocation);

            return Ok("Appointment is scheduled successfully!");
        }

        // Validate the name (letters and spaces only)
        private bool IsValidName(string name)
        {
            return Regex.IsMatch(name, "^[a-zA-Z ]+$");
        }

        // Validate the ID (numbers only)
        private bool IsValidID(string id)
        {
            return Regex.IsMatch(id, "^[0-9]+$");
        }

        // Validate email address
        private bool IsValidEmailAddress(string email)
        {
            return EMAIL_PATTERN.IsMatch(email);
        }
    }
}
