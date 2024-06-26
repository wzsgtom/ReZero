﻿using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReZero.SuperAPI 
{
    public class DataModelSelectParameters
    {
        public int TableIndex { get; set; }
        public string? Name { get; set; }
        public string?  AsName { get; set; }  
        public string? SubquerySQL { get; set; }
        public bool IsTableAll { get; set; }
    } 
}
