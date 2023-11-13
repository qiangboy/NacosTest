using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Nacos.V2;
using RestSharp;

namespace NacosTest.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly INacosNamingService _nacosNamingService;

    public TestController(INacosNamingService nacosNamingService)
    {
        _nacosNamingService = nacosNamingService;
    }

    [HttpGet]
    public IActionResult GetSelectedList()
    {
        return Ok("请求成功");
    }

    [HttpGet("real-url")]
    public async Task<IActionResult> GetRealUrl()
    {
        // need to know the service name.
        var instance = await _nacosNamingService.SelectOneHealthyInstance("MS.ILCS", "dev");
        var host = $"{instance.Ip}:{instance.Port}";

        var baseUrl = instance.Metadata.TryGetValue("secure", out _)
            ? $"https://{host}"
            : $"http://{host}";

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return Ok("empty");
        }

        var realUrl = $"{baseUrl}/api/Test/GetSelectedList";

        using var client = new HttpClient();
        var result = await client.GetAsync(realUrl);

        var getReq = await result.Content.ReadAsStringAsync();

        return Ok($"请求的真实地址：{baseUrl}，调用地址：{realUrl}，调用结果：{getReq}");
    }


    [HttpGet]
    public IActionResult Do(string url)
    {
        _ = Do(0, url);

        return Ok();
    }

    private async Task Do(int i, string realUrl)
    {
        var counter = 0;
        var sw2 = Stopwatch.StartNew();
        var sw = Stopwatch.StartNew();
        while (true)
        {
            await Task.Delay(100);
            sw.Restart();

            using RestClient client = new(realUrl);
            RestRequest request = new();
            RestResponse response = await client.ExecuteGetAsync(request);

            sw.Stop();

            var isSuccessful = response.IsSuccessful ? "成功" : "失败";

            Console.WriteLine($"并发请求 {isSuccessful}  {i}  耗时:{sw.ElapsedMilliseconds} ms -------- 已执行{sw2.ElapsedMilliseconds / 1000}秒 超过300ms的次数:{counter}");

            if (sw.ElapsedMilliseconds > 300)
            {
                counter++;
                Console.WriteLine("监测到耗时超过300ms的请求");
                await Task.Delay(3000);
            }
        }

    }
}