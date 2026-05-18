using Terraria;
using TerrariaApi.Server;
using TShockAPI;
namespace TileTracking
{
    [ApiVersion(2, 1)]
    public class TileTrack : TerrariaPlugin
    {

        public override string Author => "MaximPrime";
        public override string Name => "Tile Tracking Plugin";
        public override string Description => "A plugin for checking tile coordinates in the world";
        public override System.Version Version => new(1, 0);

        public TileTrack(Main game) : base(game)
        {

        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("spleef.tiletrack", ToggleTileTracking, "tilepos"));
        }

        private readonly Dictionary<string, bool> trackingPlayers = new();

        public void ToggleTileTracking(CommandArgs args)
        {
            string playerName = args.Player.Name;
            if (trackingPlayers.ContainsKey(playerName) && trackingPlayers[playerName])
            {
                trackingPlayers[playerName] = false;
                args.Player.SendSuccessMessage("Tile tracking disabled.");
                foreach (var plr in trackingPlayers)
                {
                    if (plr.Value)
                    {
                        break;
                    }
                }
                ServerApi.Hooks.NetGetData.Deregister(this, OnTileEdit);
            }
            else
            {
                ServerApi.Hooks.NetGetData.Register(this, OnTileEdit);
                trackingPlayers[playerName] = true;
                args.Player.SendSuccessMessage("Tile tracking enabled. Interact with a tile to see its position.");
            }
        }

        private void OnTileEdit(GetDataEventArgs args)
        {
            if (args.Handled || args.MsgID != PacketTypes.Tile)
                return;

            var player = TShock.Players[args.Msg.whoAmI];
            if (player == null || !player.Active || !trackingPlayers.ContainsKey(player.Name) || !trackingPlayers[player.Name])
                return;

            using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
            {
                var action = reader.ReadByte();
                var x = reader.ReadInt16();
                var y = reader.ReadInt16();

                if (action is 0 or 1 or 5 or 6 or 10 or 11 or 12 or 13 or 16 or 17)
                {
                    args.Handled = true;
                    NetMessage.SendTileSquare(player.Index, x, y, 1);
                    player.SendInfoMessage($"X={x}, Y={y}");
                }
            }
        }
    }
}
