// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Hashing;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using HentaiMapGenLib.Models;
using HentaiMapGenLib.Models.Json;
using MessagePack;
using Microsoft.Collections.Extensions;
using SQLite;
using ZstdNet;

namespace HentaiMapGen;

internal static partial class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        
        var nHentaiSerializer = NHentaiSerializer.Default;
        
        var dataFolder = args[1];
        var outputFolder = $@"{args[1]}\Output";

        #region Load deletedGalleries
        
        Dictionary<uint, DeletedGallery> deletedGalleries;
        await using (var stream = File.OpenRead($@"{dataFolder}\removed_galleries.json"))
        {
            deletedGalleries = JsonSerializer.Deserialize(stream, nHentaiSerializer.DeletedGalleryArray)!
                .ToDictionary(e => e.Id);
        }

        Console.WriteLine("Loaded deletedGalleries");
        
        #endregion

        using var sadPandaNameMapping = await SadPandaNameMapping.LoadAsync(args[0], dataFolder);

        using var nhentaiDb = new SQLiteConnection($@"{dataFolder}\manga.db", SQLiteOpenFlags.ReadOnly);

        var msgpackTotal = TimeSpan.Zero;
        var mappingTotal = TimeSpan.Zero;

        var galleries = new List<Book>();

        // Mapping of NHentai ID -> SadPanda Gallery ID and Token
        var nhentaiMapping = new DictionarySlim<uint, SadPandaIdToken>();

        // Mapping of NHentai ID -> NHentai Title, for when no match was found
        var naGalleries = new DictionarySlim<uint, Title>();

        // Galleries that returned an error (probably removed by NHentai) and were not matched in removed_galleries.json
        var erroredGalleries = new List<uint>();

        var sw0 = Stopwatch.GetTimestamp();

        var i = 0;
        foreach (var kv in nhentaiDb.Table<NHentaiKeyValue>().Deferred())
        {
            var galleryId = uint.Parse(kv.Key);
            var value = kv.Value;

            try
            {
                if (i++ % 1000 == 0)
                {
                    Console.WriteLine($"{i}");
                }

                var nGallery = JsonSerializer.Deserialize<Book>(value.AsSpan().RemovePrefix("json:"))!;

                nGallery = nGallery with
                {
                    Id = galleryId,
                };

                #region Write memorypack

                {
                    var sw1 = Stopwatch.GetTimestamp();
                    galleries.Add(nGallery);
                    msgpackTotal += Stopwatch.GetElapsedTime(sw1);
                }

                #endregion

                #region Map Nhentai to sadpanda
                
                var sw2 = Stopwatch.GetTimestamp();

                if (nGallery.Error != null)
                {
                    // Handle missing gallery
                    
                    if (!deletedGalleries.TryGetValue(galleryId, out var deletedGallery))
                    {
                        erroredGalleries.Add(galleryId);
                        continue;
                    }

                    if (sadPandaNameMapping.FindMapping([deletedGallery.TitleEnglish, deletedGallery.TitlePretty], out var pGallery))
                    {
                        // Console.WriteLine($"{deletedGallery.Title}: https://exhentai.org/g/{pGallery.Gid}/{pGallery.Token}/");
                        nhentaiMapping.GetOrAddValueRef(galleryId) = new SadPandaIdToken(pGallery.Gid, pGallery.Token!);
                    }
                    else
                    {
                        Console.WriteLine($"{deletedGallery.TitlePretty}: N/A");
                        naGalleries.GetOrAddValueRef(galleryId) = new Title(deletedGallery.TitleEnglish, null, deletedGallery.TitlePretty);
                    }
                }
                else
                {
                    // Handle present gallery

                    if (sadPandaNameMapping.FindMapping([nGallery.Title.English, nGallery.Title.Japanese, nGallery.Title.Pretty], out var pGallery))
                    {
                        // Console.WriteLine($"{nGallery.Title.Pretty}: https://exhentai.org/g/{pGallery.Gid}/{pGallery.Token}/");
                        nhentaiMapping.GetOrAddValueRef(galleryId) = new SadPandaIdToken(pGallery.Gid, pGallery.Token!);
                    }
                    else
                    {
                        Console.WriteLine($"{nGallery.Title.Pretty}: N/A");
                        naGalleries.GetOrAddValueRef(galleryId) = nGallery.Title;
                    }
                }
                
                mappingTotal += Stopwatch.GetElapsedTime(sw2);
                
                #endregion
            }
            catch (Exception)
            {
                Console.WriteLine($"ERRORED when processing gallery {galleryId}: {value}");
                throw;
            }
        }

        // await fileStream.WriteAsync(arrWriter.WrittenMemory);

        Console.WriteLine($"Total time: {Stopwatch.GetElapsedTime(sw0)}");
        Console.WriteLine($"MemoryPack took {msgpackTotal}");
        Console.WriteLine($"Mapping nhentai->panda took {mappingTotal}");
        Console.WriteLine($"Found NHentai->Panda mappings: {nhentaiMapping.Count}");
        Console.WriteLine($"Not found NHentai->Panda mappings: {naGalleries.Count}");
        Console.WriteLine($"Errored (deleted, etc...) NHentai galleries: {erroredGalleries.Count}");

        await using var fileStream = File.Create($@"{outputFolder}\galleries.msgpack.zst");
        await using var msgpackOutput = new CompressionStream(fileStream, new CompressionOptions(8));
        await MessagePackSerializer.SerializeAsync(fileStream, galleries);

        await using (var stream = File.Create($"{outputFolder}/nhentaiMapping.json"))
        {
            await JsonSerializer.SerializeAsync(stream, nhentaiMapping, NHentaiSerializer.Default.Options);
        }
        await using (var stream = File.Create($"{outputFolder}/naGalleries.json"))
        {
            await JsonSerializer.SerializeAsync(stream, naGalleries, NHentaiSerializer.Default.Options);
        }
        await using (var stream = File.Create($"{outputFolder}/erroredGalleries.json"))
        {
            await JsonSerializer.SerializeAsync(stream, erroredGalleries, NHentaiSerializer.Default.Options);
        }
    }
}

