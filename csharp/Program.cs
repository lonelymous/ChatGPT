using System.Text;
using Newtonsoft.Json;

internal static class Program
{
    private static bool DEBUG = false;
    private static string? accessToken;
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
            DotEnv.Load(Path.Combine(root, ".env"));
            accessToken = Environment.GetEnvironmentVariable("ACCESS_TOKEN");

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {accessToken}");

            HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/completions", 
            new StringContent("{\"model\": \"text-davinci-001\", \"prompt\": \"" + args[0] + "\", \"temperature\": 1, \"max_tokens\": 100}", Encoding.UTF8, "application/json"));
        
            string responseString = await response.Content.ReadAsStringAsync();
            try
            {
                var dynamicData = JsonConvert.DeserializeObject<dynamic>(responseString);
                Console.WriteLine("The response is:\n");
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
}

public static class DotEnv
{
    public static void Load(string filePath)
    {
        Program.Log("Loading environment variables from " + filePath);
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found");
            Console.Write("Give me an Access Token: ");
            string? accessToken = Console.ReadLine();
            Environment.SetEnvironmentVariable("ACCESS_TOKEN", accessToken!);
            return;
        }

        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split('=',StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
            {
                Program.Log("Invalid line: " + line);
                continue;
            }
            Program.Log("Setting " + parts[1] + " to " + parts[0]);
            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }
}