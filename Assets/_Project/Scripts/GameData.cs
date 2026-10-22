
using System;

[Serializable]
public class GameData
{
    public int moedas;

    public bool duploDashComprado;

    public long dataAtualizacaoUtcTicks;

    public GameData()
    {
        moedas = 0;
        duploDashComprado = false;
        dataAtualizacaoUtcTicks = 0;
    }
}
