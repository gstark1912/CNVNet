/*
 * HFX.Security v1.0.2 (http://www.horizon.com.ar)
 * ========================================================================
 * Copyright 2014 Horizon.
 * ========================================================================
 * Servicio para la logica de negocio de la entidad Cliente
*/
using KATHIE.Central.DAL.Repositories;
using KATHIE.Central.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KATHIE.Central.BLL1.Services
{
    public class ServiceClient
    {
        private RepositoryClient repositoryClient;
        public ServiceClient()
        {
            repositoryClient = new RepositoryClient();
        }

        public Client Get() { return repositoryClient.Get(); }

        public void Update(Client client)
        {
            repositoryClient.Update(client);
        }
    }
}
