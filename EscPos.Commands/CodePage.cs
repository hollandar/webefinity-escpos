namespace EscPos.Commands;

public enum CodePage : byte
{
    // Common ESC/POS code page indices (support varies by printer/firmware)
    CP437_USA_StandardEurope = 0,
    Katakana = 1,
    CP850_Multilingual = 2,
    CP860_Portuguese = 3,
    CP863_CanadianFrench = 4,
    CP865_Nordic = 5,

    CP1252_WindowsLatin1 = 16,
    CP866_Cyrillic2 = 17,
    CP852_Latin2 = 18,
    CP858_Euro = 19,

    Thai42 = 20,
    Thai11 = 21,
    Thai13 = 22,
    Thai14 = 23,
    Thai16 = 24,
    Thai17 = 25,
    Thai18 = 26,

    CP874_Thai = 30,

    CP1250_WindowsCentralEurope = 48,
    CP1251_WindowsCyrillic = 49,
    CP1253_WindowsGreek = 50,
    CP1254_WindowsTurkish = 51,
    CP1255_WindowsHebrew = 52,
    CP1256_WindowsArabic = 53,
    CP1257_WindowsBaltic = 54,
    CP1258_WindowsVietnamese = 55,

    UTF8 = 255,
}
