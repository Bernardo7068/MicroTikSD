using MikroTikSDN.Core.Services;

namespace MikroTikSDN.Core
{
    /// <summary>
    /// Fachada principal: agrega todos os serviços para um router.
    /// Na UI crias um RouterManager por cada dispositivo que queres gerir.
    /// 
    /// Exemplo de uso:
    ///   var router = new RouterManager("192.168.88.1", "admin", "password", name: "Router Casa");
    ///   var interfaces = await router.Interfaces.GetAllAsync();
    /// </summary>
    public class RouterManager
    {
        private readonly RouterClient _client;

        // Serviços disponíveis
        public InterfaceService     Interfaces       { get; }
        public BridgeService        Bridges          { get; }
        public SecurityProfileService SecurityProfiles { get; }
        public IpAddressService     IpAddresses      { get; }
        public RouteService         Routes           { get; }
        public DhcpService          Dhcp             { get; }
        public DnsService           Dns              { get; }
        public SystemService        System           { get; }

        // Metadados do router
        public string Host => _client.Host;
        public string Name
        {
            get => _client.Name;
            set => _client.Name = value;
        }

        public RouterManager(string host, string username, string password,
                             bool useHttps = false, string name = null)
        {
            _client          = new RouterClient(host, username, password, useHttps, name);

            Interfaces       = new InterfaceService(_client);
            Bridges          = new BridgeService(_client);
            SecurityProfiles = new SecurityProfileService(_client);
            IpAddresses      = new IpAddressService(_client);
            Routes           = new RouteService(_client);
            Dhcp             = new DhcpService(_client);
            Dns              = new DnsService(_client);
            System           = new SystemService(_client);
        }

        /// <summary>Testa a ligação ao router antes de mostrar na UI.</summary>
        public System.Threading.Tasks.Task<bool> TestConnectionAsync()
            => _client.TestConnectionAsync();
    }
}
