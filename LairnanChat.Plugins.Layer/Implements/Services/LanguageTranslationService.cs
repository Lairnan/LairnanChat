using LairnanChat.Plugins.Layer.Interfaces.Services;

namespace LairnanChat.Plugins.Layer.Implements.Services;

public class LanguageTranslationService : ILanguageTranslationService
{
    public Task<string> TranslateAsync(string text, string fromLanguage, string toLanguage)
    {
        return Task.FromResult(text);
    }
}