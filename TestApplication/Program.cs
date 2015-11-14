
namespace TestApplication
{
    using System;

    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hi, I'm on {Version.help} and {this} is not a {Version}");
            Console.ReadKey();
        }
    }
}
