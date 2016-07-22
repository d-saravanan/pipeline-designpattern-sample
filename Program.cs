using PipelinePattern.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelinePattern
{
    class Program
    {
        static void Main(string[] args)
        {
            var token = new System.Threading.CancellationToken();
            new PipelinePattern.Services.ProfileService().AddProfile(@"C:\Users\Saran\documents\visual studio 2013\Projects\WinFormApps\PipelinePattern\PipelinePattern\SampleInputs.txt", token);

            InMemoryProfileStore.HardPersist();

            Console.ReadKey();
        }
    }
}
