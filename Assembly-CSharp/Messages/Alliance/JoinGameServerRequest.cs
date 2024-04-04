// ROGUES
// SERVER
using System;
using UnityEngine.Networking;

// server-only, missing in reactor
#if SERVER
[Serializable]
public class JoinGameServerRequest : AllianceMessageBase
{
	// all fields internal in rogues
	public int OrigRequestId;
	public LobbySessionInfo SessionInfo;
	public LobbyServerPlayerInfo PlayerInfo;
	public string GameServerProcessCode;

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		OrigRequestId = reader.ReadInt32();
		DeserializeObject(out SessionInfo, reader);
		DeserializeObject(out PlayerInfo, reader);
		GameServerProcessCode = reader.ReadString();
	}
}
#endif
