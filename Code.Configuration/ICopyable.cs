﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Configuration
{
    public interface ICopyable
    {
        void CopyTo(object destObject);
    }
}