namespace ChatWithAKnowledgeBase;
internal class WebPageImporter
{
    public WebPageImporter()
    {
    }

    internal async Task<string> Import(string webUrl)
    {
        // download the web page
        return await new HttpClient().GetStringAsync(webUrl);
    }
}