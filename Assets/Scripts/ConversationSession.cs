using System.Collections.Generic;

[System.Serializable]
public class ConversationSession
{
    public string caseFacts;
    public string suspectProfile;
    public string sessionSummary = "";
    public List<string> turns = new List<string>();

    public void Clear()
    {
        sessionSummary = "";
        turns.Clear();
    }
}