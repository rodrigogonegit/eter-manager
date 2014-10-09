using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EterManager.Models
{
    public enum IssueSeverity
    {
        Message,
        Warning,
        Error
    }

    class Issue : IEquatable<Issue>
    {
        #region Constructors

        public Issue(IssueSeverity severity, string description, string context)
        {
            Severity = severity;
            Description = description;
            Context = context;
        }

        #endregion

        #region Properties

        public IssueSeverity Severity { get; set; }

        public string Description { get; set; }

        public string Context { get; set; }

        #endregion

        #region IEquatable Members

        public bool Equals(Issue other)
        {
            return Severity == other.Severity
                   && Description == other.Description
                   && Context == other.Context;
        }

        #endregion
    }
}
