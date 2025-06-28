namespace GbaMonoGame;

public enum MubState
{
    State0 = 0,
    State1 = 1,
    EstablishConnections = 2, // RSMULTIPLAYER_STATE_ESTABLISH_CONNECTIONS
    Connected = 3,
    Error = 4,
    State5 = 5,
    BadConnection = 6,
    UnInit = 0xFF,
}