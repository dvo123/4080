using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Model;
using sib_api_v3_sdk.Client;

namespace OfficeHourService
{
    public class EmailService
    {
        private static readonly string API_KEY = "Enter API Key Here";
        private static readonly string SENDER_EMAIL = "ru.vo147@gmail.com";
        private static readonly string SENDER_NAME = "officehourplanner@gmail.com";

        public void SendAppointmentEmail(string userName, string userId, string emailAddress, string instructorEmail, string instructor, string dayOfWeek, string time, string appointmentLocation)
        {
            Configuration.Default.AddApiKey("api-key", API_KEY);
            TransactionalEmailsApi transactionalEmailsApi = new TransactionalEmailsApi();

            SendSmtpEmail email = new SendSmtpEmail
            {
                Sender = new SendSmtpEmailSender(SENDER_EMAIL, SENDER_NAME),
                To = new List<SendSmtpEmailTo> { new SendSmtpEmailTo(emailAddress) },
                Subject = "Appointment Details",
                TextContent = GetAppointmentEmailContent(userName, instructorEmail, instructor, dayOfWeek, time, appointmentLocation)
            };

            // Send email to the instructor
            SendSmtpEmail emailtoInstructor = new SendSmtpEmail
            {
                Sender = new SendSmtpEmailSender(SENDER_EMAIL, SENDER_NAME),
                To = new List<SendSmtpEmailTo> { new SendSmtpEmailTo(instructorEmail) },
                Subject = "Appointment Request",
                TextContent = GetAppointmentInstructorEmailContent(userName, userId, emailAddress, instructor, dayOfWeek, time, appointmentLocation)
            };

            try
            {
                // Send both emails
                CreateSmtpEmail userResponse = transactionalEmailsApi.SendTransacEmail(email);
                CreateSmtpEmail instructorResponse = transactionalEmailsApi.SendTransacEmail(emailtoInstructor);

                Console.WriteLine("Emails sent successfully!");
            }
            catch (ApiException apiException)
            {
                Console.WriteLine($"API Exception: {apiException.ErrorCode} - {apiException.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to send emails: {e.Message}");
            }
        }

        public string GetAppointmentEmailContent(string userName, string instructorEmail, string instructor, string dayOfWeek, string time, string appointmentLocation)
        {
            return "Dear " + userName + ",\n\n" +
                   "Here are your appointment details:\n\n" +
                   "Instructor: " + instructor + "\n" +
                   "Day: " + dayOfWeek + "\n" +
                   "Time: " + time + "\n" +
                   "Location: " + appointmentLocation + "\n" +
                   "Your instructor's email address: " + instructorEmail + "\n\n" +
                   "Thank you for using our service!\n" + "Office Hour Planner";
        }

        public string GetAppointmentInstructorEmailContent(string userName, string userId, string emailAddress, string instructor, string dayOfWeek, string time, string appointmentLocation)
        {
            return "Dear Professor " + instructor + ",\n\n" +
                   "A student wants to make an appointment with you!\n" +
                   "Here are the appointment details:\n\n" +
                   "Student: " + userName + "\n" +
                   "ID: " + userId + "\n" +
                   "Student's email address: " + emailAddress + "\n" +
                   "Day: " + dayOfWeek + "\n" +
                   "Time: " + time + "\n" +
                   "Location: " + appointmentLocation + "\n\n" + // Add location information
                   "Thank you for using our service!\n" + "Office Hour Planner";
        }
    }
}
