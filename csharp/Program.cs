using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;

internal static class Program
{
    private static bool DEBUG = false;
    private static string? apiKey;


    private static void CheckArgs(string arg)
    {
        if (arg == "debug")
        {
            DEBUG = true;
        }
        else if (arg == "path")
        {
            UpdatePath();
        }
    }
    private static async Task Main(string[] args)
    {
        if (args.Length > 0)
        {
            if (args.Length > 1)
            {
                foreach (var arg in args)
                {
                    CheckArgs(arg);
                }
            }
            await Ask(args[0]);
        }
        else
        {
            Console.Title = "OpenAI C# CLI";
            Console.WriteLine("Welcome to the OpenAI C# CLI");
            while (true)
            {
                Console.Write("> ");
                string question = Console.ReadLine()!.Trim();
                if (question == "") continue;
                else if (question == "exit" | question == "quit" | question == "q") break;
                else if (question == "dkey")
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), ".env");
                    Log("Deleting " + path);
                    File.Delete(path);
                    Console.WriteLine("Deleted the key");
                    continue;
                }
                else if (question == "debug")
                {
                    DEBUG = !DEBUG;
                    Console.WriteLine("Debug mode is now " + (DEBUG ? "on" : "off"));
                    continue;
                }
                else if (question == "path")
                {
                    UpdatePath();
                    Console.WriteLine("Path updated");
                    continue;
                }
                await Ask(question!);
                GC.Collect();
            }
        }
    }
    private static async Task Ask(string question)
    {
        string root = Directory.GetCurrentDirectory();
        Log(root);
        LoadEnviromentVariable();
        apiKey = Environment.GetEnvironmentVariable("APIKEY");

        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("authorization", $"Bearer {apiKey}");

        HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/completions", 
        new StringContent("{\"model\": \"text-davinci-001\", \"prompt\": \"" + question + "\", \"temperature\": 1, \"max_tokens\": 100}", Encoding.UTF8, "application/json"));
    
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
    private static void UpdatePath()
    {
        string name = "PATH";
        EnvironmentVariableTarget scope = EnvironmentVariableTarget.Machine;
        string? oldValue = Environment.GetEnvironmentVariable(name, scope);
        if (oldValue!.Contains(Directory.GetCurrentDirectory()))
        {
            return;
        }
        try
        {
            Log("Updating path with " + Directory.GetCurrentDirectory());
            Environment.SetEnvironmentVariable(name, oldValue + @";" + Directory.GetCurrentDirectory(), scope);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex);
            Environment.SetEnvironmentVariable(name, oldValue!, scope);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Maybe you need to run this program as administrator");
            Console.ResetColor();
        }
    }
    private static void Log(string message)
    {
        if (DEBUG)
        {
            Console.WriteLine(message);
        }
    }
    private static void LoadEnviromentVariable()
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
