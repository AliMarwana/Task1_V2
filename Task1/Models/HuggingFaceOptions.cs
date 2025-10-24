namespace Task1.Models
{
    public class HuggingFaceOptions
    {
        public const string SectionName = "HuggingFace";

        public string ApiKey { get; set; } = string.Empty;
        public string ModelName { get; set; } = "microsoft/DialoGPT-large";
        public string Endpoint { get; set; } = "https://api-inference.huggingface.co/models/";
    }
}
