using NAudio.Wave;

string Read(string prompt)
{
    Console.Write($"{prompt} ");
    return Console.ReadLine();
}

Console.WriteLine("========================");
Console.WriteLine("= Welcome to BeatAudio =");
Console.WriteLine("=    Made by IKTeam    =");
Console.WriteLine("========================");
Console.WriteLine();
string filename = Read("Enter filename of a file to load, like .mp3, .wav etc:");
double bpm = double.Parse(Read("Enter BPM of a song:"));
double offset = double.Parse(Read("Enter beginning offset in ms, default is 0:"));

var afr = new AudioFileReader(filename);
var sampleProvider = afr.ToSampleProvider();
var wf = sampleProvider.WaveFormat;
var sr = wf.SampleRate; // sample rate (per second)
int channels = wf.Channels;

var spb = 60 / bpm; // seconds per beat
var sr_a = (int)Math.Round(sr * spb) * channels; // amount of samples per one beat

// ditch the offset!
if (offset != 0)
{
    var offsetSamples = (int)Math.Round(sr * (offset / 1000));
    var blackhole = new float[offsetSamples * channels]; sampleProvider.Read(blackhole, 0, blackhole.Length);
}

var sC = (int)Math.Ceiling((bpm / 60) * (afr.TotalTime.TotalSeconds - offset / 1000)); // calculate amount of BEATS there are

Console.WriteLine($"We had found a total of {sC} beats in a {afr.TotalTime.ToString(@"hh\:mm\:ss")} song");

var sampleBuffer = new float[sr_a];
List<Beat> beatHolder = new List<Beat>(sC);
for (int i = 0; i < sC; i++) // read all beats
{
    var bR = sampleProvider.Read(sampleBuffer, 0, sr_a);
    for (int z = bR; z < sr_a; z++) // we don't need remainders of the previous beat in the last beat of the song
    {
        sampleBuffer[z] = 0;
    }
    var beatBuffer = new float[sr_a];
    sampleBuffer.CopyTo(beatBuffer, 0);
    beatHolder.Add(new Beat() { samples = beatBuffer, position = i });
}

Console.WriteLine($"All the beats successfully loaded! Press ENTER to continue.");
Console.ReadLine();

while (true)
{
    Console.Clear();
    Console.WriteLine($"= BeatAudio by IKTeam =");
    Console.WriteLine($"-> File loaded: {Path.GetFileName(filename)}");
    Console.WriteLine($"-> Amount of beats loaded: {beatHolder.Count}{(beatHolder.Count != sC ? $" (originally {sC})" : "")}");
    Console.WriteLine();
    Console.WriteLine($"Possible actions:");
    Console.WriteLine($"1. Remove every Nth beat out of K beats.");
    Console.WriteLine($"2. Swap Nth beat with Kth beat out of C beats.");
    Console.WriteLine($"3. Double every Nth beat out of K beats.");
    Console.WriteLine($"4. Save the resulting file.");
    Console.WriteLine();
    string actionString = Read(">");
    int act = -1;
    if (!int.TryParse(actionString, out act) || act == -1) continue;

    switch (act)
    {
        case 1:
            Remove_UI(); break;
        case 2:
            Swap_UI(); break;
        case 3:
            Double_UI(); break;
        case 4:
            Save_UI(); break;
    }
}

void Remove_UI()
{
    Console.WriteLine();
    Console.WriteLine("= REMOVE GUIDE =");
    Console.WriteLine("You can split all the beats there are into groups of a fixed size C.");
    Console.WriteLine("Then, you can remove every Nth beat out of that group.");
    Console.WriteLine("For example, to remove every other beat, you need a group of 2 beats, and remove every 2nd beat.");
    Console.WriteLine("If you want to remove the 8th beat and leave all the 7 beats before it intact, you need a group of 8 beats, and remove every 8th beat.");
    Console.WriteLine();
    string beatsList = "";
    List<int> beats = new();
    while (true)
    {
        beatsList = Read("I want to remove the next beats (list of numbers, split with comma, like 1,2,3):");
        try
        {
            beatsList.Split(',').ToList().ForEach(z => beats.Add(int.Parse(z) - 1)); // we do - 1 to account for indexes beginning with 0
            break;
        } catch
        {

        }
    }
    int groupSize = 0;
    while (true)
    {
        groupSize = int.Parse(Read($"There should be this amount of beats in a group (at least {beats.Max()}!!):"));
        if (groupSize >= beats.Count) break;
    }

    Utils.RemoveNthBeats(ref beatHolder, beats.ToArray(), groupSize);

    Console.WriteLine($"Success! Press Enter to return.");
    Console.ReadLine();
}

void Swap_UI()
{
    Console.WriteLine();
    Console.WriteLine("= SWAP GUIDE =");
    Console.WriteLine("You can split all the beats there are into groups of a fixed size C.");
    Console.WriteLine("Then, you can swap two beats in that group, beat N and beat K.");
    Console.WriteLine("For example, if you want to swap every other beat, you need a group size of 2, and then just swap beat 1 with beat 2.");
    Console.WriteLine("If your group size is 3, swapping beats 1 and 3 will leave beat 2 intact.");
    Console.WriteLine("Having a group size of 4, then swapping beat 1 and beat 2, will mean that two beats after that (beat 3 and beat 4) will be intact");
    Console.WriteLine();
    int beatA = int.Parse(Read("I want to swap beat A numbered...:")) - 1;
    int beatB = int.Parse(Read("With beat B numbered...:")) - 1;
    int groupSize = 0;
    while (true)
    {
        groupSize = int.Parse(Read("In a group size of... (at least 2!):"));
        if (groupSize >= 2) break;
    }

    Utils.SwapBeats(ref beatHolder, beatA, beatB, groupSize);

    Console.WriteLine($"Successfully swapped beat {beatA+1} with beat {beatB+1} in a group of {groupSize} beats! Press Enter to return.");
    Console.ReadLine();
}

void Double_UI()
{

}

void Save_UI()
{
    Console.WriteLine();
    string output = Read("Filename of the result file:");

    var afw = new WaveFileWriter(output, new WaveFormat(sr, channels));
    Console.WriteLine($"Saving the file to {output}... Please wait!");
    var resultBuffer = new List<float>(beatHolder.Count * sr_a);
    beatHolder.ForEach((z) => { resultBuffer.AddRange(z.samples); });
    afw.WriteSamples(resultBuffer.ToArray(), 0, resultBuffer.Count);
    afw.Flush();
    afw.Close();
    Console.WriteLine($"Successfully written {resultBuffer.Count} samples to {output}! Press any key to return to main menu. You can now safely close the program, too.");
    Console.ReadLine();
}

public class Beat
{
    public int position;
    public float[] samples;
}