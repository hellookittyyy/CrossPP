using System;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Linq;

Console.OutputEncoding = System.Text.Encoding.UTF8;

if (args.Contains("--json"))
{
var sysInfo = new
{
Student = "Shapovalova Kateryna, group FEI 32",
OSDescription = RuntimeInformation.OSDescription,
OSEnvironment = Environment.OSVersion.ToString(),
ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
NetVersion = Environment.Version.ToString(),
Runtime = RuntimeInformation.FrameworkDescription,
AppDirectory = AppContext.BaseDirectory,
CurrentDirectory = Environment.CurrentDirectory,
Domain = "Library (books, book copies, readers, loans)"
};

string jsonString = JsonSerializer.Serialize(sysInfo);
Console.WriteLine(jsonString);
}
else
{
Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("CrossApp - Cross-platform programming practicum");
Console.WriteLine("Student: Shapovalova Kateryna, group FEI 32");
Console.WriteLine(new string('-', 52));

Console.WriteLine($"OS (OSDescription)   : {RuntimeInformation.OSDescription}");
Console.WriteLine($"OS (Environment)     : {Environment.OSVersion}");
Console.WriteLine($"Process Architecture : {RuntimeInformation.ProcessArchitecture}");
Console.WriteLine($".NET Version (CLR)   : {Environment.Version}");
Console.WriteLine($"Runtime              : {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"Application Directory: {AppContext.BaseDirectory}");
Console.WriteLine($"Current Directory    : {Environment.CurrentDirectory}");

Console.WriteLine(new string('-', 52));
Console.WriteLine("Domain: Library (books, book copies, readers, loans)");
}