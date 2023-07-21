﻿// Description: Interim (version 1) IoT bootcamp development project (Remote Data Center Monitoring) prototype
// Skill Level:  university engineering or comp sci ( sophomore and higher)

// device telemetry model class

#nullable disable

namespace viceroy
{
    public class Telem
    {
        public int messageId { get; set; }
        public string deviceId { get; set; }
        public double temperature { get; set; }
        public double pressure { get; set; }
    }
}