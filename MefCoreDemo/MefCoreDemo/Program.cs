namespace MefCoreDemo
{
    using System;
    using System.Composition.Convention;
    using System.Composition.Hosting;
    using System.IO;
    using System.Reflection;
    using MefDemo.Contracts;

    /// <summary>
    /// Main class.
    /// </summary>

    public class MainClass
    {
        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static void Main(string[] args)
        {

            var e = new ExportClass();
            e.Process(null);
        }
    }
}
