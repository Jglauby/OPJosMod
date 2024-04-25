using System.Linq;

namespace OPJosMod.ReviveCompany.CustomRpc
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
            if (string.IsNullOrEmpty(givenString)) return false;

            return givenString.Contains(GetCode(MessageCodes.Request)) || givenString.Contains(GetCode(MessageCodes.Response));
        }

        public static string returnMessageWithoutCode(string message)
        {
            if (MessageCodeUtil.stringContainsMessageCode(message))
            {
                message = message.Replace(GetCode(MessageCodes.Request), string.Empty);
                message = message.Replace(GetCode(MessageCodes.Response), string.Empty);
            }

            return message;
        }

        public static string returnMessageNoSeperators(string message)
        {
            if (MessageCodeUtil.stringContainsMessageCode(message))
            {
                string withoutColons = new string(message.Where(c => c != ':').ToArray());
                return withoutColons;
            }

            return message;
        }
    }
}
