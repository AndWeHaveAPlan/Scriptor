using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using AndWeHaveAPlan.Scriptor.AspExtensions.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Scriptor.AspExtensions.Test
{
    [TestClass]
    public class ScopeForwarding
    {
        private Dictionary<string, object> _testScope = new Dictionary<string, object>
        {
            {"value1", "pickle-pee"},
            {"value2", "pump-a-rum"},
        };

        [TestMethod]
        public void Serialization()
        {
            var b64String = Scope.SerializeScopeBase64(_testScope);

            Assert.IsNotNull(b64String);

            var dictionary = Scope.DeserializeScopeBase64(b64String);

            Assert.AreEqual(dictionary["value1"], "pickle-pee");
            Assert.AreEqual(dictionary["value2"], "pump-a-rum");
        }

        [TestMethod]
        public void SetHeader()
        {
            var message = new HttpRequestMessage();
            Scope.SetScopeToHeader(message.Headers, _testScope);

            var value = message.Headers.GetValues(Scope.HeaderName).FirstOrDefault();

            Assert.IsNotNull(value);
            var dictionary = Scope.DeserializeScopeBase64(value);

            Assert.AreEqual(dictionary["value1"], "pickle-pee");
            Assert.AreEqual(dictionary["value2"], "pump-a-rum");
        }


    }
}
