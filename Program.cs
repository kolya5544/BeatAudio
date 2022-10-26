using NAudio.Wave;
using NAudio.Wave.SampleProviders;

string Read(string prompt)
{
    Console.Write($"{prompt} ");
    return Console.ReadLine();
}

int sC = 0;
int sr = 0;
int sr_a = 0;
int channels = 0;

List<Beat> LoadFile(string fn, double bpm, double offset)
{
    var afr = new AudioFileReader(fn);
    var sampleProvider = afr.ToSampleProvider();
    var wf = sampleProvider.WaveFormat;
    sr = wf.SampleRate; // sample rate (per second)
    channels = wf.Channels;

    var spb = 60 / bpm; // seconds per beat
    sr_a = (int)Math.Round(sr * spb) * channels; // amount of samples per one beat

    // ditch the offset!
    if (offset != 0)
    {
        var offsetSamples = (int)Math.Round(sr * (offset / 1000));
        var blackhole = new float[offsetSamples * channels]; sampleProvider.Read(blackhole, 0, blackhole.Length);
    }

    sC = (int)Math.Ceiling((bpm / 60) * (afr.TotalTime.TotalSeconds - offset / 1000)); // calculate amount of BEATS there are

    Console.WriteLine($"We had found a total of {sC} beats in a {afr.TotalTime.ToString(@"hh\:mm\:ss")} song");

    var sampleBuffer = new float[sr_a];
    var bh = new List<Beat>(sC);
    for (int i = 0; i < sC; i++) // read all beats
    {
        var bR = sampleProvider.Read(sampleBuffer, 0, sr_a);
        for (int z = bR; z < sr_a; z++) // we don't need remainders of the previous beat in the last beat of the song
        {
            sampleBuffer[z] = 0;
        }
        var beatBuffer = new float[sr_a];
        sampleBuffer.CopyTo(beatBuffer, 0);
        bh.Add(new Beat() { samples = beatBuffer, position = i });
    }

    return bh;
}

Console.WriteLine("========================");
Console.WriteLine("= Welcome to BeatAudio =");
Console.WriteLine("=    Made by IKTeam    =");
Console.WriteLine("========================");
Console.WriteLine();
string filename = Read("Enter filename of a file to load, like .mp3, .wav etc:");
double bpm = double.Parse(Read("Enter BPM of a song:"));
double offset = double.Parse(Read("Enter beginning offset in ms, sometimes is 0:"));

List<Beat> beatHolder = new();

beatHolder = LoadFile(filename, bpm, offset);

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
    Console.WriteLine($"4. Reverse the order of beats in groups of N beats.");
    Console.WriteLine($"5. Save the resulting file.");
    Console.WriteLine($"6. Re-load the file from disk.");
    Console.WriteLine($"7. Playback, BPM and offset adjust.");
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
            Reverse_UI(); break;
        case 5:
            Save_UI(); break;
        case 6:
            beatHolder = LoadFile(filename, bpm, offset); break;
        case 7:
            Adjust_UI(); break;
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
        groupSize = int.Parse(Read($"There should be this amount of beats in a group (at least {beats.Max()+1}!!):"));
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

void Reverse_UI()
{
    Console.WriteLine();
    Console.WriteLine("= REVERSE GUIDE =");
    Console.WriteLine("You can split all the beats there are into groups of a fixed size C.");
    Console.WriteLine("Then, you can reverse the order of these groups of beats, preserving the order of beats in the group.");
    Console.WriteLine("For example, if you want to reverse every beat (so beat 1 becomes beat N, beat 2 becomes beat N-1 etc), you need a group size of 1.");
    Console.WriteLine("And if you reverse in groups of 2, 1st beat will become N-1th, 2nd beat will become Nth, 3rd beat will become N-3th, and 4th beat will be N-2th...");
    Console.WriteLine("Group size 1 [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] -> [10, 9, 8, 7, 6, 5, 4, 3, 2, 1]");
    Console.WriteLine("Group size 2 [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] -> [9, 10, 7, 8, 5, 6, 3, 4, 1, 2]");
    Console.WriteLine("Group size 3 [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] -> [8, 9, 10, 5, 6, 7, 2, 3, 4, 1] and so on...");
    Console.WriteLine();
    int group = int.Parse(Read("I want to swap beats in a group of...:"));

    Utils.ReverseBeats(ref beatHolder, group);

    Console.WriteLine($"Successfully reversed beats in groups of {group}! Press Enter to return.");
    Console.ReadLine();
}

