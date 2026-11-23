// Outside Assets: standalone C# entry point for reproducing the timeline checks.
using System;
using EchoProtocol;

public static class TimelineRunner
{
    public static int Main()
    {
        try { Console.WriteLine(TimelineSelfChecks.Run()); Console.WriteLine(TimelineRegressionChecks.Run()); return 0; }
        catch (Exception exception) { Console.Error.WriteLine(exception); return 1; }
    }
}
