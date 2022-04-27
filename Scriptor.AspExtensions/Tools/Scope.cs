using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace AndWeHaveAPlan.Scriptor.AspExtensions.Tools
{
    public static class Scope
    {
        public const string HeaderName = "Scriptor-ForwardedScope64";

        public static void SetScopeToHeader(HttpRequestHeaders headerDictionary, Dictionary<string, object> scopeParams)
        {
            var base64Scope = SerializeScopeBase64(scopeParams);
            headerDictionary.Add(HeaderName, base64Scope);
        }

        public static string SerializeScopeBase64(Dictionary<string, object> scopeParams)
        {
            var scopeString = JsonConvert.SerializeObject(scopeParams);
            var scopeBytes = Encoding.UTF8.GetBytes(scopeString);
            var base64Scope = Convert.ToBase64String(scopeBytes);
            return base64Scope;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scopeHeader"></param>
        /// <returns></returns>
        public static Dictionary<string, string> DeserializeScopeBase64(string scopeHeader)
        {
            if (scopeHeader == null)
                return null;

            var scopeBytes = Convert.FromBase64String(scopeHeader);
            var scopeJson = Encoding.UTF8.GetString(scopeBytes);

            try
            {
                var parsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(scopeJson);
                return parsed;
            }
            catch (Exception e)
            {
                throw new Exception("Invalid scope header", e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scopeHeader"></param>
        /// <returns></returns>
        public static Dictionary<string, string> DeserializeScope(string scopeHeader)
        {
            if (scopeHeader == null)
                return null;

            try
            {
                var parsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(scopeHeader);
                return parsed;
            }
            catch (Exception e)
            {
                throw new Exception("Invalid scope header", e);
            }
        }

        public static Dictionary<string, string> GetScopeFromHeaders(IHeaderDictionary headerDictionary)
        {
            string scopeHeader = headerDictionary[HeaderName].ToString();

            if (scopeHeader != null)
            {
                return DeserializeScopeBase64(scopeHeader);
            }

            scopeHeader = headerDictionary["Scriptor-ForwardedScope"].ToString();

            if (scopeHeader != null)
            {
                return DeserializeScope(scopeHeader);
            }

            return null;
        }
    }
}
