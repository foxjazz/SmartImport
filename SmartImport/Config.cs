﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SmartImport
{
    public class Config
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public string Desto { get; set; }

        public DateTime? LastRun { get; set; }

    }
}