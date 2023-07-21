﻿// Description: Interim (version 1) IoT bootcamp development project (Remote Data Center Monitoring) prototype
// Skill Level:  university engineering or comp sci ( sophomore and higher)

// This file is main .exe (client) RDTMS driver entry point
using Microsoft.Extensions.Configuration;

namespace viceroy
{
    public class RTDMS_Driver_Exec
    {
        static void Main(string[] args)
        {
            // Build a config object, using JSON providers.
            // https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            Console.WriteLine("\t** RTDMS IoT driver version 1.0 ** ");
            // instantiate the RTDMS driver object
            // Note: the using ensures the that "dispose is called when the object
            // goes out of scope.  Dispose ensures that plaform resources are freed, not waiting
            // for the garbabge collector
            using RTDMS_Driver driver = new RTDMS_Driver();

            Console.WriteLine("\t\tHVAC using pin {0}", driver.Settings.HVACPin);
            Console.WriteLine("\t\tLED  using pin {0}", driver.Settings.LEDPin);

            // start the RDTMS driver (starts the main driverTask)
            driver.Start();

            // stop the RTDMS driver (this blocks until the driverTask completes when the user exits the menu )
            driver.Stop();
        }
    }
}