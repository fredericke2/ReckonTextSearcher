using Newtonsoft.Json;
using ReckonTextSearcher.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
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
		private async Task<TextToSearch> GetInputBase()
		{
			TextToSearch text = new TextToSearch();
			int retry = 1;
			HttpResponseMessage response = null;

			int.TryParse(ConfigurationManager.AppSettings["RetryValue"], out retry);

			for (int i = 0; i < retry; i++)
			{
				HttpClient httpClient = new HttpClient();
				response = await httpClient.GetAsync(ConfigurationManager.AppSettings["InputTextBaseUrl"]);

				if (response.IsSuccessStatusCode)
				{
					break;
				}
			}

			if (response != null)
			{
				var content = await response.Content.ReadAsStringAsync();
				 text = JsonConvert.DeserializeObject<TextToSearch>(content);
			}
			return text;
		}

		private async Task<Subtexts> GetSubtext()
		{
			Subtexts text = new Subtexts();
			int retry = 1;
			HttpResponseMessage response = null;

			int.TryParse(ConfigurationManager.AppSettings["RetryValue"], out retry);

			for (int i = 0; i < retry; i++)
			{
				HttpClient httpClient = new HttpClient();
				response = await httpClient.GetAsync(ConfigurationManager.AppSettings["InputSubtextBaseUrl"]);

				if (response.IsSuccessStatusCode)
				{
					break;
				}
			}

			if (response != null)
			{
				var content = await response.Content.ReadAsStringAsync();
				text = JsonConvert.DeserializeObject<Subtexts>(content);
			}
			return text;
		}

		private async Task<bool> PostResult(ResultText resultText)
		{
			int retry = 1;
			HttpResponseMessage response = null;

			int.TryParse(ConfigurationManager.AppSettings["RetryValue"], out retry);

			for (int i = 0; i < retry; i++)
			{
				HttpClient httpClient = new HttpClient();
				var json = JsonConvert.SerializeObject(resultText);
				var data = new StringContent(json, Encoding.UTF8, "application/json");

				response = await httpClient.PostAsync(ConfigurationManager.AppSettings["PostUrl"], data);

				if (response.IsSuccessStatusCode)
				{
					break;
				}
			}
			return true;
		}


		[HttpGet]
		[Route("api/TextSearch/Search")]
		public async Task<IHttpActionResult> Search()
		{
			var text = await GetInputBase();

			var subtexts = await GetSubtext();

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

			var result = await PostResult(resultText);
			return Ok();
		}

	}
}
