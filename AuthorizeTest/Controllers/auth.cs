using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

/// <summary>
/// ダミー認可サーバ
/// </summary>
namespace AuthorizeTest.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class auth : ControllerBase
	{
		// GET: api/<auth>
		[HttpGet]
		public string Get()
		{
			return "{ 'auth': 'Hello world.' }";
		}

		/// <summary>
		///認可ページのダミー
		/// </summary>
		/// <param name="client_id">認証用クライアントID</param>
		/// <param name="response_type">認証方式(code)</param>
		/// <param name="scope">スコープ</param>
		/// <param name="redirect_uri">リダイレクト先URI</param>
		/// <param name="state">(オプション)</param>
		/// <returns>認可されたことにして、即リダイレクトします</returns>
		// GET api/auth/authorize
		[HttpGet("authorize")]
		public IActionResult Get(string client_id, string response_type, string scope, string redirect_uri, string state)
		{
			string uri = redirect_uri + "?code=AuthCode001";
			return new RedirectResult(uri);
		}

		/// <summary>
		/// アクセストークン取得のダミー
		/// </summary>
		/// <param name="client_id">認証用クライアントID</param>
		/// <param name="client_secret">認証用クライアントシークレット</param>
		/// <param name="grant_type">認証タイプ authorization_code </param>
		/// <param name="redirect_uri">ユーザー認証で指定したURL</param>
		/// <param name="code">認可コード</param>
		/// <returns></returns>
		// POST api/auth/token
		[HttpPost("token")]
		[Consumes("application/x-www-form-urlencoded")]
		public IActionResult Post([FromForm] string client_id, string client_secret, string grant_type, string redirect_uri, string code)
		{
			string result = "{" +
				@"""access_token"": ""abcdefg-1234-5678-9123-abcdefghijkl""," +
				@"""token_type"": ""Bearer""," +
				@"""expires_in"": 2592000," +
				@"""refresh_token"": ""abcdefg-9876-5432-1111-abcdefghijkl""" +
				"}";

			return Ok(result);
		}

		//// PUT api/<auth>/5
		//[HttpPut("{id}")]
		//public void Put(int id, [FromBody] string value)
		//{
		//}

		//// DELETE api/<auth>/5
		//[HttpDelete("{id}")]
		//public void Delete(int id)
		//{
		//}
	}
}
