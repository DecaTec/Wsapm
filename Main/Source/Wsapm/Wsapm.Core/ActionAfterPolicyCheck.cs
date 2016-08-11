using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Wsapm.Core
{
    /// <summary>
    /// Class representing an action after policy check.
    /// </summary>
    [Serializable]
    public class ActionAfterPolicyCheck
    {
        /// <summary>
        /// Creates a new instance of ActionAfterPolicyCheck.
        /// </summary>
        public ActionAfterPolicyCheck()
        {
        }

        /// <summary>
        /// Gets or sets the action mode.
        /// </summary>
        public ActionModeAfterPolicyCheck ActionModeAfterPolicyCheck
        {
            get;
            set;
        }

        [XmlIgnore]
        public string ActionModeAfterPolicyCheckUi
        {
            get
            {
                if (this.ActionModeAfterPolicyCheck == Core.ActionModeAfterPolicyCheck.AtLeastOnePolicySatisfied)
                    return Resources.Wsapm_Core.ActionAfterPolicyCheck_ActionModeAtLeastOnePolicySatisfied;
                else if (this.ActionModeAfterPolicyCheck == Core.ActionModeAfterPolicyCheck.NoPolicySatisfied)
                    return Resources.Wsapm_Core.ActionAfterPolicyCheck_ActionModeNoPolicySatisfied;
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the type of action after policy check.
        /// </summary>
        public ActionTypeAfterPolicyCheck ActionTypeAfterPolicyCheck
        {
            get;
            set;
        }

        [XmlIgnore]
        public string ActionTypeAfterPolicyCheckUi
        {
            get
            {
                if (this.ActionTypeAfterPolicyCheck == Core.ActionTypeAfterPolicyCheck.Hibernate)
                    return Resources.Wsapm_Core.ActionAfterPolicyCheck_ActionNameHibernate;
                else if (this.ActionTypeAfterPolicyCheck == Core.ActionTypeAfterPolicyCheck.Shutdown)
                    return Resources.Wsapm_Core.ActionAfterPolicyCheck_ActionNameShutdown;
                else if (this.ActionTypeAfterPolicyCheck == Core.ActionTypeAfterPolicyCheck.Standby)
                    return Resources.Wsapm_Core.ActionAfterPolicyCheck_ActionNameStandby;
                else if (this.ActionTypeAfterPolicyCheck == Core.ActionTypeAfterPolicyCheck.StartProgram)
                    return Resources.Wsapm_Core.ActionAfterPolicyCheck_ActionNameStartProgram + " (" + this.ProgramStart.ToString() + ")";
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the program start.
        /// </summary>
        public ProgramStart ProgramStart
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the string representation of the ActionAfterPolicyCheck.
        /// </summary>
        /// <returns>The string representation of the ActionAfterPolicyCheck.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            switch (this.ActionModeAfterPolicyCheck)
            {
                case ActionModeAfterPolicyCheck.AtLeastOnePolicySatisfied:
                    sb.Append(Resources.Wsapm_Core.ActionAfterPolicyCheck_ActionModeAtLeastOnePolicySatisfied);
                    break;
                case ActionModeAfterPolicyCheck.NoPolicySatisfied:
                    sb.Append(Resources.Wsapm_Core.ActionAfterPolicyCheck_ActionModeNoPolicySatisfied);
                    break;
                default:
                    break;
            }

            sb.Append(" → ");

            switch (this.ActionTypeAfterPolicyCheck)
            {
                case ActionTypeAfterPolicyCheck.Standby:
                    sb.Append(Resources.Wsapm_Core.ActionAfterPolicyCheck_ActionNameStandby);
                    break;
                case ActionTypeAfterPolicyCheck.Hibernate:
                    sb.Append(Resources.Wsapm_Core.ActionAfterPolicyCheck_ActionNameHibernate);
                    break;
                case ActionTypeAfterPolicyCheck.Shutdown:
                    sb.Append(Resources.Wsapm_Core.ActionAfterPolicyCheck_ActionNameShutdown);
                    break;
                case ActionTypeAfterPolicyCheck.StartProgram:
                    sb.Append(Resources.Wsapm_Core.ActionAfterPolicyCheck_ActionNameStartProgram);
                    sb.Append(": ");

                    if (this.ProgramStart != null)
                        sb.Append(this.ProgramStart.ToString());
                    break;
                default:
                    break;
            }

            return sb.ToString();
        }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            ActionAfterPolicyCheck other = obj as ActionAfterPolicyCheck;
            return this.Equals(other);
        }

        public bool Equals(ActionAfterPolicyCheck other)
        {
            if (other == null)
                return false;

            return this.GetHashCode() == other.GetHashCode();
        }

        public static bool operator ==(ActionAfterPolicyCheck a, ActionAfterPolicyCheck b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.GetHashCode() == b.GetHashCode();
        }

        public static bool operator !=(ActionAfterPolicyCheck a, ActionAfterPolicyCheck b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            var hashCode = this.ActionModeAfterPolicyCheck.ToString().GetHashCode() ^ this.ActionTypeAfterPolicyCheck.ToString().GetHashCode();

            if (this.ProgramStart != null)
                hashCode = hashCode ^ this.ProgramStart.GetHashCode();

            return hashCode;
        }

        #endregion Equals

        /// <summary>
        /// Copies the object.
        /// </summary>
        /// <returns></returns>
        public ActionAfterPolicyCheck Copy()
        {
            ActionAfterPolicyCheck action = new ActionAfterPolicyCheck();
            action.ActionModeAfterPolicyCheck = this.ActionModeAfterPolicyCheck;
            action.ActionTypeAfterPolicyCheck = this.ActionTypeAfterPolicyCheck;

            if(this.ProgramStart != null)
                action.ProgramStart = this.ProgramStart.Copy();

            return action;
        }
    }

    public enum ActionModeAfterPolicyCheck
    {
        AtLeastOnePolicySatisfied,
        NoPolicySatisfied
    }

    /// <summary>
    /// Enum representing the action type after policy check.
    /// </summary>
    public enum ActionTypeAfterPolicyCheck
    {
        Standby,
        Hibernate,
        Shutdown,
        StartProgram
    }
}
