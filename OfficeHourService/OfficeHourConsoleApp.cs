using System;
using System.Net.Http;
using System.Threading.Tasks;
/
namespace OfficeHourConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Write("Enter the path to the input text file: ");
            string filePath = Console.ReadLine();

            // Send the file path to your web API using HttpClient
            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync($"https://localhost:5001/office-hours?filePath={filePath}");
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                }
            }
        }
    }
}
