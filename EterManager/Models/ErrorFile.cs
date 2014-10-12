using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EterManager.Models
{
    class ErrorFile
    {
        public ErrorFile(string filename, string errorMotive)
        {
            this.Filename = filename;
            this.ErrorMotive = errorMotive;
        }

        public string Filename { get; set; }
        public string ErrorMotive { get; set; }
    }
}
