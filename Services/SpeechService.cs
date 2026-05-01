using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace AIComplaintAgent.Services;

public class SpeechService
{
    private readonly IConfiguration _config;

    public SpeechService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<string?> RecognizeSpeechAsync()
    {
        var key = _config["AzureSpeech:Key"];
        var region = _config["AzureSpeech:Region"];

        var speechConfig = SpeechConfig.FromSubscription(key!, region!);

        // Auto-detect language
        var autoDetectConfig = AutoDetectSourceLanguageConfig.FromLanguages(
            new[] { "en-US", "ar-SA", "fr-FR", "es-ES" });

        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        using var recognizer = new SpeechRecognizer(
            speechConfig,
            autoDetectConfig,
            audioConfig);

        var result = await recognizer.RecognizeOnceAsync();

        return result.Reason == ResultReason.RecognizedSpeech
            ? result.Text
            : null;
    }
}