// See https://aka.ms/new-console-template for more information

try
{
    int pid = int.Parse(args[0]);
    string name = args[1];
    long starttime = long.Parse(args[2]);
    int timeout = int.Parse(args[3]);

    Console.WriteLine($"{pid} : {name} {starttime} : {timeout}");
    Thread.Sleep(timeout);

    var process = System.Diagnostics.Process.GetProcessById(pid);
    
    if (process is null)
    {
        return;
    }

    if (process.ProcessName != name)
    {
        throw new ApplicationException($"Wrong process name: {process.ProcessName}");
    }

    if (process.StartTime.ToFileTime() != starttime)
    {
        throw new ApplicationException($"Wrong process start time: {process.StartTime}");
    }
    
    process.Kill();
    Console.WriteLine("killed");
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}


