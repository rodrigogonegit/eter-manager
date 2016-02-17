using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EterManager.Exceptions.EterFiles
{
    /// <summary>
    /// Exception thrown when an invalid Eter operation has occured
    /// </summary>
    class InvalidEterOperationException : Exception
    {
        #region Properties

        /// <summary>
        /// What caused the exception
        /// </summary>
        public string ExceptionMessage { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="exceptionMessage"></param>
        public InvalidEterOperationException(string exceptionMessage)
        {
            ExceptionMessage = exceptionMessage;
        }

        #endregion
    }
}
