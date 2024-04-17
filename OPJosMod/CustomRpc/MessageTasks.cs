using System;

namespace OPJosMod.OneHitShovel.CustomRpc
{
    public enum MessageTasks
    {
        ModActivated,
        EnemyDied,
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
                case MessageTasks.EnemyDied:
                    return ":EnemyDied:";
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
            else if (givenString.Contains(GetCode(MessageTasks.EnemyDied)))
            {
                return MessageTasks.EnemyDied;
            }
            else
            {
                return MessageTasks.ErrorNoTask;
            }
        }
    }
}
