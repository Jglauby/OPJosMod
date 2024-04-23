namespace OPJosMod.GhostMode.CustomRpc
{
    public enum MessageCodes 
    {
        Request,
        Response
    }

    public static class MessageCodeUtil
    {
        public static string GetCode(MessageCodes code)
        {
            switch (code)
            {
                case MessageCodes.Request:
                    return ":rpcRequest:";
                case MessageCodes.Response:
                    return ":rpcResponse:";
            }

            return ":Error:";
        }

        public static bool stringContainsMessageCode(string givenString)
        {
            return givenString.Contains(GetCode(MessageCodes.Request)) || givenString.Contains(GetCode(MessageCodes.Response));
        }

        public static string returnMessageWithoutCode(string message)
        {
            if (MessageCodeUtil.stringContainsMessageCode(message))
            {
                int codeLength = MessageCodeUtil.GetCode(MessageCodes.Request).Length;

                return message.Substring(codeLength);
            }

            return message;
        }
    }
}
