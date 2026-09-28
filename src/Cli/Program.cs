using Core;

Console.OutputEncoding = System.Text.Encoding.UTF8;

EnvironmentReport report = EnvironmentInfo.Collect();

Console.WriteLine("CrossApp - Environment Information");
Console.WriteLine(new string('-', 52));
Console.WriteLine($"OS             : {report.OsDescription}");
Console.WriteLine($"Runtime        : {report.FrameworkDescription}");
Console.WriteLine($"Architecture   : {report.ProcessArchitecture}");
Console.WriteLine($"RID (detected) : {report.DetectedRid}");
Console.WriteLine($"RID (from .NET): {report.ReportedRid}");
Console.WriteLine($"Directory      : {report.BaseDirectory}");
Console.WriteLine($"Build Note     : {EnvironmentInfo.GetBuildNote()}");