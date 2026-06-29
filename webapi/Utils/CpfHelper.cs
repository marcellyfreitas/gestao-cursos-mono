namespace ApiSgc.Utils;

public class CpfHelper
{
    private static readonly Random _random = new Random();

    public static string Generate()
    {
        var cpf = new int[11];

        for (int i = 0; i < 9; i++)
        {
            cpf[i] = _random.Next(0, 10);
        }

        cpf[9] = CalculateVerificationDigit(cpf, 10);
        cpf[10] = CalculateVerificationDigit(cpf, 11);

        return Format(cpf);
    }

    public static bool IsValid(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        var digits = cpf.Where(char.IsDigit).Select(c => c - '0').ToArray();

        if (digits.Length != 11 || digits.All(d => d == digits[0]))
            return false;

        var dv1 = CalculateVerificationDigit(digits.Take(9).ToArray(), 10);
        if (digits[9] != dv1)
            return false;

        var dv2 = CalculateVerificationDigit(digits.Take(10).ToArray(), 11);
        return digits[10] == dv2;
    }

    private static int CalculateVerificationDigit(int[] cpf, int length)
    {
        int sum = 0;
        for (int i = 0; i < length - 1; i++)
        {
            sum += cpf[i] * (length - i);
        }

        int remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    private static string Format(int[] cpf)
    {
        return $"{cpf[0]}{cpf[1]}{cpf[2]}.{cpf[3]}{cpf[4]}{cpf[5]}.{cpf[6]}{cpf[7]}{cpf[8]}-{cpf[9]}{cpf[10]}";
    }
}
