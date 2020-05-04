using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplicationSignalR
{
    [HubName("hubRobo")]
    public class HubRobo : Hub
    {
   

        public HubRobo()
        {
         
        }
        public static List<Robo> ListaRobo { get; set; } = new List<Robo>();

        public override Task OnDisconnected(bool stopCalled)
        {
            var id = Context.ConnectionId;
            if (!string.IsNullOrEmpty(id))
            {
                ListaRobo.RemoveAll(l => l.IdRobo == id);
            }

            return base.OnDisconnected(stopCalled);
        }

        //public override Task OnConnected()
        //{
        //    return base.OnConnected();
        //}

        #region Server


        public static void EnviarMensagemCliente(string mensagem)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<HubRobo>();
            hubContext.Clients.All.AddMensagemCliente(mensagem);
        }

        public static void HoradeTrabalhar(string mensagem)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<HubRobo>();
            if (!RoboLigado())
            {

                hubContext.Clients.All.AddMensagemClienteJavaScript($"Todos Robos Desligados");
                return;
            }

            var robo = string.Empty;

            do
            {
                robo = ObterRoboSemTrampo();

                if (string.IsNullOrEmpty(robo))
                    hubContext.Clients.All.AddMensagemClienteJavaScript($"Robos=> Ocupados");

            } while (string.IsNullOrEmpty(robo));

            hubContext.Clients.Client(robo).HoradeTrabalhar(mensagem);

            AtualizarRobosDisponiveis(true, robo);


        }


        private static bool RoboLigado()
        {
            return ListaRobo.Any();
        }

        #endregion


        #region Client
        public void RoboTerminou(string mensagem, string idRobo)
        {
            AtualizarRobosDisponiveis(false, idRobo);

            Clients.Client(idRobo).AddMenagemCliente(mensagem);

            //Cliente JavaScript
            Clients.All.AddMensagemClienteJavaScript($"Robo Terminou Tarefa: { mensagem }  { idRobo }");
        }

        public void RoboDisponivel(string mensagem, string idRobo)
        {
            ListaRobo.Add(new Robo()
            {
                IdRobo = idRobo,
                Ocupado = false
            });


            Clients.All.AddMensagemCliente(mensagem);

        }

        public void EnviarMensagemServer(string mensagem, string idRobo)
        {
            
            Clients.All.AddMensagemCliente(mensagem);
            //Cliente JavaScript
            Clients.All.AddMensagemClienteJavaScript($" { mensagem }  { idRobo }");
        }


        #endregion

        #region Métodos


        public static void AtualizarRobosDisponiveis(bool ocupado, string idRobo)
        {
            ListaRobo.FindAll(l =>
            {
                if (l.IdRobo == idRobo)
                {
                    l.Ocupado = ocupado;
                }
                return true;
            });
        }



        private static string ObterRoboSemTrampo()
        {
            var result = ListaRobo.FirstOrDefault(l => !l.Ocupado);
            return result?.IdRobo;
        }

        #endregion
    }
}