using Jmas.FrenchDictionary;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

class CommandLineErrorMessage
{
    internal static void Error(string m)
    {
        Console.WriteLine(m);
    }
}
class Program
{
    static void Main(string[] argv)
    {
        var a = new FrenchService();
        var wrongArgv1 = false;
        if (argv.Length == 0)
            wrongArgv1 = true;
        else
        {
            if (argv[0] == "test")
            {
                var lexerStateString = a.Lexer.Machine.GetStatesString();
                var sr = new StreamWriter("FrenchTest.txt");
                var wrongArgv2 = false;
                var lex = a.Lexer;
                var par = a.Parser;
                var ann = a.Announcer;
                void testLexer()
                {
                    sr.Write(lexerStateString);
                    sr.WriteLine();
                    sr.WriteLine();
                    sr.Flush();
                }
                var parTestString = @"
oiseau wazo
Normandy nɔʁmɑ̃di
absence apsɑ̃:s
prier pʁije
construire kɔ̃stʁɥi:ʁ
silhouette silwɛt
sommeil sɔmɛ:j
famille fami:j
besoin bəzwɛ̃
un œ̃
restaurant ʁɛstɔʁɑ̃
maximum maksimɔm
chargement ʃaʁʒəmɑ̃
pâte pɑt
exister ɛgziste
effacer efase
errer ɛʁe
femme fam
maintenance mɛ̃tnɑ̃:s
enfer ɑ̃fɛ:ʁ
espoir ɛspwa:ʁ
des de
te tə
commencer kɔmɑ̃se
enterrer ɑ̃tɛʁe
destin dɛstɛ̃
attention atɑ̃sjɔ̃
question kɛstjɔ̃
accueil akœ:j
accident aksidɑ̃
émotion emosjɔ̃
revenir ʁəvni:ʁ
cher ʃɛ:ʁ
monsieur məsjø
compteur kɔ̃tœ:ʁ
eugénie øʒeni
cœur kœ:ʁ
emmener ɑ̃mne
fil fil
longtemp lɔ̃tɑ̃
piller pije
bayonette bajɔnɛt
bourgeon buʁʒɔ̃
enougueillir ɑ̃nugeji:ʁ
citoyen sitwajɛ̃
grêler gʁele
naïf naif
égoïste egɔist
août u
tranquille tʁɑ̃kil
gentilhomme ʒɑ̃tijɔm
moyen mwajɛ̃
cactus kaktys
néanmoins neɑ̃mwɛ̃
";
                void testParser()
                {
                    foreach (var line in parTestString.Split('\n'))
                    {
                        if (line.Length <= 2 || line.Substring(0, 2) == "//")
                            continue;
                        var q = line.Split(' ');
                        var word1 = q[0].ToLower();
                        var pron2 = q[1].Trim();
                        var fw = new FrenchWord(word1);
                        lex.FindAllCombs(fw);
                        par.Parse(fw);
                        sr.WriteLine($"{word1}   {fw.GetSyllableString()}");
                    }
                    sr.Flush();
                }
                void testAnnouncer()
                {
                    foreach (var line in parTestString.Split('\n'))
                    {
                        if (line.Length <= 2 || line.Substring(0, 2) == "//")
                            continue;
                        var q = line.Split(' ');
                        var word1 = q[0].ToLower().Trim();
                        var pron2 = q[1].Trim();
                        var fw = new FrenchWord(word1);
                        lex.FindAllCombs(fw);
                        par.Parse(fw);
                        ann.Announce(fw);
                        if (fw.Pron != pron2)
                            sr.WriteLine($"{word1}   {fw.Pron}   {pron2}");
                        sr.Flush();
                    }
                }
                if (argv.Length <= 1)
                    wrongArgv2 = true;
                else
                    switch (argv[1])
                    {
                        case "--lexer":
                            testLexer();
                            break;
                        case "--parse":
                            testParser();
                            break;
                        case "--announce":
                            testAnnouncer();
                            break;
                        case "--all":
                            testLexer();
                            testParser();
                            testAnnouncer();
                            break;
                        default:
                            wrongArgv2 = true;
                            break;
                    }
                if (wrongArgv2)
                    CommandLineErrorMessage.Error(@"wrong argument after command 'test'
    expect:
        --lexer
        --parse
        --announce
        --all
    ");
                sr.Close();
            }
            else if (argv[0] == "pron")
            {
                var word = argv[1].ToLower().Trim();
                var fword = new FrenchWord(word);
                Console.WriteLine($"{word}  {a.GetPron(fword)}");
            }
            else
                wrongArgv1 = true;
        }
        if (wrongArgv1)
            CommandLineErrorMessage.Error(@"French Service Usage:
    test        Run the test program of this exe.
    pron        Get the pronunciation of a French word.
    conj        Get the conjugation of a French verb.
");
    }
}