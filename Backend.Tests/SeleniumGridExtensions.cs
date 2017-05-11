using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Backend
{
    public static class SeleniumGridExtensions
    {
        public static async Task<SeleniumGridSessionsResponse> GetSessionsAsync(this HttpClient client)
        {
            var sessionsAsString = await(await client.GetAsync("http://localhost:4444/wd/hub/sessions")).Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SeleniumGridSessionsResponse>(sessionsAsString);
        }
    }
}
