using System.Threading.Tasks;

namespace OPJosMod.HideNSeek.CustomRpc
{
    public class RpcMessage
    {
        public RpcMessage(MessageTasks task, string message, int user, MessageCodes code)
        {
            Task = task;
            Message = message;
            FromUser = user;
            MessageCode = code;
        }

        public MessageTasks Task { get; set; }

        public string Message { get; set; }

        public int FromUser { get; set; }

        public MessageCodes MessageCode { get; set; }

        public string getMessageWithCode()
        {
            string result = MessageCodeUtil.GetCode(MessageCode) + MessageTaskUtil.GetCode(Task) + Message;
            return result;
        }
    }
}
