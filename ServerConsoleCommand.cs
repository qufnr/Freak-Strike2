using CounterStrikeSharp.API;

namespace FreakStrike2;
public partial class FreakStrike2
{
    private void ServerCommandInitialize()
    {
        Server.ExecuteCommand("mp_restartgame 1");
        Server.ExecuteCommand("mp_limitteams 0");
        Server.ExecuteCommand("mp_autoteambalance 0");
        Server.ExecuteCommand("mp_humanteam t");
        
        Server.ExecuteCommand("sv_disable_teamselect_menu 0");
        
        //  Bot Commands
        Server.ExecuteCommand("bot_quota 0");
        Server.ExecuteCommand("bot_quota_mode fill");
        Server.ExecuteCommand("bot_kick");
        Server.ExecuteCommand("bot_join_team t");
    }

    private void IgnoreRoundWinConditions()
    {
        Server.ExecuteCommand("mp_ignore_round_win_conditions 0");
    }
}
