using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web; 

namespace Rcx
{
    class Program
    {
        static void Main(string[] args)
        {
            // Step 1 Create a URI to serve as the base address.
           // Uri baseUri = new Uri("http://localhost:8789/Rcx/");

           // WebServiceHost sh = new WebServiceHost(typeof(RcxService), baseUri);

           // sh.Open();
            Console.WriteLine("The service is ready.");
            Console.WriteLine("Press <ENTER> to terminate service.");
            Console.WriteLine();
            Console.ReadLine();

            // Close the ServiceHostBase to shutdown the service.
           // sh.Close();
        }
    }
}