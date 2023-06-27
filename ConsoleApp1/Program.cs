// See https://aka.ms/new-console-template for more information

using System.Net.Http.Json;
using WebApplication1.Entities;

HttpClient client = new();
client.BaseAddress = new Uri("https://localhost:7264");
client.DefaultRequestHeaders.Accept.Clear();
client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

HttpResponseMessage response = await client.GetAsync("api/issue");
response.EnsureSuccessStatusCode();

if (response.IsSuccessStatusCode)
{
    var issues = await response.Content.ReadFromJsonAsync < IEnumerable <Issue>> ();
    foreach(var issue in issues)
    {
        Console.WriteLine(issue.TenantId);
    }
}
else
{
    Console.WriteLine("No Results");
}
Console.ReadLine();