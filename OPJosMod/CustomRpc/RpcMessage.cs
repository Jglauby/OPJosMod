namespace OPJosMod.MODNAMEHERE.CustomRpc
{
    public class RpcMessage
    {
        public RpcMessage(string message, int user, MessageCodes code)
        {
            Message = message;
            FromUser = user;
            MessageCode = code;
        }

        public string Message { get; set; }

        public int FromUser { get; set; }

        public MessageCodes MessageCode { get; set; }

        public string getMessageWithCode()
        {
            string result = MessageCodeUtil.GetCode(MessageCode) + Message;
            return result;
        }
    }
}
