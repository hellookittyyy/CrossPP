using System;
using System.Runtime.InteropServices;

// Встановлюємо кодування UTF-8
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