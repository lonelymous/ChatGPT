using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;

internal static class Program
{
    private static bool DEBUG = false;
    private static string? apiKey;
    private static async Task Main(string[] args)
    {
        if (args.Length > 0)
        {
            if (args.Length > 1 && args[1] == "debug")
            {
                DEBUG = true;
            }
            string root = Directory.GetCurrentDirectory();
            Log(root);
            LoadEnviromentVariable();
            apiKey = Environment.GetEnvironmentVariable("APIKEY");

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {apiKey}");

            HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/completions", 
            new StringContent("{\"model\": \"text-davinci-001\", \"prompt\": \"" + args[0] + "\", \"temperature\": 1, \"max_tokens\": 100}", Encoding.UTF8, "application/json"));
        
            string responseString = await response.Content.ReadAsStringAsync();
            try
            {
                var dynamicData = JsonConvert.DeserializeObject<dynamic>(responseString);
                Console.WriteLine("The response is:");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(dynamicData!.choices[0].text);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error while deserialize the JSON");
                Console.WriteLine("Error message: ", ex.Message);
            } 
            finally
            {
                Console.ResetColor();
            }
        }
        else
        {
            Console.WriteLine("No arguments were passed to the program.");
        }
    }

    public static void Log(string message)
    {
        if (DEBUG)
        {
            Console.WriteLine(message);
        }
    }

    public static void LoadEnviromentVariable()
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        Log("Loading environment variables from " + filePath);
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found");
            do
            {
                Console.Write("Do you want to login or give an api key? (login/key): ");
                switch (Console.ReadLine())
                {
                    case "login":
                        Process.Start(new ProcessStartInfo("https://platform.openai.com/account/api-keys") { UseShellExecute = true });
                        break;
                    case "key":
                        Console.Write("Give me an API key: ");
                        string? apiKey = Console.ReadLine();

                        if (apiKey == null)
                        {
                            Console.WriteLine("Invalid key");
                            continue;
                        }
                        
                        apiKey = apiKey.Trim();

                        Console.Write("Do you want to save the key to a .env file? (y/n): ");
                        if (Console.ReadLine() == "y")
                        {
                            Log("Saving token to .env file");
                            File.WriteAllText(filePath, "APIKEY=" + apiKey);
                        }
                        Environment.SetEnvironmentVariable("APIKEY", apiKey);
                        return;
                    default:
                        break;
                }
            } while (true);
        }

        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split('=',StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
            {
                Log("Invalid line: " + line);
                continue;
            }
            Log("Setting " + parts[1] + " to " + parts[0]);
            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }
}
