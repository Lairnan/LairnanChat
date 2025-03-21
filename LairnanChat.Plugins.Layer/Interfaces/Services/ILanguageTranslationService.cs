namespace LairnanChat.Plugins.Layer.Interfaces.Services;

public interface ILanguageTranslationService
{
    /// <summary>
    /// Translates the specified text from the source language to the target language.
    /// </summary>
    /// <param name="text">The text to translate.</param>
    /// <param name="fromLanguage">The original language of the text.</param>
    /// <param name="toLanguage">The target language for translation.</param>
    /// <returns>A task that represents the asynchronous translation operation. The task result contains the translated text.</returns>
    Task<string> TranslateAsync(string text, string fromLanguage, string toLanguage);
}