internal partial class SadPandaNameMapping(SQLiteConnection sqliteConnection, Dictionary<ulong, SadPandaIdToken> sadPandaNameHashes) : IDisposable
{
    private readonly TableQuery<SadPandaGallery> _sadPandaGalleries = sqliteConnection.Table<SadPandaGallery>();
    
    public static async Task<SadPandaNameMapping> LoadAsync(string pandaDbLocation, string dataFolder)
    {
        var pandaDb = new SQLiteConnection(pandaDbLocation, SQLiteOpenFlags.ReadOnly);

        Dictionary<ulong, SadPandaIdToken> sadPandaNameHashes;
        if (!File.Exists($@"{dataFolder}\sadPandaNameHashes.msgpack.zst"))
        {
            sadPandaNameHashes = new Dictionary<ulong, SadPandaIdToken>();
            var sw = Stopwatch.GetTimestamp();

            foreach (var e in pandaDb.Table<SadPandaGallery>().Deferred())
            {
                var urlParts = new SadPandaIdToken(e.Gid, e.Token!);

                if (!string.IsNullOrEmpty(e.Title))
                {
                    foreach (var candidate in GetCandidates(e.Title))
                    {
                        sadPandaNameHashes.TryAdd(Hash(candidate), urlParts);
                    }
                }

                if (!string.IsNullOrEmpty(e.TitleJpn))
                {
                    foreach (var candidate in GetCandidates(e.TitleJpn))
                    {
                        sadPandaNameHashes.TryAdd(Hash(candidate), urlParts);
                    }
                }
            }

            Console.WriteLine($"Created sadPandaNameHashes in {Stopwatch.GetElapsedTime(sw)}. Total {sadPandaNameHashes.Count} sadpanda name hashes.");

            await using var fileStream = File.Create($@"{dataFolder}\sadPandaNameHashes.msgpack.zst");
            await using var stream = new CompressionStream(fileStream, new CompressionOptions(19));
            await MessagePackSerializer.SerializeAsync(stream, sadPandaNameHashes);
        }
        else
        {
            await using var fileStream = File.OpenRead($@"{dataFolder}\sadPandaNameHashes.msgpack.zst");
            await using var stream = new DecompressionStream(fileStream);
            sadPandaNameHashes = (await MessagePackSerializer.DeserializeAsync<Dictionary<ulong, SadPandaIdToken>>(stream))!;

            Console.WriteLine("Loaded sadPandaNameHashes from cache");
        }

        return new SadPandaNameMapping(pandaDb, sadPandaNameHashes);
    }

