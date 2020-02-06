﻿using PoolHouseStudio.HomeDoneGames.Common.DataAccessObjects.Request;
using PoolHouseStudio.HomeDoneGames.Common.DataAccessObjects.Response;
using PoolHouseStudio.HomeDoneGames.Common.Models;
using PoolHouseStudio.HomeDoneGames.DataAccessLayer.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoolHouseStudio.HomeDoneGames.Service.Services
{
    public interface IHubService
    {
        HubResponse DisconnectPlayer( string connectionId);
        IList<Player> GetPlayers( string roomCode );
        Task<HubResponse> JoinRoom( string connectionId, JoinRoomRequest joinRoomRequest );
    }

    public class HubService : IHubService
    {
        // connection id
        private static Dictionary<string, Player> _players = new Dictionary<string, Player>();

        // room code
        public static Dictionary<string, Game> _games = new Dictionary<string, Game>();

        private readonly IRoomRepository _roomRepository;

        public HubService( IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }


        public HubResponse DisconnectPlayer( string connectionId )
        {
            var player = _players.FirstOrDefault( e => e.Key == connectionId ).Value;
            if (player == null)
            {
                return null;
            }

            var roomCode = player.RoomCode;
            var game = _games.FirstOrDefault( e => e.Key == roomCode ).Value;

            game.Players.Remove( connectionId );
            _players.Remove( connectionId );

            return new HubSuccessResponse
            {
                Data = new PlayersUpdatedResponse
                {
                    GroupName = player.GroupName,
                    Players = game.Players.Values.ToList()
                }
            };
        }


        public IList<Player> GetPlayers( string roomCode )
        {
            var players = GetGame( roomCode ).Players.Values.ToList();
            return players;
        }

        public async Task<HubResponse> JoinRoom( string connectionId, JoinRoomRequest joinRoomRequest )
        {
            if ( string.IsNullOrWhiteSpace( joinRoomRequest.Name ) )
            {
                return new HubErrorResponse { Message = "Must enter a player name", Method = "JoinRoomAsClient", Title = "Validation Error" };
            }

            var includeProperties = string.Join( ",", "GameType" );
            var room = await _roomRepository.FirstOrDefault( e => e.RoomCode == joinRoomRequest.RoomCode, includeProperties );

            var game = GetGame( joinRoomRequest.RoomCode );
            var groupName = $"ClientGroup_{joinRoomRequest.RoomCode}";
            var player = new Player
            {
                Name = joinRoomRequest.Name,
                RoomCode = joinRoomRequest.RoomCode,
                GroupName = groupName
            };

            if ( game.Players.Count < room.GameType.MaxPlayers )
            {
                if ( game.Players.Any( e => e.Key == connectionId ) )
                {
                    return new HubErrorResponse { Message = "Player is already connected.", Method = "JoinRoomAsClient" };
                }

                game.Players.Add( connectionId, player );
                _players.Add( connectionId, player );
            }
            else
            {
                // TODO: what to do with player joining if they are outside the max player count? add as spectator? 
                return new HubErrorResponse { Message = "Game is already full!", Method = "JoinRoomAsClient" };
            }

            return new HubSuccessResponse
            {
                Data = new JoinRoomResponse
                {
                    Description = room.GameType.Description,
                    GameName = room.GameType.GameName,
                    GroupName = groupName,
                    Player = player,
                    MinPlayers = room.GameType.MinPlayers,
                    RoomCode = room.RoomCode
                },
                Message = "Joined!",
                Method = "JoinRoomAsClient"
            };
        }

        private Game GetGame(string roomCode)
        {
            var game = _games.FirstOrDefault( e => e.Key == roomCode ).Value;
            if ( game == null )
            {
                game = new Game();
                _games.Add( roomCode, game );
            }
            return game;
        }
    }
}