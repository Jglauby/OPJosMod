using System;

namespace OPJosMod.GhostMode.CustomRpc
{
    public enum MessageTasks 
    {
        ModActivated,
        TurnOffGhostMode,
        ErrorNoTask
    }

    public static class MessageTaskUtil
    {
        public static string GetCode(MessageTasks code)
        {
            switch (code)
            {
                case MessageTasks.ModActivated:
                    return ":ModActivated:";
                case MessageTasks.TurnOffGhostMode:
                    return ":TurnOffGhostMode:";
                case MessageTasks.ErrorNoTask:
                    return ":Error:";
            }

            return ":Error:";
        }

        public static string getMessageWithoutTask(string message)
        {
            foreach (MessageTasks task in Enum.GetValues(typeof(MessageTasks)))
            {
                string code = GetCode(task);
                message = message.Replace(code, "");
            }

            return message.Trim();
        }

        public static MessageTasks getMessageTask(string givenString)
        {
            if (givenString.Contains(GetCode(MessageTasks.ModActivated)))
            {
                return MessageTasks.ModActivated;
            }
            else if (givenString.Contains(GetCode(MessageTasks.TurnOffGhostMode))){
                return MessageTasks.TurnOffGhostMode;
            }
            else
            {
                return MessageTasks.ErrorNoTask;
            }
        }
    }
}
