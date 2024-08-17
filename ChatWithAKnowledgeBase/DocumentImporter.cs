namespace ChatWithAKnowledgeBase;
internal class DocumentImporter
{
    public DocumentImporter()
    {
    }

    internal async Task<string> Import(string filePath)
    {
        return await File.ReadAllTextAsync(filePath);
    }
}