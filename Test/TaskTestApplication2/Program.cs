namespace TaskTestApplication
{
    using System;

    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hi, I'm app2 on {Version.CommitShortID} and {this} is not a {Version}");
            Console.ReadKey();
        }
    }
}