    private static CandidatesEnumerator GetCandidates(string str) => new(str);
    private struct CandidatesEnumerator(string str)
    {
        private int _index = -1;

        public bool MoveNext()
        {
            _index++;
            return _index < 3;
        }

        public readonly ReadOnlySpan<char> Current => _index switch
        {
            0 => Normalize(str),
            1 => Normalize(Cleanup(str)),
            2 => Normalize(ReplaceRoman(Cleanup(str))),
            _ => throw new ArgumentOutOfRangeException(nameof(_index), _index, "Enumeration has finished"),
        };

        public readonly string[] ToArray()
        {
            return
            [
                Normalize(str),
                Normalize(Cleanup(str)),
                Normalize(ReplaceRoman(Cleanup(str))),
            ];
        }

        public CandidatesEnumerator GetEnumerator() => this;
    }

    private bool TryMatchAnyHash(ReadOnlySpan<string?> candidates, [MaybeNullWhen(false)] out SadPandaIdToken parts, [MaybeNullWhen(false)] out ReadOnlySpan<char> match, out ulong matchedHash)
    {
        foreach (var candidate in candidates)
        {
            if (candidate == null) continue;

            foreach (var subcandidate in GetCandidates(candidate))
            {
                if (sadPandaNameHashes.TryGetValue(matchedHash = Hash(match = subcandidate), out parts)) return true;
            }
        }

        matchedHash = 0;
        match = default;
        parts = default;
        return false;
    }

    private static bool CheckIfDbEntryMatches(SadPandaGallery pGallery, ReadOnlySpan<string?> candidates)
    {
        var titleEngs = pGallery.Title != null ? GetCandidates(pGallery.Title).ToArray() : [];
        var titleJpns = pGallery.TitleJpn != null ? GetCandidates(pGallery.TitleJpn).ToArray() : [];

        foreach (var candidate in candidates)
        {
            if (candidate == null) continue;

            var i = 0;
            foreach (var subcandidate in GetCandidates(candidate))
            {
                if (titleEngs.Length > i && subcandidate.SequenceEqual(titleEngs[i])) return true;
                if (titleJpns.Length > i && subcandidate.SequenceEqual(titleJpns[i])) return true;
                i++;
            }
        }

        return false;
    }

    private TableMapping? _tableMapping = null;
    public bool FindMapping(ReadOnlySpan<string?> candidates, [MaybeNullWhen(false)] out SadPandaGallery pGallery)
    {
        if (TryMatchAnyHash(candidates, out var pandaId, out var match, out var matchedHash))
        {
            var gid = pandaId.Gid;

            pGallery = _sadPandaGalleries.Connection
                .CreateCommand("select * from gallery where gid = ? limit 1", gid)
                .ExecuteDeferredQuery<SadPandaGallery>(_tableMapping ??= _sadPandaGalleries.Connection.GetMapping(typeof(SadPandaGallery)))
                .FirstOrDefault();

            if (!CheckIfDbEntryMatches(pGallery, candidates))
            {
                throw new InvalidOperationException($"Hash collision! {pGallery.Title} vs {candidates[0]} for match {match} (hash {matchedHash})");
            }

            return true;
        }

        pGallery = default;
        return false;
    }
    
