using UnityEngine;

namespace VMail.Utils.MessageFile
{
    public class JSONFileGenerator : MonoBehaviour
    {
        public static readonly string JSONName = "msg.json";

        public static void SaveJSONFile(Message message, string dirPath)
        {
            MessageInfo msg = new MessageInfo();

            msg.lastEditedByAuthor = Viewer.Social.ViewerComment.Author;
            msg.lastEditedByDate = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            msg.lastEditedByDevice = SystemInfo.deviceType;

            msg.SetPages(message.pages);

            string json = JsonUtility.ToJson(msg);
            bool success = Tools.SaveJsonFile(dirPath + "/" + JSONName, json);
            if (!success)
            {
                Debug.LogWarning("error on saving...");
                return;
            }
        }

        public static void SaveJSONFile(MessageMobile message, string dirPath)
        {
            MessageInfo messageInfo = new MessageInfo();

            messageInfo.lastEditedByAuthor = Viewer.Social.ViewerComment.Author;
            messageInfo.lastEditedByDate = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            messageInfo.lastEditedByDevice = SystemInfo.deviceType;

            messageInfo.SetPageInfos(message.GetPageInfos());

            string json = JsonUtility.ToJson(messageInfo);
            bool success = Tools.SaveJsonFile(dirPath + "/" + JSONName, json);
            if (!success)
            {
                Debug.LogWarning("error on saving...");
                return;
            }
        }

    }
}