using ClubPadel.DL;
using ClubPadel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ClubPadel.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(UserRepository repository) : ControllerBase
    {
        private static readonly string _botToken = Environment.GetEnvironmentVariable("TgBotToken");
        private readonly UserRepository _repository = repository;

        [HttpPost("telegram")]
        public async Task<IActionResult> Authenticate([FromBody] TelegramInitDataRequest request)
        {
            var user = ParseUser(request.InitData);

            if (user == null)
                return Ok();
            if (!Validate(Uri.UnescapeDataString(request.InitData)))
                return Unauthorized();

            var userEntity = await _repository.GetUser(user.Username);
            if (userEntity == null)
            {
                //return Unauthorized();
            }

            var roles = userEntity.UserRoles?
                .Where(ur => ur.Role != null)
                .Select(ur => new Claim(ClaimTypes.Role, ur.Role.Name))
                .ToList()
                ?? new List<Claim> { new Claim(ClaimTypes.Role, "Default") };

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_botToken);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([
                    new Claim(ClaimTypes.Name, user.Username)
                ]),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            tokenDescriptor.Subject.AddClaims(roles);
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });
        }

        private bool Validate(string initData)
        {
            const string CONSTANT_KEY = "WebAppData";

            var query = HttpUtility.ParseQueryString(initData);

            var queryDict = QueryToSortedDictionary(query);

            var dataCheckString = QueryDictionaryToString('\n', queryDict, [InitDataKey.HASH]);

            var secretKey = HMACSHA256.HashData(Encoding.UTF8.GetBytes(CONSTANT_KEY),
                Encoding.UTF8.GetBytes(_botToken));

            var generatedHash = HMACSHA256.HashData(secretKey,
                Encoding.UTF8.GetBytes(dataCheckString));

            var actualHash = Convert.FromHexString(queryDict[InitDataKey.HASH]);

            return actualHash.SequenceEqual(generatedHash);
        }

        private SortedDictionary<string, string> QueryToSortedDictionary(NameValueCollection query)
        {
            var result = new SortedDictionary<string, string>(
                query.AllKeys.ToDictionary(param => param!, param => query[param]!),
                StringComparer.Ordinal);

            return result;
        }

        private static string QueryDictionaryToString(char separator,
                                               IDictionary<string, string> queryDict,
                                               string[] keysToExclude)
        {
            var result = string.Join(separator, queryDict
                .Where(param => !Array.Exists(keysToExclude, element => element == param.Key))
                .Select(x => $"{x.Key}={x.Value}"));

            return result;
        }

        private static TelegramUser ParseUser(string initData)
        {
            // Parse the query string
            var query = HttpUtility.ParseQueryString(initData);

            // Extract the "user" parameter
            var userJson = query["user"];

            if (string.IsNullOrEmpty(userJson))
            {
                return null;
                //throw new ArgumentException("Invalid initData: 'user' parameter is missing.");
            }

            var user = JsonConvert.DeserializeObject<TelegramUser>(userJson);

            return user;
        }
    }

    public class TelegramInitDataRequest
    {
        public string InitData { get; set; }
    }

    public static class InitDataKey
    {
        public const string HASH = "hash";
        public const string AUTH_DATE = "auth_date";
        public const string QUERY_ID = "query_id";
        public const string USER = "user";
    }

    public class TelegramUser
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string LanguageCode { get; set; }
        public bool AllowsWriteToPm { get; set; }
        public string PhotoUrl { get; set; }
    }
}