using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Wsapm.Core
{
    /// <summary>
    /// Class to execute actions after policy check.
    /// </summary>
    public sealed class ActionAfterPolicyCheckManager
    {
        public static void ExecuteActionsAfterPolicyCheckNoPolicySatisfied(WsapmSettings settings)
        {
            if (settings == null || !settings.EnableActionsAfterPolicyCheck || settings.ActionsAfterPolicyCheck == null || settings.ActionsAfterPolicyCheck.Count == 0)
                return;

            foreach (var action in settings.ActionsAfterPolicyCheck)
            {
                if(action.ActionModeAfterPolicyCheck == ActionModeAfterPolicyCheck.NoPolicySatisfied)
                    ExecuteAction(action);
            }
        }

        public static void ExecuteActionsAfterPolicyCheckAtLeastPolicySatisfied(WsapmSettings settings)
        {
            if (settings == null || !settings.EnableActionsAfterPolicyCheck || settings.ActionsAfterPolicyCheck == null || settings.ActionsAfterPolicyCheck.Count == 0)
                return;

            foreach (var action in settings.ActionsAfterPolicyCheck)
            {
                if (action.ActionModeAfterPolicyCheck == ActionModeAfterPolicyCheck.AtLeastOnePolicySatisfied)
                    ExecuteAction(action);
            }
        }

        private static void ExecuteAction(ActionAfterPolicyCheck action)
        {
            if (action == null)
                return;

            WsapmLog.Log.WriteLine(string.Format(Resources.Wsapm_Core.ActionAfterPolicyCheckManager_ExecutingAction, action.ToString()), LogMode.Normal);

            switch (action.ActionTypeAfterPolicyCheck)
            {
                case ActionTypeAfterPolicyCheck.Standby:
                    ShutdownManager.SetStandbyState();
                    break;
                case ActionTypeAfterPolicyCheck.Hibernate:
                    ShutdownManager.SetHibernateState();
                    break;
                case ActionTypeAfterPolicyCheck.Shutdown:
                    ShutdownManager.ShutDown();
                    break;
                case ActionTypeAfterPolicyCheck.StartProgram:
                    ExecuteProgram(action);
                    break;
                default:
                    break;
            }
        }

        private static void ExecuteProgram(ActionAfterPolicyCheck action)
        {
            if (action.ProgramStart != null)
                ProgramManager.StartPrograms(new[] { action.ProgramStart });
        }       
    }
}
