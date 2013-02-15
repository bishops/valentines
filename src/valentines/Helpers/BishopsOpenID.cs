using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetOpenAuth.AspNet.Clients;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using System.Xml.Linq;

namespace valentines.Helpers
{
    public class BishopsOpenID : OpenIdClient
{
        public BishopsOpenID()
            : base("bishops", "https://google.com/accounts/o8/site-xrds?hd=bishopsstudent.org")
    {

    }

    protected override Dictionary<string, string> GetExtraData(IAuthenticationResponse response)
    {
        FetchResponse fetchResponse = response.GetExtension<FetchResponse>();

        if (fetchResponse != null)
        {
            var extraData = new Dictionary<string, string>();
            extraData.AddItemIfNotEmpty("email", fetchResponse.GetAttributeValue(WellKnownAttributes.Contact.Email));
            extraData.AddItemIfNotEmpty("firstName", fetchResponse.GetAttributeValue(WellKnownAttributes.Name.First));
            extraData.AddItemIfNotEmpty("lastName", fetchResponse.GetAttributeValue(WellKnownAttributes.Name.Last));
            return extraData;
        }

        return null;
    }
    protected override void OnBeforeSendingAuthenticationRequest(IAuthenticationRequest request)
    {
        var fetchRequest = new FetchRequest();
        fetchRequest.Attributes.AddRequired(WellKnownAttributes.Contact.Email);
        fetchRequest.Attributes.AddRequired(WellKnownAttributes.Name.First);
        fetchRequest.Attributes.AddRequired(WellKnownAttributes.Name.Last);
        request.AddExtension(fetchRequest);
    }
    }

    /// <summary>
    /// The dictionary extensions.
    /// </summary>
    internal static class DictionaryExtensions
    {

    /// <summary>
    /// Adds the value from an XDocument with the specified element name if it's not empty.
    /// </summary>
    /// <param name="dictionary">
    /// The dictionary. 
    /// </param>
    /// <param name="document">
    /// The document. 
    /// </param>
    /// <param name="elementName">
    /// Name of the element. 
    /// </param>
    public static void AddDataIfNotEmpty(
        this Dictionary<string, string> dictionary, XDocument document, string elementName)
    {
        var element = document.Root.Element(elementName);
        if (element != null)
        {
            dictionary.AddItemIfNotEmpty(elementName, element.Value);
        }
    }

    /// <summary>
    /// Adds a key/value pair to the specified dictionary if the value is not null or empty.
    /// </summary>
    /// <param name="dictionary">
    /// The dictionary. 
    /// </param>
    /// <param name="key">
    /// The key. 
    /// </param>
    /// <param name="value">
    /// The value. 
    /// </param>
    public static void AddItemIfNotEmpty(this IDictionary<string, string> dictionary, string key, string value)
    {
        if (key == null)
        {
            throw new ArgumentNullException("key");
        }

        if (!string.IsNullOrEmpty(value))
        {
            dictionary[key] = value;
        }
    }
}
}