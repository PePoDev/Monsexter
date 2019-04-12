using System.Text;
using UnityEngine;

public static class Utils
{
    const string glyphs = "abcdefghijklmnopqrstuvwxyz";
    
    /// <summary>
    /// Create a random token string (default tokenLength is 8)
    /// </summary>
    /// <param name="tokenLenght"></param>
    /// <returns></returns>
    public static string GetRandomToken(int? tokenLenght = null)
    {
        if (tokenLenght < 0 || tokenLenght == 0)
        {
            Debug.LogError("Token lenght is not be 0 or negative value");
            return null;
        }

        if (tokenLenght == null)
        {
            tokenLenght = 9;
        }

        var token = new StringBuilder();

        for (int i = 0; i < tokenLenght; i++)
        {
            token.Append(glyphs[Random.Range(0, glyphs.Length)]);
        }
        return token.ToString();
    }
}
