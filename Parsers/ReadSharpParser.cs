using Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Parsers
{
    public class ReadSharpParser : IParser
    {
        public async Task<IArticle> ParseAsync(string url)
        {
            var reader = new ReadSharp.Reader();

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("url cannot be empty");
            }

            try
            {
                var result = await reader.ReadAsync(new Uri(url));

                return new ReadSharpArticle
                {
                    Content = result.Content,
                    Title = result.Title,
                    Url = url
                };
            }
            catch (Exception) { }

            return null;
        }
    }
}
