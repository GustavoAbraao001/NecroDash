using System;

[Serializable]
public class GameData
{
    // Quantidade de moedas do jogador
    public int moedas;

    // Indica se o upgrade de Duplo Dash foi comprado
    public bool duploDashComprado;

    public GameData()
    {
        moedas = 0;
        duploDashComprado = false;
    }
}
