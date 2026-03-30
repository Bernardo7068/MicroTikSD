using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Managers
{
    public class RouterManager
    {
        private readonly string _filePath = "routers_db.json";
        private List<RouterDevice> _routers;

        public RouterManager()
        {
            _routers = LoadRouters();
        }

        public List<RouterDevice> GetAllRouters() => _routers;

        public void AddRouter(RouterDevice router)
        {
            _routers.Add(router);
            SaveRouters();
        }

        private void SaveRouters()
        {
            var json = JsonSerializer.Serialize(_routers, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        private List<RouterDevice> LoadRouters()
        {
            if (!File.Exists(_filePath))
                return new List<RouterDevice>();

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<RouterDevice>>(json) ?? new List<RouterDevice>();
        }
    }
}