namespace OPJosMod.HideNSeek.CustomRpc
{
    public enum MessageTasks 
    {
        StartedSeeking,
        ErrorNoTask
    }

    public static class MessageTaskUtil
    {
        public static string GetCode(MessageTasks code)
        {
            switch (code)
            {
                case MessageTasks.StartedSeeking:
                    return ":StartedSeeking:";
                case MessageTasks.ErrorNoTask:
                    return ":Error:";
            }

            return ":Error:";
        }

        public static MessageTasks getMessageTask(string givenString)
        {
            if (givenString.Contains(GetCode(MessageTasks.StartedSeeking))){
                return MessageTasks.StartedSeeking;
            }
            else
            {
                return MessageTasks.ErrorNoTask;
            }
        }
    }
}