void Adjust_UI()
{
    Console.Clear();
    Console.WriteLine();
    Console.WriteLine("= ADJUST GUIDE =");
    Console.WriteLine("You'll be able to set BPM and offset, then listen to what the song sounds like with beat sounds added.");
    Console.WriteLine("This should be helpful to adjust your BPM and offset accordingly");
    Console.WriteLine("Keep in mind, changing BPM or offset of the song WILL make the program re-load it from file, which WILL lead to losing any changes.");
    Console.WriteLine();
    while (true)
    {
        Console.WriteLine($"-> Current BPM: {bpm}");
        Console.WriteLine($"-> Current offset: {offset}");
        Console.WriteLine();
        Console.WriteLine($"Possible actions:");
        Console.WriteLine($"1. Change BPM.");
        Console.WriteLine($"2. Change offset.");
        Console.WriteLine($"3. Play with beat sounds.");
        Console.WriteLine($"4. Return to main menu.");
        Console.WriteLine();
        string actionString = Read(">");
        int act = -1;
        if (!int.TryParse(actionString, out act) || act == -1) break;

        switch (act)
        {
            case 1:
                bpm = double.Parse(Read("Enter BPM of a song:"));
                beatHolder = LoadFile(filename, bpm, offset); break;
            case 2:
                offset = double.Parse(Read("Enter beginning offset in ms, sometimes is 0:"));
                beatHolder = LoadFile(filename, bpm, offset); break;
            case 3:
                Playback_UI(); break;
            case 4:
                return;
        }
        Console.Clear();
    }
}

void Playback_UI()
{
    Console.WriteLine();
    Console.WriteLine("Generating the playback file...");

    Console.WriteLine("Generating tick channel...");

    using (var ms = new MemoryStream())
    using (var afw = new WaveFileWriter(ms, new WaveFormat(sr, channels)))
    using (var msOut = new MemoryStream())
    using (var wfw = new WaveFileWriter(msOut, new WaveFormat(sr, channels)))
    using (var tick = new AudioFileReader("tick.wav"))
    {  // open metronome tick file
        var sampleProvider = tick.ToSampleProvider();
        var wf = sampleProvider.WaveFormat;
        var T_sr = wf.SampleRate; // sample rate (per second)
        var T_channels = wf.Channels;

        // basically...
        // we want to build a separate file with just tick sounds matching original BPM
        // so we can then combine input file and tick sound file to get tick sounds over original file.
        float[] rawSamples = new float[((int)tick.TotalTime.TotalSeconds + 1) * T_sr];
        int read = sampleProvider.Read(rawSamples, 0, rawSamples.Length);
        var allSamples = new float[read];
        Array.Copy(rawSamples, allSamples, read);

        

        var silence = new float[Math.Max(0, sr_a - allSamples.Length)]; // we need silence after each beat to last for the duration of the beat minus tick sound duration

        for (int i = 0; i < beatHolder.Count; i++)
        {
            afw.WriteSamples(allSamples, 0, allSamples.Length);
            afw.WriteSamples(silence, 0, silence.Length);
        }

        afw.Flush();

        Console.WriteLine("Generating default channel...");

        // now we should re-build our input back into WAV to combine files
        var resultBuffer = new List<float>(beatHolder.Count * sr_a);
        beatHolder.ForEach((z) => { resultBuffer.AddRange(z.samples); });
        wfw.WriteSamples(resultBuffer.ToArray(), 0, resultBuffer.Count);
        wfw.Flush();

        Console.WriteLine("Generating final result...");
        msOut.Position = 0;
        ms.Position = 0;
        using (var readerOriginal = new WaveFileReader(msOut))
        using (var readerTick = new WaveFileReader(ms))
        {
            var wcO = new SampleChannel(readerOriginal, true);
            wcO.Volume = 0.5f;
            var wcT = new SampleChannel(readerTick, true);
            wcT.Volume = 2f;
            var mixer = new MixingSampleProvider(new[] { wcO.ToStereo(), wcT.ToStereo() });

            Console.WriteLine("Playing final result... Press ENTER to stop the playback.");
            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(mixer.ToStereo());
                outputDevice.Play();
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    Console.ReadLine();
                    outputDevice.Stop();
                }
            }
            //mixer.Read(resultSamples, 0, resultSamples.Length);
            //WaveFileWriter.CreateWaveFile16("mixed.wav", mixer);
        }
    }
}

