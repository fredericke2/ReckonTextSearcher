using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReckonTextSearcher.Models
{
	public class ResultText
	{
		public string candidate { get; set; }
		public string text { get; set; }
		public List<SubtextResult> results { get; set; }
	}
}