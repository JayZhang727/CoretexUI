using CoretexUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CoretexUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration Configuration;
        private readonly string ApiService_GetAllVehicles_Url;
        private readonly string ApiService_GetOneVehicle_Url;
        private readonly string ApiService_UpsertVehicle_Url;
        private readonly string ApiService_DeleteVehicle_Url;
        private static HttpClient _client = new HttpClient();

        [BindProperty]
        public Vehicle Vehicle { get; set; }

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
            ApiService_GetAllVehicles_Url = Configuration["Services:ServiceUrl_GetAll"];
            ApiService_GetOneVehicle_Url = Configuration["Services:ServiceUrl_GetOne"];
            ApiService_UpsertVehicle_Url = Configuration["Services:ServiceUrl_Upsert"];
            ApiService_DeleteVehicle_Url = Configuration["Services:ServiceUrl_Delete"];
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Privacy()
        {
            var serviceString = Configuration["Services:ServiceUrl"];
            var uri = new Uri(serviceString);

            var response = await _client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content.ReadAsStringAsync().Result);
            }
            else
            {
                return StatusCode(500, "Server error");
            }
        }



        #region API Calls
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var uri = new Uri(ApiService_GetAllVehicles_Url);
            var response = await _client.GetAsync(uri);
            var stringData = await response.Content.ReadAsStringAsync();
            var result = Json(new { data = JsonConvert.DeserializeObject(stringData) });

            return result;
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            Vehicle = new Vehicle();
            if (id == null)
            {
                //create
                return View(Vehicle);
            }
            //update
            var uri = new Uri($"{ApiService_GetOneVehicle_Url}/{id}");
            var response = _client.GetAsync(uri).Result;
            var stringData = response.Content.ReadAsStringAsync().Result;
            Vehicle = JsonConvert.DeserializeObject<Vehicle>(stringData);
            if (Vehicle == null)
            {
                return NotFound();
            }
            return View(Vehicle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert()
        {
            if (ModelState.IsValid)
            {
                var uri = new Uri(ApiService_UpsertVehicle_Url);

                var msg = new HttpRequestMessage(HttpMethod.Put, uri);
                var json = JsonConvert.SerializeObject(Vehicle);
                msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.SendAsync(msg);

                return RedirectToAction("Index");
            }
            return View(Vehicle);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var uri = new Uri($"{ApiService_DeleteVehicle_Url}/{id}");
            var msg = new HttpRequestMessage(HttpMethod.Delete, uri);
            var response = await _client.SendAsync(msg);
            var stringData = await response.Content.ReadAsStringAsync();
            var serverMsg = JsonConvert.DeserializeObject<string>(stringData);
            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Delete successful" });
            }
            else
            {
                return Json(new { success = false, message = serverMsg });
            }
        }
        #endregion

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private object DeserializeAPIResponse(HttpResponseMessage response)
        {
            var stringData = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject(stringData);
            return result;
        }
    }
}
