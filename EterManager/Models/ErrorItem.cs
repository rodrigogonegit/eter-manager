using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EterManager.Models
{
    class ErrorItem
    {
        public ErrorItem(string filename, string errorMotive, object arg = null)
        {
            this.Filename = filename;
            this.ErrorMotive = errorMotive;
            this.Arg = arg;
        }

        public string Filename { get; set; }

        public string ErrorMotive { get; set; }

        public object Arg { get; set; }
    }
}
