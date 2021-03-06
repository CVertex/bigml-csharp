﻿using System;
using System.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BigML
{
    public partial class Client
    {
        const string BigML = "{3}://{4}/{2}andromeda/{5}?username={0};api_key={1}";
        const string BigMLList = "{3}://{4}/{2}andromeda/{5}?{6};username={0};api_key={1}";


        //const string BigML = "https://192.168.1.105:1026/{2}andromeda/{3}?username={0};api_key={1}";
        //const string BigMLList = "https://192.168.1.105:1026/{2}andromeda/{4}?{3};username={0};api_key={1}";


        /// <summary>
        /// Map type name to resource type name (Source --> source, ...)
        /// </summary>
        static string ResourceTypeName<T>() where T : Response
        {
            return typeof(T).Name.ToLower();
        }

        /// <summary>
        /// Create a new resource using supplied arguments
        /// </summary>
        public Task<T> Create<T>(Response.Arguments<T> arguments) where T : Response, new()
        {
            var content = new JsonContent(arguments.ToJson());
            return Create<T>(content);
        }

        /// <summary>
        /// Helper to create resource given HttpContent.
        /// </summary>
        private async Task<T> Create<T>(HttpContent request) where T : Response, new()
        {
            var client = new HttpClient();
            var url = string.Format(BigML, _username, _apiKey, _dev, _protocol, _VpcDomain, ResourceTypeName<T>());
            var response =  await client.PostAsync(url, request);
            var resource = JsonValue.Parse(await response.Content.ReadAsStringAsync());

            switch (response.StatusCode)
            {
                case HttpStatusCode.Created:
                    return new T {Object = resource, Location = response.Headers.Location };

                case HttpStatusCode.BadRequest:
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.PaymentRequired:
                case HttpStatusCode.NotFound:
                    return new T {Object = resource};

                default:
                    return new T();
            }
        }

        /// <summary>
        /// Delete a resource.
        /// </summary>
        public Task<Response> Delete<T>(T resource) where T : Response
        {
            return Delete(resource.Resource);
        }

        /// <summary>
        /// Delete a resource.
        /// </summary>
        public async Task<Response> Delete(string resourceId)
        {
            if (resourceId == null) 
                throw new ArgumentNullException("resourceId");

            var client = new HttpClient();
            var response = await client.DeleteAsync(string.Format(BigML, _username, _apiKey, _dev, _protocol, _VpcDomain, resourceId));

            switch (response.StatusCode)
            {
                case HttpStatusCode.NoContent:
                    return new Response { Object = new JsonObject { { "code", (int)response.StatusCode } } };

                case HttpStatusCode.BadRequest:
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.PaymentRequired:
                case HttpStatusCode.NotFound:
                    return new Response { Object = JsonValue.Parse(await response.Content.ReadAsStringAsync()) };

                default:
                    return new Response();
            }
        }

        /// <summary>
        /// Retrieve a resource.
        /// </summary>
        public Task<T> Get<T>(T resource) where T : Response, new()
        {
            return Get<T>(resource.Resource);
        }

        /// <summary>
        /// Retrieve a resource.
        /// </summary>
        public async Task<T> Get<T>(string resourceId) where T : Response, new()
        {
            if (resourceId == null)
                throw new ArgumentNullException("resourceId");

            var client = new HttpClient();
            var response = await client.GetAsync(string.Format(BigML, _username, _apiKey, _dev, _protocol, _VpcDomain, resourceId));
            var resource = JsonValue.Parse(await response.Content.ReadAsStringAsync());

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return new T { Object = resource };

                case HttpStatusCode.BadRequest:
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.PaymentRequired:
                case HttpStatusCode.NotFound:
                    return new T { Object = resource };

                default:
                    return new T();
            }
        }

        /// <summary>
        /// Check whether a resource's status is FINISHED.
        /// </summary>
        public Task<bool> IsReady<T>(T resource) where T : Response, new()
        {
            return IsReady(resource.Resource);
        }

        /// <summary>
        /// Check whether a resource's status is FINISHED.
        /// </summary>
        public async Task<bool> IsReady(string resourceId) 
        {
            var response = await Get<Response>(resourceId);
            return response.Code == HttpStatusCode.OK
                && (Code)(int)response.Object.status.code == Code.Finished;
        }

        /// <summary>
        /// List all resources
        /// </summary>
        public async Task<Listing<T>> List<T>(string query) where T : Response, new()
        {
            var client = new HttpClient();
            var response = await client.GetAsync(string.Format(BigMLList, _username, _apiKey, _dev, _protocol, _VpcDomain, ResourceTypeName<T>(), query));
            var resource = JsonValue.Parse(await response.Content.ReadAsStringAsync());

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return new Listing<T> { Object = resource };

                case HttpStatusCode.BadRequest:
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.PaymentRequired:
                case HttpStatusCode.NotFound:
                    return new Listing<T> { Object = resource };

                default:
                    return new Listing<T>();
            }
        }

        /// <summary>
        /// Update a resource.
        /// </summary>
        public Task<T> Update<T>(T resource, string name) where T : Response, new()
        {
            return Update<T>(resource.Resource, name);
        }

        /// <summary>
        /// Update a resource with Json payload
        /// </summary>
        public async Task<T> Update<T>(string resourceId, JsonValue changes) where T : Response, new()
        {
            if (resourceId == null)
                throw new ArgumentNullException("resourceId");

            var client = new HttpClient();
            var content = new JsonContent(changes);
            var response = await client.PutAsync(string.Format(BigML, _username, _apiKey, _dev, _protocol, _VpcDomain, resourceId), content);
            var resource = JsonValue.Parse(await response.Content.ReadAsStringAsync());

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return new T { Object = resource };

                case HttpStatusCode.BadRequest:
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.PaymentRequired:
                case HttpStatusCode.NotFound:
                    return new T { Object = resource };

                default:
                    return new T();
            }
        }
        /// <summary>
        /// Update a resource.
        /// </summary>
        public Task<T> Update<T>(string resourceId, string name) where T : Response, new()
        {
            dynamic changes = new JsonObject();
            if (!string.IsNullOrWhiteSpace(name)) changes.name = name;
            return Update(resourceId, changes);
        }

    }
}
