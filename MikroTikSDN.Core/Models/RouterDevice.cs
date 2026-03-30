using System;
using System.Net.Mail;

namespace MikroTikSDN.Core.Models
{
    public class RouterDevice
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        // Construtor para desserialização JSON (RouterManager lê do ficheiro)
        public RouterDevice()
        {
            Id = System.Guid.NewGuid().ToString();
            Name = "";
            IpAddress = "";
            MacAddress = "";
            Username = "";
            Password = "";
        }

        public RouterDevice(string name, string ipAddress, string username, string password, string macAddress)
        {
            Id = System.Guid.NewGuid().ToString();
            Name = name;
            IpAddress = ipAddress;
            MacAddress = macAddress;
            Username = username;
            Password = password;
        }
    }
}