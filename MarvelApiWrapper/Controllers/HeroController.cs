using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MarvelApiWrapper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeroController : ControllerBase
    {
        // GET: api/Hero/5
        [HttpGet]
        [Route("get-user-favorite-hero-description/{id}")]
        public string Get(int id, [FromServices]IConfiguration config)
        {
            Character personagem;
            string characterName;
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    string ts = DateTime.Now.Ticks.ToString();

                    HttpResponseMessage response = client.GetAsync($"{config.GetSection("UserAuthAPI:BaseURL").Value}api/Users/{id}/favorite-hero").Result;
                    response.EnsureSuccessStatusCode();
                    string content =
                        response.Content.ReadAsStringAsync().Result;

                    dynamic resultado = JsonConvert.DeserializeObject(content);

                    characterName = resultado;
                }

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    string ts = DateTime.Now.Ticks.ToString();
                    string publicKey = config.GetSection("MarvelComicsAPI:PublicKey").Value;
                    string hash = GerarHash(ts, publicKey,
                        config.GetSection("MarvelComicsAPI:PrivateKey").Value);

                    HttpResponseMessage response = client.GetAsync(
                        config.GetSection("MarvelComicsAPI:BaseURL").Value +
                        $"characters?ts={ts}&apikey={publicKey}&hash={hash}&" +
                        $"name={Uri.EscapeUriString(characterName)}").Result;

                    response.EnsureSuccessStatusCode();
                    string content =
                        response.Content.ReadAsStringAsync().Result;

                    dynamic result = JsonConvert.DeserializeObject(content);

                    personagem = new Character();
                    personagem.Description = result.data.results[0].description;
                }

                return personagem.Description;
            }
            catch (Exception)
            {
                return "Não foi possível obter o personagem favorito do usuário";
            }
            
        }

        private string GerarHash(
            string ts, string publicKey, string privateKey)
        {
            byte[] bytes =
                Encoding.UTF8.GetBytes(ts + privateKey + publicKey);
            var gerador = MD5.Create();
            byte[] bytesHash = gerador.ComputeHash(bytes);
            return BitConverter.ToString(bytesHash)
                .ToLower().Replace("-", String.Empty);
        }
    }
}
