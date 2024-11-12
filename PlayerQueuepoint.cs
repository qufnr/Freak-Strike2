namespace FreakStrike2;

public partial class FreakStrike2
{
    /**
     * 클라이언트 퇴장 시 Queuepoint 초기화 (OnClientDisconnect)
     */
    private void ResetQueuepointOnClientDisconnect(int client)
    {
        _queuepoint.SetPlayerQueuepoint(client, 0);
    }    
}