    [GeneratedRegex("[0-9]+", RegexOptions.Compiled)]
    private static partial Regex NumberRegex();
    // private static readonly SearchValues<char> IsNumberSearchValues = SearchValues.Create(['0', '1', '2', '3', '4', '5', '6', '7', '8', '9']);
    private static string ReplaceRoman(string str)
    {
        // const int stackallocThreshold = 512;
        //
        // var sb = new DefaultInterpolatedStringHandler(0, 0, null, stackalloc char[Math.Min((int)(str.Length * 1.2), stackallocThreshold)]);
        //
        // while (str.IndexOfAny(IsNumberSearchValues) is var idx and not -1)
        // {
        //     sb.AppendFormatted(str[..idx]);
        //
        //     var nextNonDigitCharacter = str[(idx + 1)..].IndexOfAnyExcept(IsNumberSearchValues);
        //
        //     if (nextNonDigitCharacter != -1)
        //     {
        //         var end = idx + 1 + nextNonDigitCharacter;
        //         AppendRoman(str[idx..end], ref sb);
        //         str = str[(end + 1)..];
        //     }
        //     else
        //     {
        //         str = str[(idx + 1)..];
        //         break;
        //     }
        //
        //     continue;
        //
        //     static void AppendRoman(ReadOnlySpan<char> str, ref DefaultInterpolatedStringHandler sb)
        //     {
        //         str = str.TrimStart('0');
        //         if (str is []) return;
        //
        //         if (!long.TryParse(str, out var number))
        //         {
        //             sb.AppendFormatted(str);
        //             return;
        //         }
        //
        //         switch (number)
        //         {
        //             case >= 4000:
        //                 sb.AppendFormatted(str);
        //                 return;
        //             case < 1:
        //                 return;
        //         }
        //
        //         while (number >= 1)
        //         {
        //             switch (number)
        //             {
        //                 case >= 1000: sb.AppendLiteral("M"); number -= 1000; break;
        //                 case >= 900: sb.AppendLiteral("CM"); number -= 900; break;
        //                 case >= 500: sb.AppendLiteral("D"); number -= 500; break;
        //                 case >= 400: sb.AppendLiteral("CD"); number -= 400; break;
        //                 case >= 100: sb.AppendLiteral("C"); number -= 100; break;
        //                 case >= 90: sb.AppendLiteral("XC"); number -= 90; break;
        //                 case >= 50: sb.AppendLiteral("L"); number -= 50; break;
        //                 case >= 40: sb.AppendLiteral("XL"); number -= 40; break;
        //                 case >= 10: sb.AppendLiteral("X"); number -= 10; break;
        //                 case >= 9: sb.AppendLiteral("IX"); number -= 9; break;
        //                 case >= 5: sb.AppendLiteral("V"); number -= 5; break;
        //                 case >= 4: sb.AppendLiteral("IV"); number -= 4; break;
        //                 default: sb.AppendLiteral("I"); number -= 1; break;
        //             }
        //         }
        //     }
        // }
        //
        // sb.AppendFormatted(str);
        //
        // return sb.ToStringAndClear();
        
        return NumberRegex().Replace(str, static e => ToRoman(e.ValueSpan));
    }
    
