using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class Program
{
    private static ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random());

    public static string MineBitcoinHashes(string blockHeader, int difficulty)
    {
        string target = new string('0', difficulty);
        string minedNonce = string.Empty;

        object lockObj = new object();
        bool nonceFound = false;

        _ = Parallel.For(0, Environment.ProcessorCount, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, threadIndex =>
        {
            Random localRandom = random.Value;

            while (!nonceFound)
            {
                int currentNonce = localRandom.Next();

                string data = blockHeader + currentNonce.ToString();
                string hashResult = CalculateSha256Hash(data);

                if (hashResult.Substring(0, difficulty) == target)
                {
                    lock (lockObj)
                    {
                        nonceFound = true;
                        minedNonce = currentNonce.ToString() + " " + hashResult;
                    }
                }
                else
                {
                    //Console.WriteLine($"\u001b[1;31mThread {threadIndex}: Trying nonce: {currentNonce}, current hash: {hashResult}\u001b[0m"); //Removed cuz this makes the code slower 100x times
                }
            }
        });

        return minedNonce;
    }

    public static string CalculateSha256Hash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }

    public static void Main()
    {
        string blockHeader = "Block1234"; // Block header
        int difficulty = 6; // How much leading zeros
        string minedNonce = MineBitcoinHashes(blockHeader, difficulty);
        Console.WriteLine($"\n\u001b[1;32mMined nonce: {minedNonce}\u001b[0m");
    }
}
