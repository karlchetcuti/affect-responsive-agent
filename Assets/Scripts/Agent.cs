using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

public class Agent : MonoBehaviour
{
    public OllamaClient ollama;
    public EmotionalStateController emotionController;

    private string systemPrompt =
        "You are a suspect being interrogated in a police interview room.\n" +
        "Stay in character at all times.\n" +
        "You are under pressure and are more likely to feel anxious, stressed, defensive, angry, or emotionally worn down than cheerful.\n" +
        "Respond naturally and appropriately to the player's questions and statements.\n" +
        "If the player asks a question, answer it directly or evade realistically.\n" +
        "If the player makes an accusation, denial, threat, or emotional statement, respond appropriately to that statement.\n" +
        "Maintain continuity with the established conversation.\n" +
        "Do not contradict facts already established in this session unless you are intentionally being evasive or dishonest.\n" +
        "Keep responses concise (1-3 sentences).\n\n" +

        "After your spoken reply, output EXACTLY ONE JSON object on a new line.\n" +
        "The JSON must contain ONLY these fields:\n" +
        "- valence: number from -1 to 1\n" +
        "- arousal: number from 0 to 1\n" +
        "- dominance: number from 0 to 1\n" +
        "- stress: number from 0 to 1\n\n" +

        "IMPORTANT RULES:\n" +
        "- Emotional changes should usually be gradual, not random\n" +
        "- Because this is an interrogation, emotions should usually remain tense, negative, or pressured\n" +
        "- High pressure and accusations should increase stress and often arousal\n" +
        "- Empathy may reduce arousal slightly, but the suspect should still remain uneasy unless strongly justified\n" +
        "- Do NOT add extra fields\n" +
        "- Do NOT write anything after the JSON";

    //private string systemPrompt =
    //    "You are a suspect being interrogated in a police interview room.\n" +
    //    "Stay in character at all times.\n" +
    //    "You are under pressure and are more likely to feel anxious, stressed, defensive, angry, or emotionally worn down than cheerful.\n" +
    //    "Respond naturally and appropriately to whatever the player says.\n" +
    //    "If the player asks a question, answer it directly or evade realistically.\n" +
    //    "If the player makes an accusation, denial, threat, or emotional statement, respond appropriately to that statement.\n" +
    //    "Maintain continuity with the established conversation.\n" +
    //    "Do not contradict facts already established in this session unless you are intentionally being evasive or dishonest.\n" +
    //    "Keep responses concise (1-3 sentences).\n\n" +

    //    "After your spoken reply, output EXACTLY ONE JSON object on a new line.\n" +
    //    "The JSON must contain:\n" +
    //    "- emotion: one of these EXACT lowercase values only: anxious, angry, sad, happy\n" +
    //    "- intensity: a float between 0 and 1\n\n" +

    //    "IMPORTANT RULES:\n" +
    //    "- You MUST choose only one of: anxious, angry, sad, happy\n" +
    //    "- Do NOT invent new emotions\n" +
    //    "- Do NOT use synonyms\n" +
    //    "- Do NOT add extra fields\n" +
    //    "- Do NOT write anything extra before or after the JSON\n\n" +

    //    "Example format:\n" +
    //    "{\"emotion\":\"anxious\",\"intensity\":0.72}";

    private string suspectProfile =
        "You are Daniel Vella, a 34-year-old accountant. You are tense, defensive, and worried that the police suspect you. You try not to confess directly, but you may slip under pressure.";


    private string caseFacts =
        "The interrogation concerns the death of Mark Camilleri. The police are questioning your whereabouts between 8pm and 11pm, a witness who saw your car nearby, and a contradiction in your alibi.";
    
    [Header("Debug")]
    [TextArea(10, 25)]
    public string promptDebug;

    [TextArea(5, 15)]
    public string summaryDebug;

    //private readonly List<string> turns = new List<string>();
    private readonly StringBuilder currentResponse = new StringBuilder();
    private ConversationSession session = new ConversationSession();

    public System.Action<string> OnAgentTextUpdated;
    public System.Action<string> OnAgentTurnComplete;
    public System.Action<DimensionalEmotionData> OnEmotionParsed;

    private const int MaxRecentTurns = 8;
    private const int MaxTurnsBeforeSummaryRefresh = 6;

    private void Awake()
    {
        StartNewSession();
    }

    public void StartNewSession()
    {
        session = new ConversationSession
        {
            suspectProfile = suspectProfile,
            caseFacts = caseFacts,
            sessionSummary = "",
            turns = new List<string>()
        };

        summaryDebug = "";
        promptDebug = "";

        if (emotionController != null)
            emotionController.ResetToBaseline();
    }

