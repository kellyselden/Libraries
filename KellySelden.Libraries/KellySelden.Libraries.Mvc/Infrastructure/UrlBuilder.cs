using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using KellySelden.Libraries.Extensions;

namespace KellySelden.Libraries.Mvc.Infrastructure
{
	public class UrlBuilder
	{
		string _url;
		IDictionary<string, object> _queryCollection;

		public UrlBuilder SetUrl(string url)
		{
			_url = url;
			return this;
		}

		public UrlBuilder AddQuery(string key, object value)
		{
			if (_queryCollection == null) _queryCollection = new Dictionary<string, object>();
			if (value != null) _queryCollection.Add(key, value);
			return this;
		}

		public UrlBuilder SetQueryCollection(NameValueCollection queryCollection)
		{
			_queryCollection = queryCollection.AllKeys.ToDictionaryValue(k => (object)queryCollection[k]);
			return this;
		}

		public string Build()
		{
			string query = string.Join("&", _queryCollection.Select(q => q.Key + '=' + q.Value));
			return _url + (query.Any() ? '?' + query : "");
		}
	}
}