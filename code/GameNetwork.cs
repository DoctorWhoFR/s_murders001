using System.Collections.Generic;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Network;

public enum GameState
{
	Loading = 0,
	Starting = 1,
}

public enum PlayerType
{
	Civil,
	Murderer,
	Gunner
}

public struct PlayerData
{
	public Connection Connection { get; set; }
	public string DisplayName { get; set; }
	public System.Guid Id { get; set; }
	public bool Host { get; set; }
	public bool InGame { get; set; }
}

public struct GameStateData
{
	public PlayerData Murderer { get; set; }
	public PlayerData Gunner { get; set; }
	public List<PlayerData> Civils { get; set; }
}

public sealed class GameNetwork : Component, Component.INetworkListener
{
	[Property]
	private GameObject ScreensFolder { get; set; }

	[Property, HostSync, Sync] 
	public GameState State { get; set; } = GameState.Loading;

	public GameStateData GameStateData { get; set; }

	private List<PlayerData> Players { get; set; } = new List<PlayerData>();

	[Property]
	private GameObject PlayerPrefab { get; set; }

	[Property, HostSync]
	public int P_PlayersCount {  get; set; } = 0;

	[Property]
	public int MinimumPlayers { get; set; } = 2;

	public int PlayersCount()
	{
		return Players.Count;
	}

	private List<PlayerData> CopyPlayers()
	{
		var copy = new List<PlayerData>();
		foreach ( var player in Players )
		{
			copy.Add( player );
		}

		return copy;
	}

	public void CreatePlayer( PlayerData player, PlayerType playerType )
	{
		GameObject _Player = PlayerPrefab.Clone();

		CustomPlayerController customPlayerController = _Player.GetComponent<CustomPlayerController>();
		if ( customPlayerController == null )
		{
			Log.Info( "should not append" );
		}

		customPlayerController.playerType = playerType;
		customPlayerController.Id = player.Id;

		switch ( playerType )
		{
			case PlayerType.Murderer:
				Log.Info( "is an murdered give knife swep" );
				break;
			case PlayerType.Gunner:
				Log.Info( "is an gunner give gun swep" );
				break;
			case PlayerType.Civil:
				Log.Info( "is civil do not give things" );
				break;
		}

		_Player.NetworkSpawn( player.Connection );
	}

	// CheckState Thread
	async public void CheckState()
	{
		while ( true )
		{
			if ( State == GameState.Loading )
			{
				// if we are loading and have enought players start game
				if ( P_PlayersCount >= MinimumPlayers )
				{
					Log.Info( "Starting game" );
					State = GameState.Starting;

					// hide menu
					GameSearchingUI gameSearchingUI = ScreensFolder.GetComponentInChildren<GameSearchingUI>(true);
					if ( gameSearchingUI == null ) 
					{
						Log.Info( "should not append" );
					}

					gameSearchingUI.ChangeState( false );

					List<PlayerData> _chooseList = CopyPlayers();

					PlayerData murderer = Game.Random.FromList( _chooseList );
					_chooseList.Remove( murderer );
					CreatePlayer( murderer, PlayerType.Murderer );

					PlayerData gunner = Game.Random.FromList( _chooseList );
					_chooseList.Remove( gunner );
					CreatePlayer(gunner, PlayerType.Gunner );

					Log.Info( $"{murderer} {gunner}" );
					Log.Info( $"Left civils: {_chooseList.Count}" );


					GameStateData _state = new();
					_state.Murderer = murderer;
					_state.Gunner = gunner;
					_state.Civils = _chooseList;

					GameStateData = _state;

					foreach ( var player in _chooseList )
					{
						CreatePlayer(player, PlayerType.Civil );
					}

				} else
				{
					Log.Info( "not enought player's to start" );
				}
			}


			await Task.Delay( 1000 );
		}
	}



	public void OnActive( Connection connection )
	{
		Log.Info( "player is connecting" );

		PlayerData playerData = new PlayerData();
		playerData.Connection = connection;
		playerData.DisplayName = connection.DisplayName;
		playerData.Id = connection.Id;
		playerData.InGame = false;

		if ( connection.IsHost )
		{
			Log.Info( "host self connecting, adding host in list" );
			playerData.Host = true;
		}
		else
		{
			Log.Info( "client is connecting" );
		}

		P_PlayersCount++;
		Players.Add( playerData );
	}

	// When a game is firstly init
	protected override void OnEnabled()
	{
		base.OnEnabled();
		Log.Info( Networking.IsHost );
		if ( Networking.IsHost )
		{
			Log.Info( "is the host and its enabled so its first join server loading" );
			Networking.CreateLobby( new LobbyConfig()
			{
				MaxPlayers = 8,
				Privacy = LobbyPrivacy.Private,
				Name = "My Lobby Name"
			} );
			CheckState();	
		} else
		{
			Log.Info( $"should be an client {Networking.IsClient}" );
		}
	}
	
}
