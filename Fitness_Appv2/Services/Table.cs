using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fitness_Appv2.Services
{
    abstract public class Table
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        // This is a property, {get; set;} is just shorthand for a method that retrieves/allocates the value of the property
    }
}
