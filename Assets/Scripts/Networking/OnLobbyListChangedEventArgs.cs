using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;

namespace Assets.Scripts.Networking
{
    public class OnLobbyListChangedEventArgs : EventArgs 
    {
        public List<Lobby> Lobbies { get; set; }
    }
}
