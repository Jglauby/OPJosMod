using System;

namespace OPJosMod.ReviveCompany.CustomRpc
{
    public enum MessageTasks 
    {
        ModActivated,
        RevivePlayer,
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
                case MessageTasks.RevivePlayer:
                    return ":RevivePlayer:";
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
            else if (givenString.Contains(GetCode(MessageTasks.RevivePlayer))){
                return MessageTasks.RevivePlayer;
            }
            else
            {
                return MessageTasks.ErrorNoTask;
            }
        }
    }
}
