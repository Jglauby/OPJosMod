using System;

namespace OPJosMod.HideNSeek.CustomRpc
{
    public enum MessageTasks 
    {
        ModActivated,
        StartedSeeking,
        MakePlayerWhistle,
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
                case MessageTasks.StartedSeeking:
                    return ":StartedSeeking:";
                case MessageTasks.MakePlayerWhistle:
                    return ":MakeWhistle:";
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
            if (givenString.Contains(GetCode(MessageTasks.StartedSeeking))){
                return MessageTasks.StartedSeeking;
            }
            else if (givenString.Contains(GetCode(MessageTasks.MakePlayerWhistle)))
            {
                return MessageTasks.MakePlayerWhistle;
            }
            else if (givenString.Contains(GetCode(MessageTasks.ModActivated)))
            {
                return MessageTasks.ModActivated;
            }
            else
            {
                return MessageTasks.ErrorNoTask;
            }
        }
    }
}
