using Newtonsoft.Json;
using ReckonTextSearcher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ReckonTextSearcher.Controllers
{
	public class TextSearchController : ApiController
	{
		[HttpGet]
		[Route("api/TextSearch/Search")]
		public async Task<IHttpActionResult> Search()
		{
			int maxRetries = 10;
			HttpResponseMessage response = null;
			for (int i = 0; i < maxRetries; i++)
			{
				HttpClient httpClient = new HttpClient();
				response = await httpClient.GetAsync("https://join.reckon.com/test2/textToSearch");

				if (response.IsSuccessStatusCode)
				{
					break;
				}
			}

			if (response != null)
			{
				var content = await response.Content.ReadAsStringAsync();
				TextToSearch text = JsonConvert.DeserializeObject<TextToSearch>(content);

				HttpResponseMessage response2 = null;
				for (int i = 0; i < maxRetries; i++)
				{
					HttpClient httpClient2 = new HttpClient();
					response2 = await httpClient2.GetAsync("https://join.reckon.com/test2/subTexts");

					if (response2.IsSuccessStatusCode)
					{
						break;
					}
				}

				if (response != null)
				{
					var content2 = await response2.Content.ReadAsStringAsync();
					Subtexts subtexts = JsonConvert.DeserializeObject<Subtexts>(content2);

					ResultText resultText = new ResultText();
					resultText.candidate = "Frederick Escobar";
					resultText.results = new List<SubtextResult>();
					resultText.text = text.Text;
					foreach (string subtext in subtexts.subTexts)
					{
						SubtextResult subtextResult = new SubtextResult();
						subtextResult.subtext = subtext;
						var index = 0;
						var initialIndex = -1;
						string foundIndex = "";
						var textIndex = 0;
						foreach(char c in text.Text)
						{
							textIndex++;
							if (index != subtext.Length)
							{
								if (char.ToUpper(c) == char.ToUpper(subtext[index]))
								{
									if (index == 0)
									{
										initialIndex = textIndex;
									}
									index++;
								}
								else
								{
									index = 0;
									initialIndex = -1;
								}
							}
							else
							{
								if (initialIndex != -1)
								{
									if (foundIndex != "")
									{
										foundIndex += ", ";
									}
									foundIndex += initialIndex.ToString();
									initialIndex = -1;
									index = 0;
								}
							}
						}

						if (foundIndex.Any())
						{
							subtextResult.result = foundIndex;
						}
						else
						{
							subtextResult.result = "<No Output>";
						}
						resultText.results.Add(subtextResult);
					}

					HttpResponseMessage response3 = null;
					for (int i = 0; i < maxRetries; i++)
					{
						HttpClient httpClient3 = new HttpClient();
						var json = JsonConvert.SerializeObject(resultText);
						var data = new StringContent(json, Encoding.UTF8, "application/json");
						response3 = await httpClient3.PostAsync("https://join.reckon.com/test2/submitResults", data);

						if (response3.IsSuccessStatusCode)
						{
							break;
						}
					}
				}
				
			}
			return Ok();
		}

	}
}