void Double_UI()
{
    Console.WriteLine();
    Console.WriteLine("= DOUBLE GUIDE =");
    Console.WriteLine("You can split all the beats there are into groups of a fixed size C.");
    Console.WriteLine("Then, you can duplicate beats by their number in that group. Duplicating means the beat will play twice.");
    Console.WriteLine("For example, if you want to double every beat of a song, you double beat 1 in a group of 1.");
    Console.WriteLine("To double every other beat, you need a group of 2, and double beat 2 (or beat 1 - whatever you prefer)");
    Console.WriteLine("If you double two beats (say, beat 3 and beat 4) in a group of 4 beats, first two beats (beat 1 and beat 2) will be intact.");
    Console.WriteLine();
    string beatsList = "";
    List<int> beats = new();
    while (true)
    {
        beatsList = Read("I want to duplicate the next beats (list of numbers, split with comma, like 1,2,3):");
        try
        {
            beatsList.Split(',').ToList().ForEach(z => beats.Add(int.Parse(z) - 1)); // we do - 1 to account for indexes beginning with 0
            break;
        }
        catch
        {

        }
    }
    int groupSize = 0;
    while (true)
    {
        groupSize = int.Parse(Read($"There should be this amount of beats in a group (at least {beats.Max()+1}!!):"));
        if (groupSize >= beats.Count) break;
    }

    Utils.DoubleBeats(ref beatHolder, beats.ToArray(), groupSize);

    Console.WriteLine($"Success! Press Enter to return.");
    Console.ReadLine();
}

void Save_UI()
{
    Console.WriteLine();
    string output = Read("Filename of the result file:");
    string compression = Read("Apply MP3 compression to the result? (y/N):");

    bool compress = compression.ToLower() == "y";

    Console.WriteLine($"Saving the file to {output}... Please wait!");
    var resultBuffer = new List<float>(beatHolder.Count * sr_a);
    beatHolder.ForEach((z) => { resultBuffer.AddRange(z.samples); });
    if (!compress)
    {
        var afw = new WaveFileWriter(output, new WaveFormat(sr, channels));
        afw.WriteSamples(resultBuffer.ToArray(), 0, resultBuffer.Count);
        afw.Flush();
        afw.Close();
    } else
    {
        var ms = new MemoryStream();
        var afw = new WaveFileWriter(ms, new WaveFormat(sr, channels));
        afw.WriteSamples(resultBuffer.ToArray(), 0, resultBuffer.Count);
        afw.Flush();
        ms.Position = 0;
        File.WriteAllBytes(output, Utils.wavMp3(ms));
    }

    Console.WriteLine($"Successfully written {resultBuffer.Count} samples to {output}! Press ENTER to return to main menu. You can now safely close the program, too.");
    Console.ReadLine();
}

public class Beat
{
    public int position;
    public float[] samples;
}