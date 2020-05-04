using Microsoft.AspNet.SignalR.Client;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsSgnalrClient
{
    public partial class FrmRobo : Form
    {

        HubConnection _hubConnection;
        IHubProxy _hubProxy;

        public delegate void Mensagem(string texto);

        public delegate Task EventHandler(string mensagem);

        public static event EventHandler HoradeTrabalhar;

        public void PopularTextbox(string texto)
        {
            textBox1.Text += texto + Environment.NewLine;
            textBox1.SelectionStart = textBox1.TextLength;
            textBox1.ScrollToCaret();
        }


        public FrmRobo()
        {
            InitializeComponent();
        }

        private async void FrmRobo_Load(object sender, EventArgs e)
        {
            IniciarSignalr();
            HoradeTrabalhar += FrmRobo_HoradeTrabalhar;
        }

        private void IniciarSignalr()
        {
            Task.Delay(TimeSpan.FromSeconds(10)).Wait();
            _hubConnection = new HubConnection("http://localhost:1246/");
            _hubConnection.TransportConnectTimeout = TimeSpan.FromSeconds(3);
            _hubConnection.StateChanged += _hUbConnection_StateChanged;
            _hubProxy = _hubConnection.CreateHubProxy("hubRobo");

            _hubProxy.On("AddMensagemCliente", (mensagem) =>
            {
                Log(mensagem);
            });

            _hubProxy.On("HoradeTrabalhar", (mensagem) =>
            {
                Log(mensagem);
                HoradeTrabalhar.Invoke(mensagem);
            });

            Conectar();
        }

        private async Task Conectar()
        {
            ServicePointManager.DefaultConnectionLimit = 10;
            _hubConnection.Start().Wait(TimeSpan.FromSeconds(10));

            if (_hubConnection.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected)
            {
                await _hubProxy.Invoke("RoboDisponivel", "Estou Disponivel robo: " + _hubConnection.ConnectionId, _hubConnection.ConnectionId);
            }
            else
            {
                Log("Servidor off ");
                _hubConnection.Reconnected += _hUbConnection_Reconnected;
                _hubConnection.Reconnecting += _hUbConnection_Reconnecting;
            }
        }

        private async void _hUbConnection_Reconnecting()
        {
            await Conectar();
        }

        private async void _hUbConnection_Reconnected()
        {
            await Conectar();
        }

        private async void _hUbConnection_StateChanged(StateChange obj)
        {
            if (obj.NewState == ConnectionState.Connecting || obj.NewState == ConnectionState.Disconnected || obj.NewState == ConnectionState.Disconnected)
            {
              await  Conectar();
            }
        }

        private async Task FrmRobo_HoradeTrabalhar(string mensagem)
        {
            await Task.Factory.StartNew(() =>
            {
                Log("Vou enrolar um 15 segundos !!!!");
                Task.Delay(TimeSpan.FromSeconds(15)).Wait();
                Log("Terminei !!!");

                _hubProxy.Invoke("RoboTerminou", $"{"Terminei..     "}{mensagem}", _hubConnection.ConnectionId);
            });
        }

        private void Log(string v)
        {
            var d = new Mensagem(PopularTextbox);
            this.Invoke(d, v);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                return;
            }

            _hubProxy.Invoke("EnviarMensagemServer", $"{textBox2.Text}{_hubConnection.ConnectionId}", _hubConnection.ConnectionId);
        }

    }
}
