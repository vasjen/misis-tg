using HtmlAgilityPack;
using misis_tg.Models;
using misis_tg.Models.Enum;

namespace misis_tg.Services;

public class ParserService(IHttpClientFactory clientFactory, ILogger<ParserService> logger)
{
    private readonly HttpClient _client = clientFactory.CreateClient("misis");

    public async Task<List<EstimationResult?>> ParseAsync(string url)
    {
        string? content = await GetResponseFromUrl(url);
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(content);

        HtmlNodeCollection? rows = doc.DocumentNode.SelectNodes("//tr");
        List<EstimationResult> results = [];
        if (rows != null)
        {
            foreach (var row in rows)
            {
                var cells = row.SelectNodes("td");
                if (cells != null && cells.Count >= 8 && cells[1].InnerText.Any())
                {
                    string orderNumber = cells[0].InnerText;
                    string regNumber = cells[1].InnerText;
                    string snils = cells[2].InnerText;
                    if (String.IsNullOrEmpty(regNumber))
                    {
                        logger.LogWarning("Регистрационный номер не найден. Ccылка на страницу: " + url);
                        throw new ArgumentException("Регистрационный номер не найден.");
                    }
                    results.Add(new EstimationResult(Int32.Parse(orderNumber),snils, Int32.Parse(regNumber)));
                }
            }
        }
        else
        {
            logger.LogError("Не удалось извлечь данные из HTML.");
        }
        
        return results;
    }
    
    public async Task<List<Direction>> ParsePostgraduateAsync()
    {
        var content = await GetResponseFromUrl("postgraduate/chance-estimation/");
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(content);

        var mainDiv = doc.DocumentNode.SelectSingleNode("//div[@class='main']");
        List<Direction> directions = new ();
        if (mainDiv != null)
        {       
            var links = mainDiv.SelectNodes(".//a");
            if (links != null) 
            { 
                foreach (var link in links) 
                { 
                    try 
                    { 
                        string directionName = link.InnerText; 
                        string directionId = link.GetAttributeValue("href", ""); 
                        int startIndex = directionId.LastIndexOf("=") + 1; 
                        directionId = directionId.Substring(startIndex);
                        directions.Add(new(
                            directions.Any(p => p.Name == directionName) ? BudgetType.Paid : BudgetType.Budget, directionName, directionId, EducationFormat.Postgraduate));
                    }
                    catch (Exception e) 
                    { 
                        logger.LogError(e.Message);
                    }
                }
            }
            logger.LogInformation($"Parsed {directions.Count} directions from {links.Count}.");
        }
        return directions;
    }
    public async Task<List<Direction>> ParseMasterAsync()
    {
        List<Direction> directions = [];
        HtmlDocument doc = new();
        string? content = await GetResponseFromUrl("magistracy/chance-estimation/");
        doc.LoadHtml(content);

        var mainDiv = doc.DocumentNode.SelectNodes("//div[@role='tabpanel']");
        directions.AddRange(ParseFromTable(mainDiv[0], BudgetType.Budget, EducationFormat.Master));
        directions.AddRange(ParseFromTable(mainDiv[1], BudgetType.Direction, EducationFormat.Master));
        directions.AddRange(ParseFromTable(mainDiv[2], BudgetType.Paid, EducationFormat.Master));
        
        return directions;
    }
    public async Task<List<Direction>> ParseBachelorAsync()
    {
        
        string? content = await GetResponseFromUrl("baccalaureate-and-specialties/chance-estimation/");
        HtmlDocument doc = new();
        doc.LoadHtml(content);
        List<Direction> directions = [];
        HtmlNodeCollection? tables = doc.DocumentNode.SelectNodes("//tbody");

        if (tables != null)
        { 
            var direction = ParseFromTable(tables[0], BudgetType.Budget, EducationFormat.Bachelor); 
            directions.AddRange(direction);
            direction = ParseFromTable(tables[1], BudgetType.Direction, EducationFormat.Bachelor); 
            directions.AddRange(direction);
            direction = ParseFromTable(tables[2], BudgetType.Paid, EducationFormat.Bachelor); 
            directions.AddRange(direction);
        }
        else
        {
            logger.LogError($"Не удалось найти таблицы на странице. content: ");
        }

        return directions;
    }

    private async Task<string?> GetResponseFromUrl(string url)
    {
        HttpResponseMessage response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
    

    private List<Direction> ParseFromTable(HtmlNode mainDiv, BudgetType budget, EducationFormat format)
    {
        List<Direction> directions = new ();
        {       
            var links = mainDiv.SelectNodes(".//a");
            if (links != null) 
            { 
                foreach (var link in links) 
                {
                    Console.WriteLine(link.InnerText);
                    try 
                    { 
                        string directionName = link.InnerText; 
                        string directionId = link.GetAttributeValue("href", ""); 
                        int startIndex = directionId.LastIndexOf("=") + 1; 
                        directionId = directionId.Substring(startIndex);
                        directions.Add(new(budget, directionName, directionId, format));
                    }
                    catch (Exception e) 
                    { 
                        logger.LogError(e.Message);
                    }
                }
            }
            logger.LogInformation($"Parsed {directions.Count} directions from {links.Count}.");
        }
        return directions;
    }
    
    
    
}