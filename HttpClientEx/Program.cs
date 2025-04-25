using System.Text;
using System.Text.Json;

namespace HttpClientEx;
class Program
{
    static async Task Main(string[] args)
    {
        //1.Get with query parameters
        string id = "302345749";
        string year = "2001";

        using var client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:9514")
        };
        var endpoint = $"/test_get_method?id={id}&year={year}";
        var response = await client.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        var contentOfGet = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response: {contentOfGet}");
        
        //2.Post - get data to server using a json
        var payload = new
        {
            id = int.Parse(id),
            year = int.Parse(year),
            contentFromGet = contentOfGet
        };
        string json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var postResponse = await client.PostAsync("/test_post_method", content);
        postResponse.EnsureSuccessStatusCode();
        string postResponseRaw = await postResponse.Content.ReadAsStringAsync();
        Console.WriteLine(postResponseRaw);
        
        //get the message from the json response
        using var postjsonDoc = JsonDocument.Parse(postResponseRaw);
        string postMessage = postjsonDoc.RootElement.GetProperty("message").GetString()
                             ?? throw new Exception("'message not found in response'");
        
        //3. Put - update data on the server
        int originalId = int.Parse(id);
        int originalYear = int.Parse(year);
        int idForPut = (originalId - 145987) % 34;
        int yearForPut = (originalYear + 785) % 62;
        var payloadForPut = new
        {
            id = idForPut,
            year = yearForPut,
        };
        string jsonForPut = JsonSerializer.Serialize(payloadForPut);
        using var contentForPut = new StringContent(jsonForPut, Encoding.UTF8, "application/json");
        var putResponse = await client.PutAsync($"/test_put_method?id={postMessage}", contentForPut);
        putResponse.EnsureSuccessStatusCode();
        string putResponseRaw = await putResponse.Content.ReadAsStringAsync();
        Console.WriteLine(putResponseRaw);
        //get the message from the json
        using var putjsonDoc = JsonDocument.Parse(putResponseRaw);
        string putMessage = putjsonDoc.RootElement.GetProperty("message").GetString()
                            ?? throw new Exception("'message not found in response'");
        
        //4. delete resource
        await client.DeleteAsync($"/test_delete_method?id={putMessage}");
    }
}