    private static string ToRoman(ReadOnlySpan<char> str)
    {
        str = str.TrimStart('0');
        if (str is []) return string.Empty;

        if (!long.TryParse(str, out var number))
            return new string(str);

        switch (number)
        {
            case >= 4000:
                return new string(str);
            case < 1:
                return string.Empty;
        }

        var sb = new DefaultInterpolatedStringHandler(0, 0, null, stackalloc char["MMMCMXCIX".Length]);

        while (number >= 1)
        {
            switch (number)
            {
                case >= 1000: sb.AppendLiteral("M"); number -= 1000; break;
                case >= 900: sb.AppendLiteral("CM"); number -= 900; break;
                case >= 500: sb.AppendLiteral("D"); number -= 500; break;
                case >= 400: sb.AppendLiteral("CD"); number -= 400; break;
                case >= 100: sb.AppendLiteral("C"); number -= 100; break;
                case >= 90: sb.AppendLiteral("XC"); number -= 90; break;
                case >= 50: sb.AppendLiteral("L"); number -= 50; break;
                case >= 40: sb.AppendLiteral("XL"); number -= 40; break;
                case >= 10: sb.AppendLiteral("X"); number -= 10; break;
                case >= 9: sb.AppendLiteral("IX"); number -= 9; break;
                case >= 5: sb.AppendLiteral("V"); number -= 5; break;
                case >= 4: sb.AppendLiteral("IV"); number -= 4; break;
                default: sb.AppendLiteral("I"); number -= 1; break;
            }
        }

        return sb.ToStringAndClear();
    }

    private static ulong Hash(ReadOnlySpan<char> str)
        => XxHash3.HashToUInt64(MemoryMarshal.Cast<char, byte>(str));

