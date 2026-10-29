using Microsoft.AspNetCore.SignalR;

namespace UtpPos.Mystery.Services.WebApi
{
    public class HubMystery : Hub
    {
        static List<ClsHubUsuariosWEB> ConnectedClients = new List<ClsHubUsuariosWEB>();
        private readonly IHubContext<HubMystery> _hubContext;

        public HubMystery(IHubContext<HubMystery> hubContext_)
        {
            _hubContext = hubContext_;
        }

        #region Conectar Clientes Web y Windows
        public async Task ConectarWeb(string Empresa_ID, string Negocio_ID, string TokenUsuName, string TokenUsuPass)
        {
            try
            {
                string id = Context.ConnectionId;

                bool existe = false;
                int filaExiste = -1;
                for (int i = 0; i < ConnectedClients.Count; i++)
                {
                    if (ConnectedClients[i].ConnectionId == id)
                    {
                        filaExiste = i;
                        existe = true;
                        break;
                    }
                }

                if (!existe)
                {
                    ConnectedClients.Add(new ClsHubUsuariosWEB
                    {
                        ConnectionId = id,
                        Empresa_Id = Empresa_ID,
                        Negocio_Id = Negocio_ID,
                        Tipo = "WEB",
                        Data = ""
                    });
                }
                else
                {
                    if (filaExiste >= 0)
                    {
                        ConnectedClients[filaExiste].ConnectionId = id;
                        ConnectedClients[filaExiste].Empresa_Id = Empresa_ID;
                        ConnectedClients[filaExiste].Negocio_Id = Negocio_ID;
                        ConnectedClients[filaExiste].Tipo = "WEB";
                        ConnectedClients[filaExiste].Data = "";
                    }
                }

                await Clients.Client(id).SendAsync("datosNavegadorMysteryKey", Empresa_ID, Negocio_ID, id);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task ConectarGMASTER(string Empresa_ID, string Negocio_ID, string TokenUsuName, string TokenUsuPass)
        {
            try
            {
                string id = Context.ConnectionId;

                bool existe = false;
                int filaExiste = -1;
                for (int i = 0; i < ConnectedClients.Count; i++)
                {
                    if (ConnectedClients[i].ConnectionId == id)
                    {
                        filaExiste = i;
                        existe = true;
                        break;
                    }
                }

                if (!existe)
                {
                    ConnectedClients.Add(new ClsHubUsuariosWEB
                    {
                        ConnectionId = id,
                        Empresa_Id = Empresa_ID,
                        Negocio_Id = Negocio_ID,
                        Tipo = "GMASTER",
                        Data = ""
                    });
                }
                else
                {
                    if (filaExiste >= 0)
                    {
                        ConnectedClients[filaExiste].ConnectionId = id;
                        ConnectedClients[filaExiste].Empresa_Id = Empresa_ID;
                        ConnectedClients[filaExiste].Negocio_Id = Negocio_ID;
                        ConnectedClients[filaExiste].Tipo = "GMASTER";
                        ConnectedClients[filaExiste].Data = "";
                    }
                }

                await Clients.Client(id).SendAsync("datosGmasterMysteryWinKey", Negocio_ID.ToString(), id);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion


        #region Metodos Propios de SignalR
        public override async Task OnConnectedAsync()
        {
            var id = Context.ConnectionId;
            await Groups.AddToGroupAsync(id, "SignalR Users");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var id = Context.ConnectionId;
            await Groups.RemoveFromGroupAsync(id, "SignalR Users");

            lock (ConnectedClients)
            {
                for (int i = 0; i < ConnectedClients.Count; i++)
                {
                    if (ConnectedClients[i].ConnectionId == id)
                    {
                        ConnectedClients.Remove(ConnectedClients[i]);
                        break;
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
        #endregion
    }
}
