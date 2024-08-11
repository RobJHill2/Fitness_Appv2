using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fitness_Appv2.Services
{
    public class testTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string XciseAttribute { get; set; }
        public float RepsAttribute { get; set; }
        public float WeightAttribute { get; set; }
        public DateTime DateAttribute { get; set; }
        public float e1RMaxAttribute { get; set; }
        public testTable()
        {
            System.Diagnostics.Debug.WriteLine("Record Created with WeightAttribute = {0} and RepsAttribute = {1}", Convert.ToString(WeightAttribute), Convert.ToString(RepsAttribute));

        }


    }
}