    private static string Normalize(string str)
    {
        // const int stackallocThreshold = 512;
        //
        // Span<char> buffer = stackalloc char[stackallocThreshold];
        //
        // var normalized = NormalizeImpl(str, buffer, NormalizationForm.FormD, out var toReturn, out var isAsciiString);
        //
        // char[]? toReturn2 = null;
        // var toLowerDestBuff = normalized.Length < stackallocThreshold
        //     ? toReturn != null ? buffer : stackalloc char[normalized.Length]
        //     : (toReturn2 = ArrayPool<char>.Shared.Rent(normalized.Length));
        //
        // int charsWritten;
        // if (isAsciiString)
        // {
        //     Ascii.ToLower(normalized, toLowerDestBuff, out charsWritten);
        //     // don't need to check charsWritten, success is guaranteed if dest.Length >= src.Length
        // }
        // else
        // {
        //     charsWritten = normalized.ToLowerInvariant(toLowerDestBuff);
        //
        //     if (charsWritten == -1)
        //     {
        //         if (toReturn2 != null) ArrayPool<char>.Shared.Return(toReturn2);
        //         if (toReturn != null) ArrayPool<char>.Shared.Return(toReturn);
        //
        //         throw new InvalidOperationException("Could not lower normalized string");
        //     }
        // }
        //
        // var outArray = new string(toLowerDestBuff[..charsWritten].Trim());
        //
        // if (toReturn2 != null) ArrayPool<char>.Shared.Return(toReturn2);
        // if (toReturn != null) ArrayPool<char>.Shared.Return(toReturn);
        //
        // return outArray;
        //
        // static ReadOnlySpan<char> NormalizeImpl(ReadOnlySpan<char> str, Span<char> buffer512, NormalizationForm normalizationForm, out char[]? toReturn, out bool isAsciiString)
        // {
        //     toReturn = null;
        //     
        //     // ReSharper disable once AssignmentInConditionalExpression
        //     if (isAsciiString = Ascii.IsValid(str))
        //     {
        //         // If its ASCII && one of the 4 main forms, then its already normalized
        //         if (normalizationForm is NormalizationForm.FormC or NormalizationForm.FormKC or NormalizationForm.FormD or NormalizationForm.FormKD)
        //             return str;
        //     }
        //     
        //     ValidateArguments(str, normalizationForm);
        //
        //     if (IcuIsNormalized(str, normalizationForm))
        //     {
        //         return str;
        //     }
        //     
        //     return IcuNormalize(str, buffer512, normalizationForm, out toReturn);
        //     
        //     static void ValidateArguments(ReadOnlySpan<char> strInput, NormalizationForm normalizationForm)
        //     {
        //         Debug.Assert(strInput != null);
        //
        //         if (OperatingSystem.IsBrowser() && normalizationForm is NormalizationForm.FormKC or NormalizationForm.FormKD)
        //         {
        //             // Browser's ICU doesn't contain data needed for FormKC and FormKD
        //             throw new PlatformNotSupportedException();
        //         }
        //
        //         if (normalizationForm != NormalizationForm.FormC && normalizationForm != NormalizationForm.FormD &&
        //             normalizationForm != NormalizationForm.FormKC && normalizationForm != NormalizationForm.FormKD)
        //         {
        //             throw new ArgumentException("Argument_InvalidNormalizationForm", nameof(normalizationForm));
        //         }
        //
        //         if (HasInvalidUnicodeSequence(strInput))
        //         {
        //             throw new ArgumentException("Argument_InvalidCharSequenceNoIndex", nameof(strInput));
        //         }
        //     }
        //
        //     // ICU does not signal an error during normalization if the input string has invalid unicode,
        //     // unlike Windows (which uses the ERROR_NO_UNICODE_TRANSLATION error value to signal an error).
        //     //
        //     // We walk the string ourselves looking for these bad sequences so we can continue to throw
        //     // ArgumentException in these cases.
        //     static bool HasInvalidUnicodeSequence(ReadOnlySpan<char> s)
        //     {
        //         for (int i = 0; i < s.Length; i++)
        //         {
        //             char c = s[i];
        //
        //             if (c < '\ud800')
        //             {
        //                 continue;
        //             }
        //
        //             if (c == '\uFFFE')
        //             {
        //                 return true;
        //             }
        //
        //             // If we see low surrogate before a high one, the string is invalid.
        //             if (char.IsLowSurrogate(c))
        //             {
        //                 return true;
        //             }
        //
        //             if (char.IsHighSurrogate(c))
        //             {
        //                 if (i + 1 >= s.Length || !char.IsLowSurrogate(s[i + 1]))
        //                 {
        //                     // A high surrogate at the end of the string or a high surrogate
        //                     // not followed by a low surrogate
        //                     return true;
        //                 }
        //                 else
        //                 {
        //                     i++; // consume the low surrogate.
        //                     continue;
        //                 }
        //             }
        //         }
        //
        //         return false;
        //     }
        //
        //     static unsafe bool IcuIsNormalized(ReadOnlySpan<char> strInput, NormalizationForm normalizationForm)
        //     {
        //         [DllImport("System.Globalization.Native", EntryPoint = "GlobalizationNative_IsNormalized", CharSet = CharSet.Unicode)]
        //         static extern int IsNormalized(NormalizationForm normalizationForm, char* src, int srcLen);
        //
        //         // Debug.Assert(!GlobalizationMode.Invariant);
        //         // Debug.Assert(!GlobalizationMode.UseNls);
        //
        //         int ret;
        //         fixed (char* pInput = strInput)
        //         {
        //             ret = IsNormalized(normalizationForm, pInput, strInput.Length);
        //         }
        //
        //         if (ret == -1)
        //         {
        //             throw new ArgumentException("Argument_InvalidCharSequenceNoIndex", nameof(strInput));
        //         }
        //
        //         return ret == 1;
        //     }
        //
        //     static unsafe ReadOnlySpan<char> IcuNormalize(ReadOnlySpan<char> strInput, Span<char> buffer512, NormalizationForm normalizationForm, out char[]? toReturn)
        //     {
        //         [DllImport("System.Globalization.Native", EntryPoint = "GlobalizationNative_NormalizeString", CharSet = CharSet.Unicode)]
        //         static extern int NormalizeString(
        //             NormalizationForm normalizationForm,
        //             char* src,
        //             int srcLen,
        //             char* dstBuffer,
        //             int dstBufferCapacity);
        //         
        //         // Debug.Assert(!GlobalizationMode.Invariant);
        //         // Debug.Assert(!GlobalizationMode.UseNls);
        //
        //         toReturn = null;
        //
        //         Span<char> buffer = strInput.Length <= stackallocThreshold ? buffer512 : (toReturn = ArrayPool<char>.Shared.Rent(strInput.Length));
        //
        //         for (int attempt = 0; attempt < 2; attempt++)
        //         {
        //             int realLen;
        //             fixed (char* pInput = strInput)
        //             fixed (char* pDest = &MemoryMarshal.GetReference(buffer))
        //             {
        //                 realLen = NormalizeString(normalizationForm, pInput, strInput.Length, pDest, buffer.Length);
        //             }
        //
        //             if (realLen == -1)
        //             {
        //                 throw new ArgumentException("Argument_InvalidCharSequenceNoIndex", nameof(strInput));
        //             }
        //
        //             if (realLen <= buffer.Length)
        //             {
        //                 ReadOnlySpan<char> result = buffer[..realLen];
        //                 return result.SequenceEqual(strInput) ? strInput : result;
        //             }
        //
        //             Debug.Assert(realLen > stackallocThreshold);
        //
        //             if (attempt == 0)
        //             {
        //                 if (toReturn != null)
        //                 {
        //                     // Clear toReturn first to ensure we don't return the same buffer twice
        //                     char[] temp = toReturn;
        //                     toReturn = null;
        //                     ArrayPool<char>.Shared.Return(temp);
        //                 }
        //
        //                 buffer = toReturn = ArrayPool<char>.Shared.Rent(realLen);
        //             }
        //         }
        //
        //         throw new ArgumentException("Argument_InvalidCharSequenceNoIndex", nameof(strInput));
        //     }
        // }

        return str.Normalize(NormalizationForm.FormD).ToLowerInvariant().Trim();
    }

