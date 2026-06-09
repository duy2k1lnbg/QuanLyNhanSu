namespace Bu.Services.AI_Services.Interfaces
{
    public interface IPromptManager
    {
        string GetPrompt(string key);
        string GetSchema();
    }
}