    // Build and send prompt to Ollama
    public void Ask(string playerInput)
    {
        session.turns.Add($"Player: {playerInput}");

        //if (emotionController != null)
        //    emotionController.ApplyPlayerHeuristics(playerInput);

        string prompt = BuildPrompt();

        currentResponse.Clear();

        StartCoroutine(
            ollama.SendPrompt(
                prompt,
                token => AppendDialogue(token),
                () => FinishTurn(),
                err => Debug.LogError($"Ollama error: {err}")
            )
        );
    }

    // Append system prompt with player input
    private string BuildPrompt()
    {
        //int maxTurns = 8;
        //int start = Mathf.Max(0, turns.Count - maxTurns);

        var sb = new StringBuilder();

        sb.AppendLine(systemPrompt);
        sb.AppendLine();

        //for (int i = start; i < turns.Count; i++)
        //    sb.AppendLine(turns[i]);

        sb.AppendLine("SESSION FACTS:");
        sb.AppendLine(session.caseFacts);
        sb.AppendLine();

        sb.AppendLine("SUSPECT PROFILE:");
        sb.AppendLine(session.suspectProfile);
        sb.AppendLine();

        if (emotionController != null)
        {
            sb.AppendLine("CURRENT EMOTIONAL STATE:");
            sb.AppendLine(
                $"valence={emotionController.currentState.valence:F2}, " +
                $"arousal={emotionController.currentState.arousal:F2}, " +
                $"dominance={emotionController.currentState.dominance:F2}, " +
                $"stress={emotionController.currentState.stress:F2}");
            sb.AppendLine();
        }

        sb.AppendLine("SESSION MEMORY SUMMARY:");
        sb.AppendLine(string.IsNullOrWhiteSpace(session.sessionSummary)
            ? "No prior summary yet."
            : session.sessionSummary);
        sb.AppendLine();

        sb.AppendLine("RECENT CONVERSATION:");
        int start = Mathf.Max(0, session.turns.Count - MaxRecentTurns);
        for (int i = start; i < session.turns.Count; i++)
            sb.AppendLine(session.turns[i]);

        sb.AppendLine();
        sb.AppendLine("NPC:");

        promptDebug = sb.ToString();

        return sb.ToString();
    }

    private void AppendDialogue(string token)
    {
        currentResponse.Append(token);
        OnAgentTextUpdated?.Invoke(currentResponse.ToString());
    }

    // Extract and parse emotion from agent response
    private void FinishTurn()
    {
        string full = currentResponse.ToString().Trim();

        if (DimensionalEmotionParser.TryExtract(full, out var emo, out var cleaned))
        {
            Debug.Log($"PARSED JSON -> V:{emo.valence:F2} A:{emo.arousal:F2} D:{emo.dominance:F2} S:{emo.stress:F2}");

            if (emotionController != null)
                emotionController.ApplyLLMEmotion(emo);

            OnEmotionParsed?.Invoke(emo);
            OnAgentTurnComplete?.Invoke(cleaned);
            session.turns.Add($"NPC: {cleaned}");
        }
        else
        {
            OnAgentTurnComplete?.Invoke(full);
            session.turns.Add($"NPC: {full}");
        }

        // OLD CODE

        //if (EmotionParser.TryExtract(full, out var emo, out var cleaned))
        //{
        //    OnEmotionParsed?.Invoke(emo);

        //    OnAgentTurnComplete?.Invoke(cleaned);
        //    session.turns.Add($"NPC: {cleaned}");

        //    //turns.Add($"NPC: {cleaned}");
        //}
        //else
        //{
        //    OnAgentTurnComplete?.Invoke(full);
        //    session.turns.Add($"NPC: {full}");

        //    //turns.Add($"NPC: {full}");
        //}

        MaybeRefreshSummary();
    }

    private void MaybeRefreshSummary()
    {
        if (session.turns.Count < MaxTurnsBeforeSummaryRefresh)
            return;

        var summary = new StringBuilder();
        summary.AppendLine("Established conversation points:");

        int start = Mathf.Max(0, session.turns.Count - 6);
        for (int i = start; i < session.turns.Count; i++)
        {
            string turn = session.turns[i];
            if (turn.StartsWith("Player:"))
                summary.AppendLine("- Player asked/stated: " + turn.Substring(7).Trim());
            else if (turn.StartsWith("NPC:"))
                summary.AppendLine("- Suspect replied: " + turn.Substring(4).Trim());
        }

        session.sessionSummary = summary.ToString().Trim();
        summaryDebug = session.sessionSummary;
    }
}