    [GeneratedRegex(@"[\p{P}\p{S}\p{M}\p{C}\s]", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex PunctuationEtcRegex();

    private static string Cleanup(string str)
    {
        // const int stackallocThreshold = 512;
        //
        // var sb = new DefaultInterpolatedStringHandler(0, 0, null, stackalloc char[Math.Min(str.Length, stackallocThreshold)]);
        //
        // while (str.IndexOfWhitespaceOrAnyPunctuation() is var idx and not -1)
        // {
        //     sb.AppendFormatted(str[..idx]);
        //     str = str[(idx + 1)..];
        // }
        //
        // sb.AppendFormatted(str);
        //
        // return sb.ToStringAndClear();

        return PunctuationEtcRegex().Replace(str, "");
    }

    public void Dispose()
    {
        sqliteConnection.Dispose();
    }
}

// file static class PunctuationEtcHelper 
// {
//     /// <summary>Finds the next index of any character that matches a character in the set [\p{P}\p{S}\p{M}\p{C}\s].</summary>
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     internal static int IndexOfWhitespaceOrAnyPunctuation(this ReadOnlySpan<char> span)
//     {
//         int i = span.IndexOfAnyExcept(AsciiLettersAndDigits);
//         if ((uint)i < (uint)span.Length)
//         {
//             if (char.IsAscii(span[i]))
//             {
//                 return i;
//             }
//
//             do
//             {
//                 char ch;
//                 if (((ch = span[i]) < 128 ? ("\uffff\uffff\uffffﰀ\u0001\u0001"[ch >> 4] & (1 << (ch & 0xF))) != 0 : RegexRunner.CharInClass(ch, "\0\0\u001c\0\u0013\u0014\u0016\u0019\u0015\u0018\u0017\0\0\u001b\u001c\u001a\u001d\0\0\a\b\u0006\0\0\u000f\u0010\u001e\u0012\u0011\0d")))
//                 {
//                     return i;
//                 }
//                 i++;
//             }
//             while ((uint)i < (uint)span.Length);
//         }
//     
//         return -1;
//     }
//     
//     /// <summary>Supports searching for characters in or not in "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".</summary>
//     private static readonly SearchValues<char> AsciiLettersAndDigits = SearchValues.Create("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
// }