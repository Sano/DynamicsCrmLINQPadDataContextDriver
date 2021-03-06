﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Tedd.DynamicsCrmLINQPadDataContextDriver.Models;

namespace Tedd.DynamicsCrmLINQPadDataContextDriver.DataConnection
{
    public class XrmConnection : IDisposable
    {
        private OrganizationServiceProxy _organizationServiceProxy;
        public void Connect(ConnectionData connectionData)
        {
            if (_organizationServiceProxy != null)
                throw new Exception("Already connected.");

            AuthenticationCredentials clientCredentials = new AuthenticationCredentials();
            clientCredentials.ClientCredentials.UserName.UserName = connectionData.Domain + "\\" + connectionData.Username;
            clientCredentials.ClientCredentials.UserName.Password = connectionData.Password;
            Uri OrgServiceURL = new Uri(connectionData.OrganizationUrlWithCrmService2011);
            OrganizationServiceProxy serviceProxy = new OrganizationServiceProxy(OrgServiceURL, null, clientCredentials.ClientCredentials, null);
            //Console.WriteLine("Connected to " + serviceProxy.ServiceConfiguration.ServiceEndpoints.First().Value.Address.Uri); 
            _organizationServiceProxy = serviceProxy;
        }


        public string WhoAmI()
        {
            WhoAmIResponse whoResp = (WhoAmIResponse)_organizationServiceProxy.Execute(new WhoAmIRequest());
            Entity user = _organizationServiceProxy.Retrieve("systemuser", whoResp.UserId, new ColumnSet(true));
            return user["fullname"].ToString();
        }

        public List<EntityMetadata> GetEntities()
        {
            RetrieveAllEntitiesRequest req = new RetrieveAllEntitiesRequest();
            req.EntityFilters = EntityFilters.Entity | EntityFilters.Attributes;
            req.RetrieveAsIfPublished = true;

            RetrieveAllEntitiesResponse response = (RetrieveAllEntitiesResponse)_organizationServiceProxy.Execute(req);

            var list = response.EntityMetadata.ToList();
            return list;
        }



        #region IDisposable
        private void ReleaseUnmanagedResources()
        {
            var osp = _organizationServiceProxy;
            _organizationServiceProxy = null;
            osp?.Dispose();
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                _organizationServiceProxy?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~XrmConnection()
        {
            Dispose(false);
        }
        #endregion
    }
}
